using System.Runtime.Serialization;

namespace TTNet.Data;

/// <summary>
/// Message schedule
/// </summary>
public enum Schedule
{
    /// <summary>
    /// Replace any message in the queue.
    /// </summary>
    [EnumMember(Value = "replace")]
    Replace,
    /// <summary>
    /// Enqueue a message.
    /// </summary>
    [EnumMember(Value = "push")]
    Push
}
