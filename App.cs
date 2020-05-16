using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TTNet.Data
{
    /// <summary>
    /// A The Things Network Application Data connection.
    /// </summary>
    public class App : IDisposable
    {
        private readonly string ClientID;
        private IMqttClient MqttClient;

        private JsonSerializerOptions SerializerOptions;
        private JsonDocumentOptions DocumentOptions;

        /// <summary>
        /// Application identifier.
        /// </summary>
        public string AppID { get; private set; }

        /// <summary>
        /// Value indicating whether this <see cref="T:TTNet.Data.App"/> is connected.
        /// </summary>
        public bool IsConnected => MqttClient.IsConnected;

        #region Events
        // If connected, manage MQTT subscriptions when a handler is added or removed

        private event EventHandler<MessageReceivedEventArgs> MessageReceivedField;
        private event EventHandler<UpdateReceivedEventArgs> MessageScheduledReceivedField;
        private event EventHandler<MessageInfoReceivedEventArgs> MessageSentReceivedField;
        private event EventHandler<UpdateReceivedEventArgs> MessageAckReceivedField;
        private event EventHandler<ActivationReceivedEventArgs> ActivationReceivedField;
        private event EventHandler<UpdateReceivedEventArgs> DeviceCreatedField;
        private event EventHandler<UpdateReceivedEventArgs> DeviceUpdatedField;
        private event EventHandler<UpdateReceivedEventArgs> DeviceDeletedField;
        private event EventHandler<ErrorReceivedEventArgs> ErrorReceivedField;

        /// <summary>
        /// Occurs when connection is completed.
        /// </summary>
        public event EventHandler<MqttClientConnectedEventArgs> Connected;
        /// <summary>
        /// Occurs when disconnected.
        /// </summary>
        public event EventHandler<MqttClientDisconnectedEventArgs> Disconnected;
        /// <summary>
        /// Occurs when a exception is throwed.
        /// </summary>
        public event EventHandler<Exception> ExceptionThrowed;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = MessageReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/up"));
                }
                MessageReceivedField += value;
            }
            remove
            {
                Delegate[] list;
                MessageReceivedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = MessageReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/up"));
                }
            }
        }

        /// <summary>
        /// Occurs when message scheduled to be sent to a device.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> MessageScheduledReceived
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = MessageScheduledReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/scheduled"));
                }
                MessageScheduledReceivedField += value;
            }
            remove
            {
                Delegate[] list;
                MessageScheduledReceivedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = MessageScheduledReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/scheduled"));
                }
            }
        }

        /// <summary>
        /// Occurs when a message is sent to a device.
        /// </summary>
        public event EventHandler<MessageInfoReceivedEventArgs> MessageSentReceived
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = MessageSentReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/sent"));
                }
                MessageSentReceivedField += value;
            }
            remove
            {
                Delegate[] list;
                MessageSentReceivedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = MessageSentReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/sent"));
                }
            }
        }

        /// <summary>
        /// Occurs when an ack sent by a device is received.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> MessageAckReceived
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = MessageAckReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/acks"));
                }
                MessageAckReceivedField += value;
            }
            remove
            {
                Delegate[] list;
                MessageAckReceivedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = MessageAckReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/acks"));
                }
            }
        }

        /// <summary>
        /// Occurs when a device is activated.
        /// </summary>
        public event EventHandler<ActivationReceivedEventArgs> ActivationReceived
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = ActivationReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations"));
                }
                ActivationReceivedField += value;
            }
            remove
            {
                Delegate[] list;
                ActivationReceivedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = ActivationReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/activations"));
                }
            }
        }

        /// <summary>
        /// Occurs when a device is created.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> DeviceCreated
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = DeviceCreatedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/create"));
                }
                DeviceCreatedField += value;
            }
            remove
            {
                Delegate[] list;
                DeviceCreatedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = DeviceCreatedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/create"));
                }
            }
        }

        /// <summary>
        /// Occurs when a device is updated.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> DeviceUpdated
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = DeviceUpdatedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/update"));
                }
                DeviceUpdatedField += value;
            }
            remove
            {
                Delegate[] list;
                DeviceUpdatedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = DeviceUpdatedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/update"));
                }
            }
        }

        /// <summary>
        /// Occurs when a device is deleted.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> DeviceDeleted
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = DeviceDeletedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/delete"));
                }
                DeviceDeletedField += value;
            }
            remove
            {
                Delegate[] list;
                DeviceDeletedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = DeviceDeletedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () => await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/delete"));
                }
            }
        }

        /// <summary>
        /// Occurs when an error is received.
        /// </summary>
        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived
        {
            add
            {
                Delegate[] list;
                if (MqttClient.IsConnected)
                {
                    list = ErrorReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () =>
                        {
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/up/errors");
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/errors");
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations/errors");
                        });
                }
                ErrorReceivedField += value;
            }
            remove
            {
                Delegate[] list;
                ErrorReceivedField -= value;
                if (MqttClient.IsConnected)
                {
                    list = ErrorReceivedField?.GetInvocationList();
                    if (list == null || list.Length == 0)
                        Task.Run(async () =>
                        {
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/up/errors");
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/errors");
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/activations/errors");
                        });
                }
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TTNet.Data.App"/> class.
        /// </summary>
        /// <param name="appId">App identifier.</param>
        public App(string appId)
        {
            AppID = appId;
            ClientID = Guid.NewGuid().ToString();

            //DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ssK")
            SerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true };
            DocumentOptions = new JsonDocumentOptions();

            MqttClient = new MqttFactory().CreateMqttClient();

            // Handle MQTT events
            MqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate((Action<MqttClientConnectedEventArgs>)OnConnected);
            MqttClient.UseDisconnectedHandler(async e => await Task.Run(() => Disconnected.Invoke(this, e)));
            MqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate((Action<MqttApplicationMessageReceivedEventArgs>)OnApplicationMessageReceived);
        }

        /// <summary>
        /// Connect to The Things Network server.
        /// </summary>
        /// <returns><c>true</c> if connection succeded; otherwise, <c>false</c>.</returns>
        /// <param name="accessKey">App access key.</param>
        /// <param name="region">Region.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<bool> Connect(string accessKey, string region, CancellationToken cancellationToken)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(ClientID)
                .WithTcpServer($"{region}.thethings.network", 8883)
                .WithCredentials(AppID, accessKey)
                .WithTls()
                .WithCleanSession()
                .Build();

            var result = await MqttClient.ConnectAsync(options, cancellationToken);
            return result.ResultCode == MqttClientConnectResultCode.Success;
        }

        /// <summary>
        /// Disconnect from server.
        /// </summary>
        /// <returns>The disconnection task.</returns>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task Disconnect(CancellationToken cancellationToken) =>
            await MqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);

        private async void OnConnected(MqttClientConnectedEventArgs e)
        {
            Delegate[] list;

            // Subscribe to topics with handled events
            if (e.AuthenticateResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                list = MessageReceivedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/up");

                list = MessageScheduledReceivedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/scheduled");

                list = MessageSentReceivedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/sent");

                list = MessageAckReceivedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/acks");

                list = ActivationReceivedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations");

                list = DeviceCreatedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/create");

                list = DeviceUpdatedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/update");

                list = DeviceDeletedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/delete");

                list = ErrorReceivedField?.GetInvocationList();
                if (list != null && list.Length > 0)
                {
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/up/errors");
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/errors");
                    await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations/errors");
                }
            }

            Connected.Invoke(this, e);
        }

        private void OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            Message<JsonElement> msg;
            MessageInfo msgInf;
            Activation activation;
            Error error;
            string[] topic = e.ApplicationMessage.Topic.Split('/');

            // Parse the message and raise the corresponding event

            if (topic[3] == "up")
            {
                try
                {
                    msg = JsonDocument.Parse(e.ApplicationMessage.Payload, DocumentOptions).RootElement.ConvertTo<Message<JsonElement>>();
                    MessageReceivedField.Invoke(this, new MessageReceivedEventArgs(msg, e.ApplicationMessage.Topic, topic));
                }
                catch (Exception ex)
                {
                    ExceptionThrowed.Invoke(this, ex);
                }
            }
            else if (topic[3] == "events")
            {
                if (topic.Length == 5)
                {
                    switch (topic[4])
                    {
                        case "activations":
                            try
                            {
                                activation = JsonDocument.Parse(e.ApplicationMessage.Payload, DocumentOptions).RootElement.ConvertTo<Activation>();
                                ActivationReceivedField.Invoke(this, new ActivationReceivedEventArgs(activation, e.ApplicationMessage.Topic, topic));
                            }
                            catch (Exception ex)
                            {
                                ExceptionThrowed.Invoke(this, ex);
                            }
                            break;
                        case "create":
                            DeviceCreatedField.Invoke(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                            break;
                        case "update":
                            DeviceUpdatedField.Invoke(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                            break;
                        case "delete":
                            DeviceDeletedField.Invoke(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                            break;
                    }
                }
                else if (topic.Length == 6)
                {
                    if (topic[5] == "errors")
                    {
                        error = JsonDocument.Parse(e.ApplicationMessage.Payload, DocumentOptions).RootElement.ConvertTo<Error>();
                        ErrorReceivedField.Invoke(this, new ErrorReceivedEventArgs(error, e.ApplicationMessage.Topic, topic));
                    }
                    else if (topic[4] == "down")
                    {
                        switch (topic[5])
                        {
                            case "scheduled":
                                MessageScheduledReceivedField.Invoke(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                                break;
                            case "sent":
                                msgInf = JsonDocument.Parse(e.ApplicationMessage.Payload, DocumentOptions).RootElement.ConvertTo<MessageInfo>();
                                MessageSentReceivedField.Invoke(this, new MessageInfoReceivedEventArgs(msgInf, e.ApplicationMessage.Topic, topic));
                                break;
                            case "acks":
                                MessageAckReceivedField.Invoke(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                                break;
                        }
                    }
                }
            }
        }

        #region Publish
        /// <summary>
        /// Send the specified message to a device.
        /// </summary>
        /// <returns>The publication task.</returns>
        /// <param name="msg">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task Publish<T>(Message<T> msg, CancellationToken cancellationToken)
        {
            try
            {
                await Publish(JsonConverter.From(msg), msg.AppID, msg.DeviceID, cancellationToken);
            }
            catch (JsonException ex)
            {
                ExceptionThrowed.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Send the specified payload to a device.
        /// </summary>
        /// <returns>The publication task.</returns>
        /// <param name="deviceID">Device identifier.</param>
        /// <param name="payload">Payload fields or a <see cref="T:byte[]"/> for a raw payload.</param>
        /// <param name="port">Port.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="confirmed">If set to <c>true</c> the message must be confirmed.</param>
        /// <param name="schedule">How this message shold be screduled in the queue.</param>
        /// <typeparam name="T">A serializable type or a <see cref="T:byte[]"/> type.</typeparam>
        public async Task Publish<T>(string deviceID, T payload, int port, CancellationToken cancellationToken, bool confirmed = false, Schedule schedule = Schedule.Replace)
        {
            var msg = new Message<T>
            {
                AppID = AppID,
                DeviceID = deviceID,
                Port = port,
                Confirmed = confirmed,
                Schedule = schedule
            };

            if (payload is byte[])
                msg.PayloadRaw = payload as byte[];
            else
                msg.PayloadFields = payload;

            await Publish(msg, cancellationToken);
        }

        /// <summary>
        /// Send the specified JSON to a device.
        /// </summary>
        /// <returns>The publication task.</returns>
        /// <param name="json">JSON.</param>
        /// <param name="appID">App identifier.</param>
        /// <param name="deviceID">Device identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task Publish(string json, string appID, string deviceID, CancellationToken cancellationToken)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"{appID}/devices/{deviceID}/down")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
                .Build();

            await MqttClient.PublishAsync(message, cancellationToken);
        }
        #endregion

        /// <summary>
        /// Dispose all resources used by this object
        /// </summary>
        public void Dispose() => MqttClient.Dispose();
    }
}