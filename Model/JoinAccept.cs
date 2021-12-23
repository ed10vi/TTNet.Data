using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TTNet.Data.Model;

/// <summary>
/// Join data.
/// </summary>
public class JoinAccept
{
    /// <summary>
    /// Join Server issued identifier for the session keys.
    /// </summary>
    [JsonPropertyName("session_key_id")]
    public byte[] SessionKeyId { get; init; } = null!;

    /// <summary>
    /// Timestamp at which the uplink has been received by the Network Server.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset ReceivedAt { get; private set; }

    /// <summary>
    /// ISO 8601 UTC timestamp at which the uplink has been received by the Network Server.
    /// </summary>
    [JsonPropertyName("received_at"), EditorBrowsable(EditorBrowsableState.Never)]
    public string _ReceivedAt
    {
        get => ReceivedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        set => ReceivedAt = DateTimeOffset.Parse(value);
    }
}
