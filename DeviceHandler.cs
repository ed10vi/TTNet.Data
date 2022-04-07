using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using TTNet.Data.Model;

namespace TTNet.Data;

/// <summary>
/// A device handler for managing subscriptions and publishing.
/// </summary>
public class DeviceHandler
{
    private protected IMqttClient? _mqttClient { get; init; }
    private protected IManagedMqttClient? _managedMqttClient { get; init; }
    private readonly string _topicBase;
    private protected JsonSerializerOptions _serializerOptions { get; init; }

    #region Events
    internal EventHandler<MessageReceivedEventArgs>? _join;
    internal EventHandler<MessageReceivedEventArgs>? _up;
    internal EventHandler<MessageReceivedEventArgs>? _downQueued;
    internal EventHandler<MessageReceivedEventArgs>? _downSent;
    internal EventHandler<MessageReceivedEventArgs>? _downAck;
    internal EventHandler<MessageReceivedEventArgs>? _downNack;
    internal EventHandler<MessageReceivedEventArgs>? _downFailed;
    internal EventHandler<MessageReceivedEventArgs>? _serviceData;
    internal EventHandler<MessageReceivedEventArgs>? _locationSolved;

    /// <summary>
    /// Occurs when a join-accept message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> Join
    {
        add
        {
            var list = _join?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/join"));
            _join += value;
        }
        remove
        {
            _join -= value;
            var list = _join?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/join"));
        }
    }

    /// <summary>
    /// Occurs when an uplink message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> Up
    {
        add
        {
            var list = _up?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/up"));
            _up += value;
        }
        remove
        {
            _up -= value;
            var list = _up?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/up"));
        }
    }

    /// <summary>
    /// Occurs when a downlink queued message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownQueued
    {
        add
        {
            var list = _downQueued?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/down/queued"));
            _downQueued += value;
        }
        remove
        {
            _downQueued -= value;
            var list = _downQueued?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/down/queued"));
        }
    }

    /// <summary>
    /// Occurs when a downlink sent message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownSent
    {
        add
        {
            var list = _downSent?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/down/sent"));
            _downSent += value;
        }
        remove
        {
            _downSent -= value;
            var list = _downSent?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/down/sent"));
        }
    }

    /// <summary>
    /// Occurs when a downlink ACK message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownAck
    {
        add
        {
            var list = _downAck?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/down/ack"));
            _downAck += value;
        }
        remove
        {
            _downAck -= value;
            var list = _downAck?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/down/ack"));
        }
    }

    /// <summary>
    /// Occurs when a downlink NACK message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownNack
    {
        add
        {
            var list = _downNack?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/down/nack"));
            _downNack += value;
        }
        remove
        {
            _downNack -= value;
            var list = _downNack?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/down/nack"));
        }
    }

    /// <summary>
    /// Occurs when a downlink failed message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownFailed
    {
        add
        {
            var list = _downFailed?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/down/failed"));
            _downFailed += value;
        }
        remove
        {
            _downFailed -= value;
            var list = _downFailed?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/down/failed"));
        }
    }

    /// <summary>
    /// Occurs when a service data message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> ServiceData
    {
        add
        {
            var list = _serviceData?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/service/data"));
            _serviceData += value;
        }
        remove
        {
            _serviceData -= value;
            var list = _serviceData?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/service/data"));
        }
    }

    /// <summary>
    /// Occurs when a location solved message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> LocationSolved
    {
        add
        {
            var list = _locationSolved?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{_topicBase}/location/solved"));
            _locationSolved += value;
        }
        remove
        {
            _locationSolved -= value;
            var list = _locationSolved?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{_topicBase}/location/solved"));
        }
    }
    #endregion

