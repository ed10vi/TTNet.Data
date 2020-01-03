using System;
using System.Text.Json;

namespace TTNet.Data
{
    /// <summary>
    /// Event arguments for a generic update.
    /// </summary>
    public class UpdateReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// MQTT topic.
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// MQTT topic fields.
        /// </summary>
        /// <value><see cref="TTNet.Data.UpdateReceivedEventArgs.Topic"/> splitted.</value>
        public string[] TopicFields { get; private set; }

        /// <summary>
        /// Application identifier.
        /// </summary>
        public string AppID => TopicFields[0];

        /// <summary>
        /// Device identifier.
        /// </summary>
        public string DeviceID => TopicFields[2];

        internal UpdateReceivedEventArgs(string topic, string[] topicFields)
        {
            Topic = topic;
            TopicFields = topicFields;
        }
    }

    /// <summary>
    /// Event arguments for a message received.
    /// </summary>
    public class MessageReceivedEventArgs : UpdateReceivedEventArgs
    {
        /// <summary>
        /// The message.
        /// </summary>
        public Message<JsonElement> Message { get; private set; }

        internal MessageReceivedEventArgs(Message<JsonElement> msg, string topic, string[] topicFields) : base(topic, topicFields)
        {
            Message = msg;
        }
    }

    /// <summary>
    /// Event arguments for an update from a sent message.
    /// </summary>
    public class MessageInfoReceivedEventArgs : UpdateReceivedEventArgs
    {
        /// <summary>
        /// The information about the message.
        /// </summary>
        public MessageInfo MessageInfo { get; private set; }

        internal MessageInfoReceivedEventArgs(MessageInfo msgInf, string topic, string[] topicFields) : base(topic, topicFields)
        {
            MessageInfo = msgInf;
        }
    }

    /// <summary>
    /// Event arguments for a device activation received.
    /// </summary>
    public class ActivationReceivedEventArgs : UpdateReceivedEventArgs
    {
        /// <summary>
        /// The information about the activation.
        /// </summary>
        public Activation Activation { get; private set; }

        internal ActivationReceivedEventArgs(Activation activation, string topic, string[] topicFields) : base(topic, topicFields)
        {
            Activation = activation;
        }
    }

    /// <summary>
    /// Event arguments for a error received.
    /// </summary>
    public class ErrorReceivedEventArgs : UpdateReceivedEventArgs
    {
        /// <summary>
        /// The error.
        /// </summary>
        public Error Error { get; private set; }

        /// <summary>
        /// The type of error.
        /// </summary>
        public ErrorType Type { get; private set; }

        internal ErrorReceivedEventArgs(Error error, string topic, string[] topicFields) : base(topic, topicFields)
        {
            Error = error;
            switch (topicFields[4])
            {
                case "up":
                    Type = ErrorType.Up;
                    break;
                case "down":
                    Type = ErrorType.Down;
                    break;
                case "activations":
                    Type = ErrorType.Activations;
                    break;
            }
        }
    }
}