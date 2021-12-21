using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Error that was encountered.
/// </summary>
public class Error
{
    /// <summary>
    /// Component in which the error occurred.
    /// </summary>
    [JsonPropertyName("namespace")]
    public string Namespace { get; init; } = null!;

    /// <summary>
    /// Error ID.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Error message.
    /// </summary>
    [JsonPropertyName("message_format")]
    public string MessageFormat { get; init; } = null!;

    /// <summary>
    /// Correlation identifiers of the error.
    /// </summary>
    [JsonPropertyName("correlation_id")]
    public string CorrelationId { get; init; } = null!;

    /// <summary>
    /// gRPC error code.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; init; }
}
