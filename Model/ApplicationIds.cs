using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// IDs of the application.
/// </summary>
public class ApplicationIds
{
    /// <summary>
    /// Application ID.
    /// </summary>
    [JsonPropertyName("application_id")]
    public string ApplicationId { get; init; } = null!;
}
