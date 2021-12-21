using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Settings for the transmission.
/// </summary>
public class Settings
{
    /// <summary>
    /// Data rate settings.
    /// </summary>
    [JsonPropertyName("data_rate")]
    public DataRate DataRate { get; init; } = null!;

    /// <summary>
    /// LoRa coding rate.
    /// </summary>
    [JsonPropertyName("coding_rate")]
    public string CodingRate { get; init; } = null!;

    /// <summary>
    /// Frequency (Hz).
    /// </summary>
    [JsonPropertyName("frequency")]
    public string Frequency { get; init; } = null!;

    /// <summary>
    /// Gateway ID.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public ulong Timestamp { get; init; }

    /// <summary>
    /// Timestamp at which the message has been received.
    /// </summary>
    [JsonIgnore]
    public DateTime Time { get; private set; }

    /// <summary>
    /// ISO 8601 UTC timestamp at which the message has been received.
    /// </summary>
    [JsonPropertyName("time"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _Time
    {
        get => Time.ToString("yyyy-MM-ddTHH:mm:ssK");
        set => Time = DateTime.Parse(value);
    }
}
