using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// IDs of the version.
/// </summary>
public class VersionIds
{
    /// <summary>
    /// Brand ID.
    /// </summary>
    [JsonPropertyName("brand_id")]
    public string BrandId { get; init; } = null!;

    /// <summary>
    /// Model ID.
    /// </summary>
    [JsonPropertyName("model_id")]
    public string ModelId { get; init; } = null!;

    /// <summary>
    /// Hardware version.
    /// </summary>
    [JsonPropertyName("hardware_version")]
    public string HardwareVersion { get; init; } = null!;

    /// <summary>
    /// Firmware version.
    /// </summary>
    [JsonPropertyName("firmware_version")]
    public string FirmwareVersion { get; init; } = null!;

    /// <summary>
    /// Band ID.
    /// </summary>
    [JsonPropertyName("band_id")]
    public string BandId { get; init; } = null!;
}
