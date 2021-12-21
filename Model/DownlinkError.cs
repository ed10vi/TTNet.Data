using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Downlink failed event data.
/// </summary>
public class DownlinkError
{    /// <summary>
    /// Downlink that which triggered the failure.
    /// </summary>
    [JsonPropertyName("downlink")]
    public Downlink Downlink { get; init; } = null!;
    /// <summary>
    /// Error that was encountered while sending the downlink.
    /// </summary>
    [JsonPropertyName("error")]
    public Error Error { get; init; } = null!;
}
