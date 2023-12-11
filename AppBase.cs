using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using TTNet.Data.Model;

namespace TTNet.Data;

/// <summary>
/// A The Things Network Application Data connection.
/// </summary>
public abstract class AppBase : DeviceHandler, IDisposable
{
    /// <summary>
    /// Client identifier.
    /// </summary>
    public string ClientID { get; private set; }

    private Dictionary<string, DeviceHandler> _deviceHandlers;

    /// <summary>
    /// Application identifier.
    /// </summary>
    public string AppID { get; private set; }

    /// <summary>
    /// Tenant identifier.
    /// </summary>
    public string? TenantID { get; private set; }

    /// <summary>
    /// Value indicating whether this <see cref="TTNet.Data.AppBase"/> is connected.
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Get the <see cref="TTNet.Data.DeviceHandler"/> for a device ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The <see cref="TTNet.Data.DeviceHandler"/>.</returns>
    public DeviceHandler this[string deviceId]
    {
        get
        {
            DeviceHandler result;
            if (_deviceHandlers.ContainsKey(deviceId))
                result = _deviceHandlers[deviceId];
            else
            {
                if (_mqttClient != null)
                    result = new DeviceHandler(_mqttClient, deviceId, AppID, TenantID);
                else if (_managedMqttClient != null)
                    result = new DeviceHandler(_managedMqttClient, deviceId, AppID, TenantID);
                else
                    throw new Exception();
                _deviceHandlers.Add(deviceId, result);
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
    /// Initializes a new instance of the <see cref="TTNet.Data.AppBase"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="MQTTnet.Client.IMqttClient"/>.</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    protected AppBase(IMqttClient mqttClient, string appId, string? tenantId = "ttn") : base(mqttClient, "+", appId, tenantId)
    {
        AppID = appId;
        TenantID = tenantId;
        ClientID = Guid.NewGuid().ToString();
        _deviceHandlers = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TTNet.Data.AppBase"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="MQTTnet.Extensions.ManagedClient.IManagedMqttClient"/>.</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    protected AppBase(IManagedMqttClient mqttClient, string appId, string? tenantId = "ttn") : base(mqttClient, "+", appId, tenantId)
    {
        AppID = appId;
        TenantID = tenantId;
        ClientID = Guid.NewGuid().ToString();
        _deviceHandlers = [];
    }

    private protected MqttClientOptions GetMqttClientOptions(string server, int port, bool withTls, string username, string apiKey)
    {
        var o = new MqttClientOptionsBuilder()
            .WithClientId(ClientID)
            .WithTcpServer(server, port)
            .WithCredentials(username, apiKey)
            .WithCleanSession();
        return withTls ? o.WithTlsOptions(o => o.WithSslProtocols(System.Security.Authentication.SslProtocols.None)).Build() : o.Build();
    }

    private protected async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        Message? msg;
        MessageReceivedEventArgs eventArgs;
        string[] topic = e.ApplicationMessage.Topic.Split('/');

        // Parse the message and raise the corresponding event
        try
        {
            msg = JsonSerializer.Deserialize<Message>(e.ApplicationMessage.PayloadSegment, _serializerOptions);
            if (msg == null)
                throw new JsonException("JsonSerializer returned null");
            eventArgs = new MessageReceivedEventArgs(msg, e.ApplicationMessage.Topic, topic);
            switch (topic[4])
            {
                case "join":
                    _join?.Invoke(this, eventArgs);
                    if (_deviceHandlers.ContainsKey(topic[3]))
                        _deviceHandlers[topic[3]]._join?.Invoke(this, eventArgs);
                    break;
                case "up":
                    _up?.Invoke(this, eventArgs);
                    if (_deviceHandlers.ContainsKey(topic[3]))
                        _deviceHandlers[topic[3]]._up?.Invoke(this, eventArgs);
                    break;
                case "down":
                    switch (topic[5])
                    {
                        case "queued":
                            _downQueued?.Invoke(this, eventArgs);
                            if (_deviceHandlers.ContainsKey(topic[3]))
                                _deviceHandlers[topic[3]]._downQueued?.Invoke(this, eventArgs);
                            break;
                        case "sent":
                            _downSent?.Invoke(this, eventArgs);
                            if (_deviceHandlers.ContainsKey(topic[3]))
                                _deviceHandlers[topic[3]]._downSent?.Invoke(this, eventArgs);
                            break;
                        case "ack":
                            _downAck?.Invoke(this, eventArgs);
                            if (_deviceHandlers.ContainsKey(topic[3]))
                                _deviceHandlers[topic[3]]._downAck?.Invoke(this, eventArgs);
                            break;
                        case "nack":
                            _downNack?.Invoke(this, eventArgs);
                            if (_deviceHandlers.ContainsKey(topic[3]))
                                _deviceHandlers[topic[3]]._downNack?.Invoke(this, eventArgs);
                            break;
                        case "failed":
                            _downFailed?.Invoke(this, eventArgs);
                            if (_deviceHandlers.ContainsKey(topic[3]))
                                _deviceHandlers[topic[3]]._downFailed?.Invoke(this, eventArgs);
                            break;
                    }
                    break;
                case "service":
                    if (topic[5] == "data")
                    {
                        _serviceData?.Invoke(this, eventArgs);
                        if (_deviceHandlers.ContainsKey(topic[3]))
                            _deviceHandlers[topic[3]]._serviceData?.Invoke(this, eventArgs);
                    }
                    break;
                case "location":
                    if (topic[5] == "solved")
                    {
                        _locationSolved?.Invoke(this, eventArgs);
                        if (_deviceHandlers.ContainsKey(topic[3]))
                            _deviceHandlers[topic[3]]._locationSolved?.Invoke(this, eventArgs);
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

    private protected async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
    {
        // Subscribe to topics with handled events
        if (eventArgs.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
        {
            await SubscribeAsync();
            foreach (DeviceHandler d in _deviceHandlers.Values)
                await d.SubscribeAsync();
        }
        if (Connected != null)
            await Connected.InvokeAsync(this, eventArgs);
    }

    private protected Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs) =>
        Disconnected?.InvokeAsync(this, eventArgs) ?? Task.CompletedTask;

    private protected Task HandleApplicationMessageProcessedAsync(ApplicationMessageProcessedEventArgs eventArgs) =>
        MessageProcessed?.InvokeAsync(this, eventArgs.ApplicationMessage.Id) ?? Task.CompletedTask;

    private protected Task HandleApplicationMessageSkippedAsync(ApplicationMessageSkippedEventArgs eventArgs) =>
        MessageSkipped?.InvokeAsync(this, eventArgs.ApplicationMessage.Id) ?? Task.CompletedTask;

    private protected Task HandleConnectingFailedAsync(ConnectingFailedEventArgs eventArgs) =>
        ExceptionThrowed?.InvokeAsync(this, eventArgs.Exception) ?? Task.CompletedTask;

    private protected Task HandleSynchronizingSubscriptionsFailedAsync(ManagedProcessFailedEventArgs eventArgs) =>
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
