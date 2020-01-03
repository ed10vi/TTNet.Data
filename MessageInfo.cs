using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TTNet.Data
{
    /// <summary>
    /// Information about a sent message.
    /// </summary>
    public class MessageInfo
    {
        [JsonPropertyName("payload")]
        private string payload
        {
            get { return Payload != null ? Convert.ToBase64String(Payload) : null; }
            set { Payload = value != null ? Convert.FromBase64String(value) : null; }
        }
        /// <summary>
        /// The LoRaWAN packet.
        /// </summary>
        [JsonIgnore] public byte[] Payload { get; private set; }

        /// <summary>
        /// The message it self.
        /// </summary>
        [JsonPropertyName("message")]
        public Message<JsonElement> Message { get; private set; }

        /// <summary>
        /// EUI of the gateway.
        /// </summary>
        [JsonPropertyName("gateway_id")]
        public string GatewayID { get; private set; }

        /// <summary>
        /// The configuration used to send the message.
        /// </summary>
        [JsonPropertyName("config")]
        public MessageConfig Config { get; private set; }
    }

    /// <summary>
    /// Configuration used to send a message.
    /// </summary>
    public class MessageConfig
    {
        [JsonPropertyName("modulation")]
        private string modulation
        {
            get => Modulation.GetAttribute<EnumMemberAttribute>().Value;
            set => Modulation = EnumAttributeGetter.GetByAttribute<EnumMemberAttribute, Modulation>(t => t.Value == value);
        }
        /// <summary>
        /// <see cref="T:TTNet.Data.Modulation"/> that was used.
        /// </summary>
        [JsonIgnore] public Modulation Modulation { get; private set; }

        /// <summary>
        /// Data rate that was used.
        /// </summary>
        [JsonPropertyName("data_rate")]
        public string DataRate { get; private set; }

        /// <summary>
        /// Airtime in nanoseconds.
        /// </summary>
        [JsonPropertyName("airtime")]
        public int Airtime { get; private set; }

        /// <summary>
        /// LoRaWAN frame counter.
        /// </summary>
        [JsonPropertyName("counter")]
        public int Counter { get; private set; }

        /// <summary>
        /// Frequency at which the message was sent.
        /// </summary>
        [JsonPropertyName("frequency")]
        public float Frequency { get; private set; }

        /// <summary>
        /// Power.
        /// </summary>
        [JsonPropertyName("power")]
        public int Power { get; private set; }
    }
}
