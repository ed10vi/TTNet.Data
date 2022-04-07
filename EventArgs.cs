using System;
using TTNet.Data.Model;

namespace TTNet.Data;

/// <summary>
/// Event arguments for a generic mesage.
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// MQTT topic.
    /// </summary>
    public string Topic { get; init; }

    /// <summary>
    /// MQTT topic fields.
    /// </summary>
    /// <value><see cref="TTNet.Data.MessageReceivedEventArgs.Topic"/> splitted.</value>
    public string[] TopicFields { get; init; }

    /// <summary>
    /// Application identifier.
    /// </summary>
    public string AppID { get; init; }

    /// <summary>
    /// Tenant identifier.
    /// </summary>
    public string? TenantID { get; init; }

    /// <summary>
    /// Device identifier.
    /// </summary>
    public string DeviceID => TopicFields[3];


    /// <summary>
    /// The message received.
    /// </summary>
    public Message Message { get; init; }

    internal MessageReceivedEventArgs(Message msg, string topic, string[] topicFields)
    {
        string[] apptenant = topicFields[1].Split('@');
        Message = msg;
        Topic = topic;
        TopicFields = topicFields;
        AppID = apptenant[0];
        if (apptenant.Length > 1)
            TenantID = apptenant[1];
    }
}
