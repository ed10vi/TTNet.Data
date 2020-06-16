using System;
using System.Text.Json.Serialization;

namespace TTNet.Data
{
    /// <summary>
    /// Gateway data.
    /// </summary>
    public class Gateway
    {
        /// <summary>
        /// EUI of the gateway
        /// </summary>
        [JsonPropertyName("gtw_id")]
        public string ID { get; private set; }

        /// <summary>
        /// Timestamp when the gateway received the message.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public uint Timestamp { get; private set; }

        [JsonPropertyName("time")]
        private string time
        {
            get { return Time?.ToString("yyyy-MM-ddTHH:mm:ssK"); }
            set
            {
                DateTime time;
                Time = value != null && DateTime.TryParse(value, out time) ? time : default(DateTime?);
            }
        }
        /// <summary>
        /// Time when the gateway received the message - left out when gateway does not have synchronized time.
        /// </summary>
        [JsonIgnore] public DateTime? Time { get; private set; }

        /// <summary>
        /// Channel where the gateway received the message.
        /// </summary>
        [JsonPropertyName("channel")]
        public byte Channel { get; private set; }

        /// <summary>
        /// Signal strength of the received message.
        /// </summary>
        [JsonPropertyName("rssi")]
        public short Rssi { get; private set; }

        /// <summary>
        /// Signal to noise ratio of the received message.
        /// </summary>
        [JsonPropertyName("snr")]
        public float Snr { get; private set; }

        /// <summary>
        /// RF chain where the gateway received the message.
        /// </summary>
        [JsonPropertyName("rf_chain")]
        public byte RfChain { get; private set; }

        /// <summary>
        /// Latitude of the gateway reported in its status updates.
        /// </summary>
        [JsonPropertyName("latitude")]
        public float Latitude { get; private set; }

        /// <summary>
        /// Longitude of the gateway reported in its status updates.
        /// </summary>
        [JsonPropertyName("longitude")]
        public float Longitude { get; private set; }

        /// <summary>
        /// Altitude of the gateway reported in its status updates.
        /// </summary>
        [JsonPropertyName("altitude")]
        public float Altitude { get; private set; }

        /// <summary>
        /// The source of the location data.
        /// </summary>
        [JsonPropertyName("location_source")]
        public string LocationSource { get; private set; }
    }
}
