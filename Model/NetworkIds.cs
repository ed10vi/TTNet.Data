using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// IDs of the network.
/// </summary>
public class NetworkIds
{
    /// <summary>
    /// Network ID.
    /// </summary>
    [JsonPropertyName("net_id")]
    public string NetId { get; init; } = null!;

    /// <summary>
    /// Tenant ID.
    /// </summary>
    [JsonPropertyName("tenant_id")]
    public string TenantId { get; init; } = null!;

    /// <summary>
    /// Cluster ID.
    /// </summary>
    [JsonPropertyName("cluster_id")]
    public string ClusterId { get; init; } = null!;
}
