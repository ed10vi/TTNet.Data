using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Metadata for the antenna of the gateway that received this message.
/// </summary>
public class RxMetadata
{
    /// <summary>
    /// IDs of the gateway.
    /// </summary>
    [JsonPropertyName("gateway_ids")]
    public GatewayIds GatewayIds { get; init; } = null!;

    /// <summary>
    /// Timestamp at which the uplink has been received by the gateway.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset Time { get; private set; }

    /// <summary>
    /// ISO 8601 UTC timestamp at which the uplink has been received by the gateway.
    /// </summary>
    [JsonPropertyName("time"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _Time
    {
        get => Time.ToString("yyyy-MM-ddTHH:mm:ssK");
        set => Time = DateTimeOffset.Parse(value);
    }

    /// <summary>
    /// Timestamp of the gateway concentrator when the message has been received.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public ulong Timestamp { get; init; }

    /// <summary>
    /// Received signal strength indicator (dBm).
    /// </summary>
    [JsonPropertyName("rssi")]
    public int Rssi { get; init; }

    /// <summary>
    /// Received signal strength indicator of the channel (dBm).
    /// </summary>
    [JsonPropertyName("channel_rssi")]
    public int ChannelRssi { get; init; }

    /// <summary>
    /// Index of the gateway channel that received the message.
    /// </summary>
    [JsonPropertyName("channel_index")]
    public int ChannelIndex { get; init; }

    /// <summary>
    /// Signal-to-noise ratio (dB).
    /// </summary>
    [JsonPropertyName("snr")]
    public float Snr { get; init; }

    /// <summary>
    /// Gateway location metadata (only for gateways with location set to public).
    /// </summary>
    [JsonPropertyName("location")]
    public Location Location { get; init; } = null!;

    /// <summary>
    /// Uplink token injected by gateway, Gateway Server or fNS.
    /// </summary>
    [JsonPropertyName("uplink_token")]
    public byte[] UplinkToken { get; init; } = null!;
}
