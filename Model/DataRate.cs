using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Data rate settings.
/// </summary>
public class DataRate
{
    /// <summary>
    /// LoRa modulation settings.
    /// </summary>
    [JsonPropertyName("lora")]
    public Lora Lora { get; init; } = null!;
}

/// <summary>
/// LoRa modulation settings.
/// </summary>
public class Lora
{
    /// <summary>
    /// Bandwidth (Hz).
    /// </summary>
    [JsonPropertyName("bandwidth")]
    public int Bandwidth { get; init; }

    /// <summary>
    /// Spreading factor.
    /// </summary>
    [JsonPropertyName("spreading_factor")]
    public int SpreadingFactor { get; init; }
}