    private DeviceHandler(string deviceId, string appId, string? tenantId)
    {
        _topicBase = tenantId != null ? $"v3/{appId}@{tenantId}/devices/{deviceId}" : $"v3/{appId}/devices/{deviceId}";
        _serializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(new JsonSnakeUpperCaseNamingPolicy()) }
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TTNet.Data.DeviceHandler"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="MQTTnet.Client.IMqttClient"/>.</param>
    /// <param name="deviceId">Device identifier (+ for all devices).</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    public DeviceHandler(IMqttClient mqttClient, string deviceId, string appId, string? tenantId = "ttn") : this(deviceId, appId, tenantId)
    {
        _mqttClient = mqttClient;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TTNet.Data.DeviceHandler"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="MQTTnet.Extensions.ManagedClient.IManagedMqttClient"/>.</param>
    /// <param name="deviceId">Device identifier (+ for all devices).</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    public DeviceHandler(IManagedMqttClient mqttClient, string deviceId, string appId, string? tenantId = "ttn") : this(deviceId, appId, tenantId)
    {
        _managedMqttClient = mqttClient;
    }

    /// <summary>
    /// Subscribe to all topics with an event handler asigned.
    /// </summary>
    public async Task SubscribeAsync()
    {
        Delegate[]? list;

        if (_mqttClient?.IsConnected == true)
        {
            list = _join?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/join");

            list = _up?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/up");

            list = _downQueued?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/down/queued");

            list = _downSent?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/down/sent");

            list = _downAck?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/down/ack");

            list = _downNack?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/down/nack");

            list = _downFailed?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/down/failed");

            list = _serviceData?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/service/data");

            list = _locationSolved?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{_topicBase}/location/solved");
        }
    }

    #region Publish
    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication result.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<MqttClientPublishResult> PublishAsync(Message msg, CancellationToken cancellationToken, Schedule schedule = Schedule.Push) =>
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(msg, _serializerOptions), cancellationToken, schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication result.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<MqttClientPublishResult> PublishAsync(Downlink msg, CancellationToken cancellationToken, Schedule schedule = Schedule.Push) =>
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(new Message { Downlinks = new[] { msg } }, _serializerOptions), cancellationToken, schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication result.</returns>
    /// <param name="json">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<MqttClientPublishResult> PublishAsync(string json, CancellationToken cancellationToken, Schedule schedule = Schedule.Push)
    {
        if (_mqttClient == null)
            throw new InvalidOperationException("This is a managed instance. Use PublishAsync(msg, schedule).");
        var pubMsg = new MqttApplicationMessageBuilder()
            .WithTopic($"{_topicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
            .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
        return _mqttClient.PublishAsync(pubMsg, cancellationToken);
    }

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication result.</returns>
    /// <param name="json">Message stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<MqttClientPublishResult> PublishAsync(Stream json, CancellationToken cancellationToken, Schedule schedule = Schedule.Push)
    {
        if (_mqttClient == null)
            throw new InvalidOperationException("This is a managed instance. Use PublishAsync(msg, schedule).");
        var pubMsg = new MqttApplicationMessageBuilder()
            .WithTopic($"{_topicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
            .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
        return _mqttClient.PublishAsync(pubMsg, cancellationToken);
    }

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication result.</returns>
    /// <param name="json">Message bytes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<MqttClientPublishResult> PublishAsync(IEnumerable<byte> json, CancellationToken cancellationToken, Schedule schedule = Schedule.Push)
    {
        if (_mqttClient == null)
            throw new InvalidOperationException("This is a managed instance. Use PublishAsync(msg, schedule).");
        var pubMsg = new MqttApplicationMessageBuilder()
            .WithTopic($"{_topicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
            .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
        return _mqttClient.PublishAsync(pubMsg, cancellationToken);
    }

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<Guid> PublishAsync(Message msg, Schedule schedule = Schedule.Push) =>
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(msg, _serializerOptions), schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<Guid> PublishAsync(Downlink msg, Schedule schedule = Schedule.Push) =>
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(new Message { Downlinks = new[] { msg } }, _serializerOptions), schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="json">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual async Task<Guid> PublishAsync(string json, Schedule schedule = Schedule.Push)
    {
        if (_managedMqttClient == null)
            throw new InvalidOperationException("This is an unmanaged instance. Use PublishAsync(msg, default(CancellationToken), schedule).");
        var pubMsg = new ManagedMqttApplicationMessageBuilder()
            .WithApplicationMessage(builder => builder
                .WithTopic($"{_topicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
            )
            .Build();
        await _managedMqttClient.PublishAsync(pubMsg);
        return pubMsg.Id;
    }

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="json">Message stream.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual async Task<Guid> PublishAsync(Stream json, Schedule schedule = Schedule.Push)
    {
        if (_managedMqttClient == null)
            throw new InvalidOperationException("This is an unmanaged instance. Use PublishAsync(msg, default(CancellationToken), schedule).");
        var pubMsg = new ManagedMqttApplicationMessageBuilder()
            .WithApplicationMessage(builder => builder
                .WithTopic($"{_topicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
            )
            .Build();
        await _managedMqttClient.PublishAsync(pubMsg);
        return pubMsg.Id;
    }

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="json">Message bytes.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual async Task<Guid> PublishAsync(IEnumerable<byte> json, Schedule schedule = Schedule.Push)
    {
        if (_managedMqttClient == null)
            throw new InvalidOperationException("This is an unmanaged instance. Use PublishAsync(msg, default(CancellationToken), schedule).");
        var pubMsg = new ManagedMqttApplicationMessageBuilder()
            .WithApplicationMessage(builder => builder
                .WithTopic($"{_topicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
            )
            .Build();
        await _managedMqttClient.PublishAsync(pubMsg);
        return pubMsg.Id;
    }
    #endregion

    private async Task SubscribeAsync(string topic)
    {
        if (_mqttClient != null)
        {
            if (_mqttClient.IsConnected)
                await _mqttClient.SubscribeAsync(topic);
        }
        else if (_managedMqttClient != null)
        {
            await _managedMqttClient.SubscribeAsync(topic);
        }
    }

    private async Task UnsubscribeAsync(string topic)
    {
        if (_mqttClient != null)
        {
            if (_mqttClient.IsConnected)
                await _mqttClient.UnsubscribeAsync(topic);
        }
        else if (_managedMqttClient != null)
        {
            await _managedMqttClient.UnsubscribeAsync(topic);
        }
    }
}
