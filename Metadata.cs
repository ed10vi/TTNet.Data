using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TTNet.Data
{
    /// <summary>
    /// Modulation type.
    /// </summary>
    public enum Modulation
    {
        /// <summary>
        /// LoRa modulation.
        /// </summary>
        [EnumMember(Value = "LORA")]
        LoRa,
        /// <summary>
        /// FSK modulation.
        /// </summary>
        [EnumMember(Value = "FSK")]
        FSK
    }

    /// <summary>
    /// Extra information about a message.
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Airtime in nanoseconds.
        /// </summary>
        [JsonPropertyName("airtime")]
        public int Airtime { get; private set; }

        [JsonPropertyName("time")]
        private string TimeRaw
        {
            get { return Time.ToString("yyyy-MM-ddTHH:mm:ssK"); }
            set { Time = DateTime.Parse(value); }
        }
        /// <summary>
        /// Time when the server received the message.
        /// </summary>
        [JsonIgnore] public DateTime Time { get; private set; }

        /// <summary>
        /// Frequency at which the message was sent.
        /// </summary>
        [JsonPropertyName("frequency")]
        public float Frequency { get; private set; }

        [JsonPropertyName("modulation")]
        private string ModulationRaw
        {
            get { return Modulation.GetAttribute<EnumMemberAttribute>().Value; }
            set { Modulation = EnumAttributeGetter.GetByAttribute<EnumMemberAttribute, Modulation>(t => t.Value == value); }
        }
        /// <summary>
        /// Modulation that was used.
        /// </summary>
        [JsonIgnore] public Modulation Modulation { get; private set; }

        /// <summary>
        /// Data rate that was used - if LoRa modulation.
        /// </summary>
        [JsonPropertyName("data_rate")]
        public string DataRate { get; private set; }

        /// <summary>
        /// Bit rate that was used - if FSK modulation.
        /// </summary>
        [JsonPropertyName("bit_rate")]
        public int BitRate { get; private set; }

        /// <summary>
        /// Coding rate that was used.
        /// </summary>
        [JsonPropertyName("coding_rate")]
        public string CodingRate { get; private set; }

        /// <summary>
        /// Gateways that received this message.
        /// </summary>
        [JsonPropertyName("gateways")]
        public Gateway[] Gateways { get; private set; }

        /// <summary>
        /// Latitude of the device.
        /// </summary>
        [JsonPropertyName("latitude")]
        public float Latitude { get; private set; }

        /// <summary>
        /// Longitude of the device.
        /// </summary>
        [JsonPropertyName("longitude")]
        public float Longitude { get; private set; }

        /// <summary>
        /// Altitude of the device.
        /// </summary>
        [JsonPropertyName("altitude")]
        public float Altitude { get; private set; }
        
        /// <summary>
        /// Gets the erlier time between the server and gateways.
        /// </summary>
        [JsonIgnore] public DateTime MinTime
        {
            get
            {
                var time = Time;
                DateTime gwTime;
                var gateways = Gateways?.Where(gw => gw.Time != null);
                if (gateways != null && gateways.Count() > 0)
                {
                    gateways = gateways.OrderBy(gw => gw.Time);
                    gwTime = (DateTime)gateways.First().Time;
                    if (gwTime < time)
                        time = gwTime;
                }
                return time;
            }
        }
    }
}
