using System.Text.Json;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Downlink data.
/// </summary>
public class Downlink
{
    /// <summary>
    /// Frame port.
    /// </summary>
    [JsonPropertyName("f_port")]
    public byte FPort { get; init; }

    /// <summary>
    /// Frame payload.
    /// </summary>
    [JsonPropertyName("frm_payload")]
    public byte[]? FrmPayload { get; init; }

    /// <summary>
    /// Decoded payload object, to be encoded by the device payload formatter.
    /// </summary>
    [JsonPropertyName("decoded_payload")]
    public JsonElement? DecodedPayload { get; init; }

    /// <summary>
    /// Priority of the message in the downlink queue.
    /// </summary>
    [JsonPropertyName("priority")]
    public Priority Priority { get; init; }

    /// <summary>
    /// If the downlink expects a confirmation from the device or not.
    /// </summary>
    [JsonPropertyName("confirmed")]
    public bool Confirmed { get; init; }

    /// <summary>
    /// Correlation identifiers of the message.
    /// </summary>
    [JsonPropertyName("correlation_ids")]
    public string[]? CorrelationIds { get; init; }
}
