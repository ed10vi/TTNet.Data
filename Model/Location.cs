using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Location metadata.
/// </summary>
public class Location
{
    /// <summary>
    /// Location latitude.
    /// </summary>
    [JsonPropertyName("latitude")]
    public float Latitude { get; init; }

    /// <summary>
    /// Location longitude.
    /// </summary>
    [JsonPropertyName("longitude")]
    public float Longitude { get; init; }

    /// <summary>
    /// Location altitude.
    /// </summary>
    [JsonPropertyName("altitude")]
    public int Altitude { get; init; }

    /// <summary>
    /// Location source.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; init; } = null!;
}
