using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// IDs of the gateway.
/// </summary>
public class GatewayIds
{
    /// <summary>
    /// Gateway ID.
    /// </summary>
    [JsonPropertyName("gateway_id")]
    public string GatewayId { get; init; } = null!;

    /// <summary>
    /// Gateway EUI.
    /// </summary>
    [JsonIgnore]
    public byte[] Eui { get; private set; } = null!;

    /// <summary>
    /// Gateway EUI.
    /// </summary>
    [JsonPropertyName("eui"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _Eui
    {
        get => Eui.ToHexString();
        set => Eui = value.HexToByteArray();
    }
}
