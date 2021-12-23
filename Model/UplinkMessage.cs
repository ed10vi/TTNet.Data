using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Uplink message.
/// </summary>
public class UplinkMessage
{
    /// <summary>
    /// Join Server issued identifier for the session keys used by this uplink.
    /// </summary>
    [JsonPropertyName("session_key_id")]
    public byte[]? SessionKeyId { get; init; }

    /// <summary>
    /// Frame port.
    /// </summary>
    [JsonPropertyName("f_port")]
    public byte FPort { get; init; }

    /// <summary>
    /// Frame counter.
    /// </summary>
    [JsonPropertyName("f_cnt")]
    public int? FCnt { get; init; }

    /// <summary>
    /// Frame payload (Base64).
    /// </summary>
    [JsonPropertyName("frm_payload")]
    public byte[] FrmPayload { get; init; } = null!;

    /// <summary>
    /// Decoded payload object, decoded by the device payload formatter.
    /// </summary>
    [JsonPropertyName("decoded_payload")]
    public JsonElement? DecodedPayload { get; init; }

    /// <summary>
    /// A list of metadata for each antenna of each gateway that received this message.
    /// </summary>
    [JsonPropertyName("rx_metadata")]
    public RxMetadata[]? RxMetadata { get; init; }

    /// <summary>
    /// Settings for the transmission.
    /// </summary>
    [JsonPropertyName("settings")]
    public Settings Settings { get; init; } = null!;

    /// <summary>
    /// Timestamp at which the uplink has been received by the Network Server.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset? ReceivedAt { get; private set; }

    /// <summary>
    /// ISO 8601 UTC timestamp at which the uplink has been received by the Network Server.
    /// </summary>
    [JsonPropertyName("received_at"), EditorBrowsable(EditorBrowsableState.Never)]
    public string? _ReceivedAt
    {
        get => ReceivedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        set => ReceivedAt = value != null ? DateTimeOffset.Parse(value) : null;
    }

    /// <summary>
    /// If the downlink expects a confirmation from the device or not.
    /// </summary>
    [JsonPropertyName("confirmed")]
    public bool Confirmed { get; init; }

    /// <summary>
    /// End device location metadata.
    /// </summary>
    [JsonPropertyName("locations")]
    public Locations? Locations { get; init; }

    /// <summary>
    /// Time-on-air, calculated by the Network Server using payload size and transmission settings.
    /// </summary>
    [JsonIgnore]
    public TimeSpan? ConsumedAirtime { get; private set; }

    /// <summary>
    /// ISO 8601 UTC timestamp at which the uplink has been received by the Network Server.
    /// </summary>
    [JsonPropertyName("consumed_airtime"), EditorBrowsable(EditorBrowsableState.Never)]
    public string? _ConsumedAirtime
    {
        get => ConsumedAirtime != null ? $"{ConsumedAirtime.Value.TotalSeconds}s" : null;
        set => ConsumedAirtime = value != null ? TimeSpan.FromSeconds(double.Parse(value.TrimEnd('s'), CultureInfo.InvariantCulture)) : null;
    }

    /// <summary>
    /// IDs of the version.
    /// </summary>
    [JsonPropertyName("version_ids")]
    public VersionIds? VersionIds { get; init; }

    /// <summary>
    /// IDs of the network.
    /// </summary>
    [JsonPropertyName("network_ids")]
    public NetworkIds? NetworkIds { get; init; }
}
