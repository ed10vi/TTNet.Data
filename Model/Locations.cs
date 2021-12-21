using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Location metadata.
/// </summary>
public class Locations
{
    /// <summary>
    /// User location.
    /// </summary>
    [JsonPropertyName("user")]
    public Location User { get; init; } = null!;
}
