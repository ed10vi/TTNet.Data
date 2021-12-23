using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// A message from The Things Network MQTT Data API.
/// </summary>
public class Message
{
    /// <summary>
    /// End device IDs.
    /// </summary>
    [JsonPropertyName("end_device_ids")]
    public DeviceIds EndDeviceIds { get; init; } = null!;

    /// <summary>
    /// Correlation identifiers of the message.
    /// </summary>
    [JsonPropertyName("correlation_ids")]
    public string[] CorrelationIds { get; init; } = null!;

    /// <summary>
    /// Timestamp at which the message has been received by the Application Server.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset ReceivedAt { get; private set; }

    /// <summary>
    /// ISO 8601 UTC timestamp at which the message has been received by the Application Server.
    /// </summary>
    [JsonPropertyName("received_at"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _ReceivedAt
    {
        get => ReceivedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        set => ReceivedAt = DateTimeOffset.Parse(value);
    }

    /// <summary>
    /// Join data.
    /// </summary>
    [JsonPropertyName("join_accept")]
    public JoinAccept? JoinAccept { get; init; }

    /// <summary>
    /// Uplink message.
    /// </summary>
    [JsonPropertyName("uplink_message")]
    public UplinkMessage? UplinkMessage { get; init; }

    /// <summary>
    /// Downlink messages.
    /// </summary>
    [JsonPropertyName("downlinks")]
    public Downlink[]? Downlinks { get; init; }

    /// <summary>
    /// Signals if the message is coming from the Network Server or is simulated.
    /// </summary>
    [JsonPropertyName("simulated")]
    public bool Simulated { get; init; }

    /// <summary>
    /// Downlink queued event data.
    /// </summary>
    [JsonPropertyName("downlink_queued")]
    public Downlink? DownlinkQueued { get; init; }

    /// <summary>
    /// Downlink ACK event data.
    /// </summary>
    [JsonPropertyName("downlink_ack")]
    public Downlink? DownlinkAck { get; init; }

    /// <summary>
    /// Downlink NACK event data.
    /// </summary>
    [JsonPropertyName("downlink_nack")]
    public Downlink? DownlinkNack { get; init; }

    /// <summary>
    /// Downlink sent event data.
    /// </summary>
    [JsonPropertyName("downlink_sent")]
    public Downlink? DownlinkSent { get; init; }

    /// <summary>
    /// Downlink failed event data.
    /// </summary>
    [JsonPropertyName("downlink_failed")]
    public DownlinkError? DownlinkFailed { get; init; }
}
