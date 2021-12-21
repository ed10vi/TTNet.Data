using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// IDs of the device.
/// </summary>
public class DeviceIds
{
    /// <summary>
    /// Device ID.
    /// </summary>
    [JsonPropertyName("device_id")]
    public string DeviceId { get; init; } = null!;

    /// <summary>
    /// IDs if the Application.
    /// </summary>
    [JsonPropertyName("application_ids")]
    public ApplicationIds ApplicationIds { get; init; } = null!;

    /// <summary>
    /// DevEUI of the end device.
    /// </summary>
    [JsonIgnore]
    public byte[] DeviceEui { get; private set; } = null!;

    /// <summary>
    /// DevEUI of the end device.
    /// </summary>
    [JsonPropertyName("dev_eui"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _DeviceEui
    {
        get => DeviceEui.ToHexString();
        set => DeviceEui = value.HexToByteArray();
    }

    /// <summary>
    /// JoinEUI of the end device (also known as AppEUI in LoRaWAN versions below 1.1).
    /// </summary>
    [JsonIgnore]
    public byte[] JoinEui { get; private set; } = null!;

    /// <summary>
    /// JoinEUI of the end device (also known as AppEUI in LoRaWAN versions below 1.1).
    /// </summary>
    [JsonPropertyName("join_eui"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _JoinEui
    {
        get => JoinEui.ToHexString();
        set => JoinEui = value.HexToByteArray();
    }

    /// <summary>
    /// Device address known by the Network Server.
    /// </summary>
    [JsonIgnore]
    public byte[] DeviceAddress { get; private set; } = null!;

    /// <summary>
    /// Device address known by the Network Server.
    /// </summary>
    [JsonPropertyName("dev_addr"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _DeviceAddress
    {
        get => DeviceAddress.ToHexString();
        set => DeviceAddress = value.HexToByteArray();
    }
}
