using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using TTNet.Data.Model;

namespace TTNet.Data;

/// <summary>
/// A The Things Network Application Data connection.
/// </summary>
public abstract class AppBase : DeviceHandler, IDisposable,
    IMqttClientConnectedHandler,
    IMqttClientDisconnectedHandler,
    IMqttApplicationMessageReceivedHandler,
    IApplicationMessageProcessedHandler,
    IApplicationMessageSkippedHandler,
    IConnectingFailedHandler,
    ISynchronizingSubscriptionsFailedHandler
{
    /// <summary>
    /// Client identifier.
    /// </summary>
    public string ClientID { get; init; }

    private Dictionary<string, DeviceHandler> DeviceHandlers;

    /// <summary>
    /// Application identifier.
    /// </summary>
    public string AppID { get; private set; }

    /// <summary>
    /// Tenant identifier.
    /// </summary>
    public string? TenantID { get; private set; }

    /// <summary>
    /// Value indicating whether this <see cref="T:TTNet.Data.AppBase"/> is connected.
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Get the <see cref="T:TTNet.Data.DeviceHandler"/> for a device ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The <see cref="T:TTNet.Data.DeviceHandler"/>.</returns>
    public DeviceHandler this[string deviceId]
    {
        get
        {
            DeviceHandler result;
            if (DeviceHandlers.ContainsKey(deviceId))
                result = DeviceHandlers[deviceId];
            else
            {
                if (MqttClient != null)
                    result = new DeviceHandler(MqttClient, deviceId, AppID, TenantID);
                else if (ManagedMqttClient != null)
                    result = new DeviceHandler(ManagedMqttClient, deviceId, AppID, TenantID);
                else
                    throw new Exception();
                DeviceHandlers.Add(deviceId, result);
            }
            return result;
        }
    }

    /// <summary>
    /// Occurs when connection is completed.
    /// </summary>
    public event EventHandler<MqttClientConnectedEventArgs>? Connected;
    /// <summary>
    /// Occurs when disconnected.
    /// </summary>
    public event EventHandler<MqttClientDisconnectedEventArgs>? Disconnected;
    /// <summary>
    /// Occurs when a exception is throwed.
    /// </summary>
    public event EventHandler<Exception>? ExceptionThrowed;
    /// <summary>
    /// Occurs when a message is processed in managed mode.
    /// </summary>
    public event EventHandler<Guid>? MessageProcessed;
    /// <summary>
    /// Occurs when a message is skipped in managed mode.
    /// </summary>
    public event EventHandler<Guid>? MessageSkipped;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TTNet.Data.AppBase"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="T:MQTTnet.Client.IMqttClient"/>.</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    protected AppBase(IMqttClient mqttClient, string appId, string? tenantId = "ttn") : base(mqttClient, "+", appId, tenantId)
    {
        AppID = appId;
        TenantID = tenantId;
        ClientID = Guid.NewGuid().ToString();
        DeviceHandlers = new Dictionary<string, DeviceHandler>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TTNet.Data.AppBase"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="T:MQTTnet.Extensions.ManagedClient.IManagedMqttClient"/>.</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    protected AppBase(IManagedMqttClient mqttClient, string appId, string? tenantId = "ttn") : base(mqttClient, "+", appId, tenantId)
    {
        AppID = appId;
        TenantID = tenantId;
        ClientID = Guid.NewGuid().ToString();
        DeviceHandlers = new Dictionary<string, DeviceHandler>();
    }

    async Task IMqttApplicationMessageReceivedHandler.HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        Message? msg;
        MessageReceivedEventArgs eventArgs;
        string[] topic = e.ApplicationMessage.Topic.Split('/');

        // Parse the message and raise the corresponding event
        try
        {
            msg = JsonSerializer.Deserialize<Message>(e.ApplicationMessage.Payload, SerializerOptions);
            //msg = JsonDocument.Parse(e.ApplicationMessage.Payload, DocumentOptions).RootElement.ConvertTo<Message>();
            if (msg == null)
                throw new JsonException("JsonSerializer returned null");
            eventArgs = new MessageReceivedEventArgs(msg, e.ApplicationMessage.Topic, topic);
            switch (topic[4])
            {
                case "join":
                    JoinField?.Invoke(this, eventArgs);
                    if (DeviceHandlers.ContainsKey(topic[3]))
                        DeviceHandlers[topic[3]].JoinField?.Invoke(this, eventArgs);
                    break;
                case "up":
                    UpField?.Invoke(this, eventArgs);
                    if (DeviceHandlers.ContainsKey(topic[3]))
                        DeviceHandlers[topic[3]].UpField?.Invoke(this, eventArgs);
                    break;
                case "down":
                    switch (topic[5])
                    {
                        case "queued":
                            DownQueuedField?.Invoke(this, eventArgs);
                            if (DeviceHandlers.ContainsKey(topic[3]))
                                DeviceHandlers[topic[3]].DownQueuedField?.Invoke(this, eventArgs);
                            break;
                        case "sent":
                            DownSentField?.Invoke(this, eventArgs);
                            if (DeviceHandlers.ContainsKey(topic[3]))
                                DeviceHandlers[topic[3]].DownSentField?.Invoke(this, eventArgs);
                            break;
                        case "ack":
                            DownAckField?.Invoke(this, eventArgs);
                            if (DeviceHandlers.ContainsKey(topic[3]))
                                DeviceHandlers[topic[3]].DownAckField?.Invoke(this, eventArgs);
                            break;
                        case "nack":
                            DownNackField?.Invoke(this, eventArgs);
                            if (DeviceHandlers.ContainsKey(topic[3]))
                                DeviceHandlers[topic[3]].DownNackField?.Invoke(this, eventArgs);
                            break;
                        case "failed":
                            DownFailedField?.Invoke(this, eventArgs);
                            if (DeviceHandlers.ContainsKey(topic[3]))
                                DeviceHandlers[topic[3]].DownFailedField?.Invoke(this, eventArgs);
                            break;
                    }
                    break;
                case "service":
                    if (topic[5] == "data")
                    {
                        ServiceDataField?.Invoke(this, eventArgs);
                        if (DeviceHandlers.ContainsKey(topic[3]))
                            DeviceHandlers[topic[3]].ServiceDataField?.Invoke(this, eventArgs);
                    }
                    break;
                case "location":
                    if (topic[5] == "solved")
                    {
                        LocationSolvedField?.Invoke(this, eventArgs);
                        if (DeviceHandlers.ContainsKey(topic[3]))
                            DeviceHandlers[topic[3]].LocationSolvedField?.Invoke(this, eventArgs);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            if (ExceptionThrowed != null)
                await ExceptionThrowed.InvokeAsync(this, ex);
        }
    }

    async Task IMqttClientConnectedHandler.HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
    {
        // Subscribe to topics with handled events
        if (eventArgs.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
        {
            await SubscribeAsync();
            foreach (var d in DeviceHandlers.Values)
                await d.SubscribeAsync();
        }
        if (Connected != null)
            await Connected.InvokeAsync(this, eventArgs);
    }

    Task IMqttClientDisconnectedHandler.HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs) =>
        Disconnected?.InvokeAsync(this, eventArgs) ?? Task.CompletedTask;

    Task IApplicationMessageProcessedHandler.HandleApplicationMessageProcessedAsync(ApplicationMessageProcessedEventArgs eventArgs) =>
        MessageProcessed?.InvokeAsync(this, eventArgs.ApplicationMessage.Id) ?? Task.CompletedTask;

    Task IApplicationMessageSkippedHandler.HandleApplicationMessageSkippedAsync(ApplicationMessageSkippedEventArgs eventArgs) =>
        MessageSkipped?.InvokeAsync(this, eventArgs.ApplicationMessage.Id) ?? Task.CompletedTask;

    Task IConnectingFailedHandler.HandleConnectingFailedAsync(ManagedProcessFailedEventArgs eventArgs) =>
        ExceptionThrowed?.InvokeAsync(this, eventArgs.Exception) ?? Task.CompletedTask;

    Task ISynchronizingSubscriptionsFailedHandler.HandleSynchronizingSubscriptionsFailedAsync(ManagedProcessFailedEventArgs eventArgs) =>
        ExceptionThrowed?.InvokeAsync(this, eventArgs.Exception) ?? Task.CompletedTask;

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication result.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<MqttClientPublishResult> PublishAsync(Message msg, CancellationToken cancellationToken, Schedule schedule) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication result.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<MqttClientPublishResult> PublishAsync(Downlink msg, CancellationToken cancellationToken, Schedule schedule) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication result.</returns>
    /// <param name="json">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<MqttClientPublishResult> PublishAsync(string json, CancellationToken cancellationToken, Schedule schedule) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication result.</returns>
    /// <param name="json">Message stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<MqttClientPublishResult> PublishAsync(Stream json, CancellationToken cancellationToken, Schedule schedule = Schedule.Push) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication ID.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<Guid> PublishAsync(Message msg, Schedule schedule = Schedule.Push) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication ID.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<Guid> PublishAsync(Downlink msg, Schedule schedule = Schedule.Push) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication ID.</returns>
    /// <param name="json">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<Guid> PublishAsync(string json, Schedule schedule = Schedule.Push) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Unsupported. This must be called from a specific device.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown always.</exception>
    /// <returns>The publication ID.</returns>
    /// <param name="json">Message stream.</param>
    /// <param name="schedule">Schedule.</param>
    public override Task<Guid> PublishAsync(Stream json, Schedule schedule = Schedule.Push) =>
        throw new InvalidOperationException();

    /// <summary>
    /// Dispose all resources used by this object
    /// </summary>
    public abstract void Dispose();
}
