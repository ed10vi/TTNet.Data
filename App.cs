using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
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
        private IManagedMqttClient ManagedMqttClient;

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

        /// <summary>
        /// Connection managed automatically.
        /// </summary>
        public bool Managed
        {
            get => MqttClient == null && ManagedMqttClient != null;
        }

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
                var list = MessageReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/up");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/up");
                    });
                MessageReceivedField += value;
            }
            remove
            {
                MessageReceivedField -= value;
                var list = MessageReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/up");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/up");
                    });
            }
        }

        /// <summary>
        /// Occurs when message scheduled to be sent to a device.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> MessageScheduledReceived
        {
            add
            {
                var list = MessageScheduledReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/scheduled");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/scheduled");
                    });
                MessageScheduledReceivedField += value;
            }
            remove
            {
                MessageScheduledReceivedField -= value;
                var list = MessageScheduledReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/scheduled");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/scheduled");
                    });
            }
        }

        /// <summary>
        /// Occurs when a message is sent to a device.
        /// </summary>
        public event EventHandler<MessageInfoReceivedEventArgs> MessageSentReceived
        {
            add
            {
                var list = MessageSentReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/sent");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/sent");
                    });
                MessageSentReceivedField += value;
            }
            remove
            {
                MessageSentReceivedField -= value;
                var list = MessageSentReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/sent");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/sent");
                    });
            }
        }

        /// <summary>
        /// Occurs when an ack sent by a device is received.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> MessageAckReceived
        {
            add
            {
                var list = MessageAckReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/acks");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/acks");
                    });
                MessageAckReceivedField += value;
            }
            remove
            {
                MessageAckReceivedField -= value;
                var list = MessageAckReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/acks");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/acks");
                    });
            }
        }

        /// <summary>
        /// Occurs when a device is activated.
        /// </summary>
        public event EventHandler<ActivationReceivedEventArgs> ActivationReceived
        {
            add
            {
                var list = ActivationReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations");
                    });
                ActivationReceivedField += value;
            }
            remove
            {
                ActivationReceivedField -= value;
                var list = ActivationReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/activations");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/activations");
                    });
            }
        }

        /// <summary>
        /// Occurs when a device is created.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> DeviceCreated
        {
            add
            {
                var list = DeviceCreatedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/create");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/create");
                    });
                DeviceCreatedField += value;
            }
            remove
            {
                DeviceCreatedField -= value;
                var list = DeviceCreatedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/create");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/create");
                    });
            }
        }

        /// <summary>
        /// Occurs when a device is updated.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> DeviceUpdated
        {
            add
            {
                var list = DeviceUpdatedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/update");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/update");
                    });
                DeviceUpdatedField += value;
            }
            remove
            {
                DeviceUpdatedField -= value;
                var list = DeviceUpdatedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/update");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/update");
                    });
            }
        }

        /// <summary>
        /// Occurs when a device is deleted.
        /// </summary>
        public event EventHandler<UpdateReceivedEventArgs> DeviceDeleted
        {
            add
            {
                var list = DeviceDeletedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/delete");
                        else if (MqttClient.IsConnected)
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/delete");
                    });
                DeviceDeletedField += value;
            }
            remove
            {
                DeviceDeletedField -= value;
                var list = DeviceDeletedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/delete");
                        else if (MqttClient.IsConnected)
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/delete");
                    });
            }
        }

        /// <summary>
        /// Occurs when an error is received.
        /// </summary>
        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived
        {
            add
            {
                var list = ErrorReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                        {
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/up/errors");
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/errors");
                            await ManagedMqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations/errors");
                        }
                        else if (MqttClient.IsConnected)
                        {
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/up/errors");
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/down/errors");
                            await MqttClient.SubscribeAsync($"{AppID}/devices/+/events/activations/errors");
                        }
                    });
                ErrorReceivedField += value;
            }
            remove
            {
                ErrorReceivedField -= value;
                var list = ErrorReceivedField?.GetInvocationList();
                if (list == null || list.Length == 0)
                    Task.Run(async () =>
                    {
                        if (Managed)
                        {
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/up/errors");
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/errors");
                            await ManagedMqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/activations/errors");
                        }
                        else if (MqttClient.IsConnected)
                        {
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/up/errors");
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/down/errors");
                            await MqttClient.UnsubscribeAsync($"{AppID}/devices/+/events/activations/errors");
                        }
                    });
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TTNet.Data.App"/> class.
        /// </summary>
        /// <param name="appId">App identifier.</param>
        /// <param name="managed">Automatically manage the connection.</param>
        public App(string appId, bool managed = false)
        {
            AppID = appId;
            ClientID = Guid.NewGuid().ToString();

            //DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ssK")
            SerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true };
            DocumentOptions = new JsonDocumentOptions();

            if (managed)
            {
                MqttClient = null;
                ManagedMqttClient = new MqttFactory().CreateManagedMqttClient();
                ManagedMqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(e => Connected(this, e));
                ManagedMqttClient.UseDisconnectedHandler(async e => await Task.Run(() => Disconnected(this, e)));
                ManagedMqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate((Action<MqttApplicationMessageReceivedEventArgs>)OnApplicationMessageReceived);
            }
            else
            {
                ManagedMqttClient = null;
                MqttClient = new MqttFactory().CreateMqttClient();
                MqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate((Action<MqttClientConnectedEventArgs>)OnConnected);
                MqttClient.UseDisconnectedHandler(async e => await Task.Run(() => Disconnected(this, e)));
                MqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate((Action<MqttApplicationMessageReceivedEventArgs>)OnApplicationMessageReceived);
            }
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
            if (Managed)
                throw new InvalidOperationException("In managed mode, the Start command must be used");

            var result = await MqttClient.ConnectAsync(GetMqttClientOptions(accessKey, region), cancellationToken);
            return result.ResultCode == MqttClientConnectResultCode.Success;
        }

        /// <summary>
        /// Start connection to The Things Network server.
        /// </summary>
        /// <param name="accessKey">App access key.</param>
        /// <param name="region">Region.</param>
        /// <param name="autoReconnectDelay">Time to wait after a disconnection to reconnect.</param>
        public async Task Start(string accessKey, string region, TimeSpan autoReconnectDelay)
        {
            if (!Managed)
                throw new InvalidOperationException("In unmanaged mode, the Connect command must be used");

            await ManagedMqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(autoReconnectDelay)
                .WithClientOptions(GetMqttClientOptions(accessKey, region))
                .Build());
        }

        /// <summary>
        /// Disconnect from server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task Disconnect(CancellationToken cancellationToken)
        {
            if (Managed)
                throw new InvalidOperationException("In managed mode, the Stop command must be used");

            await MqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
        }

        /// <summary>
        /// Stop connection.
        /// </summary>
        public async Task Stop()
        {
            if (!Managed)
                throw new InvalidOperationException("In unmanaged mode, the Disconnect command must be used");

            await ManagedMqttClient.StopAsync();
        }

        private IMqttClientOptions GetMqttClientOptions(string accessKey, string region) =>
            new MqttClientOptionsBuilder()
                .WithClientId(ClientID)
                .WithTcpServer($"{region}.thethings.network", 8883)
                .WithCredentials(AppID, accessKey)
                .WithTls()
                .WithCleanSession()
                .Build();

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

            Connected(this, e);
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
                    MessageReceivedField(this, new MessageReceivedEventArgs(msg, e.ApplicationMessage.Topic, topic));
                }
                catch (Exception ex)
                {
                    ExceptionThrowed(this, ex);
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
                                ActivationReceivedField(this, new ActivationReceivedEventArgs(activation, e.ApplicationMessage.Topic, topic));
                            }
                            catch (Exception ex)
                            {
                                ExceptionThrowed(this, ex);
                            }
                            break;
                        case "create":
                            DeviceCreatedField(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                            break;
                        case "update":
                            DeviceUpdatedField(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                            break;
                        case "delete":
                            DeviceDeletedField(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                            break;
                    }
                }
                else if (topic.Length == 6)
                {
                    if (topic[5] == "errors")
                    {
                        error = JsonDocument.Parse(e.ApplicationMessage.Payload, DocumentOptions).RootElement.ConvertTo<Error>();
                        ErrorReceivedField(this, new ErrorReceivedEventArgs(error, e.ApplicationMessage.Topic, topic));
                    }
                    else if (topic[4] == "down")
                    {
                        switch (topic[5])
                        {
                            case "scheduled":
                                MessageScheduledReceivedField(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
                                break;
                            case "sent":
                                msgInf = JsonDocument.Parse(e.ApplicationMessage.Payload, DocumentOptions).RootElement.ConvertTo<MessageInfo>();
                                MessageSentReceivedField(this, new MessageInfoReceivedEventArgs(msgInf, e.ApplicationMessage.Topic, topic));
                                break;
                            case "acks":
                                MessageAckReceivedField(this, new UpdateReceivedEventArgs(e.ApplicationMessage.Topic, topic));
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
        public async Task<MqttClientPublishResult> Publish<T>(Message<T> msg, CancellationToken cancellationToken)
        {
            MqttClientPublishResult result = null;
            try
            {
                result = await Publish(JsonConverter.From(msg), msg.AppID, msg.DeviceID, cancellationToken);
            }
            catch (JsonException ex)
            {
                ExceptionThrowed(this, ex);
            }
            return result;
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
        public async Task<MqttClientPublishResult> Publish<T>(string deviceID, T payload, int port, CancellationToken cancellationToken, bool confirmed = false, Schedule schedule = Schedule.Replace)
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

            return await Publish(msg, cancellationToken);
        }

        /// <summary>
        /// Send the specified JSON to a device.
        /// </summary>
        /// <returns>The publication task.</returns>
        /// <param name="json">JSON.</param>
        /// <param name="appID">App identifier.</param>
        /// <param name="deviceID">Device identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task<MqttClientPublishResult> Publish(string json, string appID, string deviceID, CancellationToken cancellationToken)
        {
            MqttClientPublishResult result;
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"{appID}/devices/{deviceID}/down")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
                .Build();

            if (Managed)
                result = await ManagedMqttClient.PublishAsync(message, cancellationToken);
            else
                result = await MqttClient.PublishAsync(message, cancellationToken);
            return result;
        }
        #endregion

        /// <summary>
        /// Dispose all resources used by this object
        /// </summary>
        public void Dispose()
        {
            if (Managed)
                ManagedMqttClient.Dispose();
            else
                MqttClient.Dispose();
        }
    }
}