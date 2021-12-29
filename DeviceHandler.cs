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
    private protected IMqttClient? MqttClient { get; init; }
    private protected IManagedMqttClient? ManagedMqttClient { get; init; }
    private readonly string TopicBase;
    private protected JsonSerializerOptions SerializerOptions { get; init; }

    #region Events
    internal EventHandler<MessageReceivedEventArgs>? JoinField;
    internal EventHandler<MessageReceivedEventArgs>? UpField;
    internal EventHandler<MessageReceivedEventArgs>? DownQueuedField;
    internal EventHandler<MessageReceivedEventArgs>? DownSentField;
    internal EventHandler<MessageReceivedEventArgs>? DownAckField;
    internal EventHandler<MessageReceivedEventArgs>? DownNackField;
    internal EventHandler<MessageReceivedEventArgs>? DownFailedField;
    internal EventHandler<MessageReceivedEventArgs>? ServiceDataField;
    internal EventHandler<MessageReceivedEventArgs>? LocationSolvedField;

    /// <summary>
    /// Occurs when a join-accept message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> Join
    {
        add
        {
            var list = JoinField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/join"));
            JoinField += value;
        }
        remove
        {
            JoinField -= value;
            var list = JoinField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/join"));
        }
    }

    /// <summary>
    /// Occurs when an uplink message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> Up
    {
        add
        {
            var list = UpField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/up"));
            UpField += value;
        }
        remove
        {
            UpField -= value;
            var list = UpField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/up"));
        }
    }

    /// <summary>
    /// Occurs when a downlink queued message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownQueued
    {
        add
        {
            var list = DownQueuedField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/down/queued"));
            DownQueuedField += value;
        }
        remove
        {
            DownQueuedField -= value;
            var list = DownQueuedField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/down/queued"));
        }
    }

    /// <summary>
    /// Occurs when a downlink sent message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownSent
    {
        add
        {
            var list = DownSentField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/down/sent"));
            DownSentField += value;
        }
        remove
        {
            DownSentField -= value;
            var list = DownSentField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/down/sent"));
        }
    }

    /// <summary>
    /// Occurs when a downlink ACK message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownAck
    {
        add
        {
            var list = DownAckField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/down/ack"));
            DownAckField += value;
        }
        remove
        {
            DownAckField -= value;
            var list = DownAckField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/down/ack"));
        }
    }

    /// <summary>
    /// Occurs when a downlink NACK message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownNack
    {
        add
        {
            var list = DownNackField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/down/nack"));
            DownNackField += value;
        }
        remove
        {
            DownNackField -= value;
            var list = DownNackField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/down/nack"));
        }
    }

    /// <summary>
    /// Occurs when a downlink failed message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> DownFailed
    {
        add
        {
            var list = DownFailedField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/down/failed"));
            DownFailedField += value;
        }
        remove
        {
            DownFailedField -= value;
            var list = DownFailedField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/down/failed"));
        }
    }

    /// <summary>
    /// Occurs when a service data message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> ServiceData
    {
        add
        {
            var list = ServiceDataField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/service/data"));
            ServiceDataField += value;
        }
        remove
        {
            ServiceDataField -= value;
            var list = ServiceDataField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/service/data"));
        }
    }

    /// <summary>
    /// Occurs when a location solved message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> LocationSolved
    {
        add
        {
            var list = LocationSolvedField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await SubscribeAsync($"{TopicBase}/location/solved"));
            LocationSolvedField += value;
        }
        remove
        {
            LocationSolvedField -= value;
            var list = LocationSolvedField?.GetInvocationList();
            if (list == null || list.Length == 0)
                Task.Run(async () =>
                    await UnsubscribeAsync($"{TopicBase}/location/solved"));
        }
    }
    #endregion

    private DeviceHandler(string deviceId, string appId, string? tenantId)
    {
        TopicBase = tenantId != null ? $"v3/{appId}@{tenantId}/devices/{deviceId}" : $"v3/{appId}/devices/{deviceId}";
        SerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(new JsonSnakeUpperCaseNamingPolicy()) }
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TTNet.Data.DeviceHandler"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="T:MQTTnet.Client.IMqttClient"/>.</param>
    /// <param name="deviceId">Device identifier (+ for all devices).</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    public DeviceHandler(IMqttClient mqttClient, string deviceId, string appId, string? tenantId = "ttn") : this(deviceId, appId, tenantId)
    {
        MqttClient = mqttClient;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TTNet.Data.DeviceHandler"/> class.
    /// </summary>
    /// <param name="mqttClient">A <see cref="T:MQTTnet.Extensions.ManagedClient.IManagedMqttClient"/>.</param>
    /// <param name="deviceId">Device identifier (+ for all devices).</param>
    /// <param name="appId">App identifier.</param>
    /// <param name="tenantId">Tenant identifier. Use null for The Things Stack Open Source deployment.</param>
    public DeviceHandler(IManagedMqttClient mqttClient, string deviceId, string appId, string? tenantId = "ttn") : this(deviceId, appId, tenantId)
    {
        ManagedMqttClient = mqttClient;
    }

    /// <summary>
    /// Subscribe to all topics with an event handler asigned.
    /// </summary>
    public async Task SubscribeAsync()
    {
        Delegate[]? list;

        if (MqttClient?.IsConnected == true)
        {
            list = JoinField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/join");

            list = UpField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/up");

            list = DownQueuedField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/down/queued");

            list = DownSentField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/down/sent");

            list = DownAckField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/down/ack");

            list = DownNackField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/down/nack");

            list = DownFailedField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/down/failed");

            list = ServiceDataField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/service/data");

            list = LocationSolvedField?.GetInvocationList();
            if (list != null && list.Length > 0)
                await SubscribeAsync($"{TopicBase}/location/solved");
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
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(msg, SerializerOptions), cancellationToken, schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication result.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<MqttClientPublishResult> PublishAsync(Downlink msg, CancellationToken cancellationToken, Schedule schedule = Schedule.Push) =>
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(new Message { Downlinks = new[] { msg } }, SerializerOptions), cancellationToken, schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication result.</returns>
    /// <param name="json">Message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<MqttClientPublishResult> PublishAsync(string json, CancellationToken cancellationToken, Schedule schedule = Schedule.Push)
    {
        if (MqttClient == null)
            throw new InvalidOperationException("This is a managed instance. Use PublishAsync(msg, schedule).");
        var pubMsg = new MqttApplicationMessageBuilder()
            .WithTopic($"{TopicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
            .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
        return MqttClient.PublishAsync(pubMsg, cancellationToken);
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
        if (MqttClient == null)
            throw new InvalidOperationException("This is a managed instance. Use PublishAsync(msg, schedule).");
        var pubMsg = new MqttApplicationMessageBuilder()
            .WithTopic($"{TopicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
            .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
        return MqttClient.PublishAsync(pubMsg, cancellationToken);
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
        if (MqttClient == null)
            throw new InvalidOperationException("This is a managed instance. Use PublishAsync(msg, schedule).");
        var pubMsg = new MqttApplicationMessageBuilder()
            .WithTopic($"{TopicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
            .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
        return MqttClient.PublishAsync(pubMsg, cancellationToken);
    }

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<Guid> PublishAsync(Message msg, Schedule schedule = Schedule.Push) =>
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(msg, SerializerOptions), schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="msg">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual Task<Guid> PublishAsync(Downlink msg, Schedule schedule = Schedule.Push) =>
        PublishAsync(JsonSerializer.SerializeToUtf8Bytes(new Message { Downlinks = new[] { msg } }, SerializerOptions), schedule);

    /// <summary>
    /// Send the specified message.
    /// </summary>
    /// <returns>The publication ID.</returns>
    /// <param name="json">Message.</param>
    /// <param name="schedule">Schedule.</param>
    public virtual async Task<Guid> PublishAsync(string json, Schedule schedule = Schedule.Push)
    {
        if (ManagedMqttClient == null)
            throw new InvalidOperationException("This is an unmanaged instance. Use PublishAsync(msg, default(CancellationToken), schedule).");
        var pubMsg = new ManagedMqttApplicationMessageBuilder()
            .WithApplicationMessage(builder => builder
                .WithTopic($"{TopicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
            )
            .Build();
        await ManagedMqttClient.PublishAsync(pubMsg);
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
        if (ManagedMqttClient == null)
            throw new InvalidOperationException("This is an unmanaged instance. Use PublishAsync(msg, default(CancellationToken), schedule).");
        var pubMsg = new ManagedMqttApplicationMessageBuilder()
            .WithApplicationMessage(builder => builder
                .WithTopic($"{TopicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
            )
            .Build();
        await ManagedMqttClient.PublishAsync(pubMsg);
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
        if (ManagedMqttClient == null)
            throw new InvalidOperationException("This is an unmanaged instance. Use PublishAsync(msg, default(CancellationToken), schedule).");
        var pubMsg = new ManagedMqttApplicationMessageBuilder()
            .WithApplicationMessage(builder => builder
                .WithTopic($"{TopicBase}/down/{schedule.GetAttribute<EnumMemberAttribute>().Value}")
                .WithPayloadFormatIndicator(MqttPayloadFormatIndicator.CharacterData)
                .WithPayload(json)
                .WithAtMostOnceQoS()
            )
            .Build();
        await ManagedMqttClient.PublishAsync(pubMsg);
        return pubMsg.Id;
    }
    #endregion

    private async Task SubscribeAsync(string topic)
    {
        if (MqttClient != null)
        {
            if (MqttClient.IsConnected)
                await MqttClient.SubscribeAsync(topic);
        }
        else if (ManagedMqttClient != null)
        {
            await ManagedMqttClient.SubscribeAsync(topic);
        }
    }

    private async Task UnsubscribeAsync(string topic)
    {
        if (MqttClient != null)
        {
            if (MqttClient.IsConnected)
                await MqttClient.UnsubscribeAsync(topic);
        }
        else if (ManagedMqttClient != null)
        {
            await ManagedMqttClient.UnsubscribeAsync(topic);
        }
    }
}
