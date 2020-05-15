using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TTNet.Data
{
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
        /// Put befor any message in the queue.
        /// </summary>
        [EnumMember(Value = "first")]
        First,
        /// <summary>
        /// Put after any message in the queue.
        /// </summary>
        [EnumMember(Value = "last")]
        Last
    }

    /// <summary>
    /// A message whose payload fields are serialized from/to the given type.
    /// </summary>
    /// <typeparam name="T">A serializable type.</typeparam>
    public class Message<T>
    {
        /// <summary>
        /// Application identifier.
        /// </summary>
        [JsonPropertyName("app_id")]
        public string AppID { get; set; }

        /// <summary>
        /// Device identifier.
        /// </summary>
        [JsonPropertyName("dev_id")]
        public string DeviceID { get; set; }

        [JsonPropertyName("hardware_serial")]
        private string hardwareSerial
        {
            get => HardwareSerial?.ToHexString();
            set => HardwareSerial = value?.HexToByteArray();
        }
        /// <summary>
        /// In case of LoRaWAN: the DevEUI.
        /// </summary>
        [JsonIgnore] public byte[] HardwareSerial { get; private set; }

        /// <summary>
        /// LoRaWAN FPort.
        /// </summary>
        [JsonPropertyName("port")]
        public int? Port { get; set; }

        /// <summary>
        /// LoRaWAN frame counter.
        /// </summary>
        [JsonPropertyName("counter")]
        public int? Counter { get; private set; }

        /// <summary>
        /// Retry (you could also detect this from the counter).
        /// </summary>
        /// <value><c>true</c> if this message is a retry, otherwise <c>false</c>.</value>
        [JsonPropertyName("is_retry")]
        public bool? IsRetry { get; private set; }

        /// <summary>
        /// Confirmation
        /// </summary>
        /// <value><c>true</c> if this message was a confirmed or should be confirmed, otherwise <c>false</c>.</value>
        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }

        /// <summary>
        /// Raw payload
        /// </summary>
        [JsonPropertyName("payload_raw")]
        public byte[] PayloadRaw { get; set; }

        /// <summary>
        /// Object containing the results from the payload functions - left out when empty.
        /// </summary>
        [JsonPropertyName("payload_fields")]
        public T PayloadFields { get; set; }

        /// <summary>
        /// The <see cref="T:TTNet.Data.Metadata"/> of the message.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; private set; }

        [JsonPropertyName("schedule")]
        private string schedule
        {
            get => Schedule.GetAttribute<EnumMemberAttribute>().Value;
            set => Schedule = EnumAttributeGetter.GetByAttribute<EnumMemberAttribute, Schedule>(t => t.Value == value);
        }
        /// <summary>
        /// The <see cref="T:TTNet.Data.Schedule"/> for the message.
        /// </summary>
        [JsonIgnore] public Schedule Schedule { get; set; }
    }
}