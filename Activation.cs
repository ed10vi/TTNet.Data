using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

using System;

namespace TTNet.Data
{
    /// <summary>
    /// An activation message
    /// </summary>
    public class Activation
    {
        [JsonPropertyName("app_eui")]
        private string appEui
        {
            get => AppEUI?.ToHexString();
            set => AppEUI = value?.HexToByteArray();
        }
        /// <summary>
        /// EUI of the application.
        /// </summary>
        [JsonIgnore] public byte[] AppEUI { get; private set; }

        [JsonPropertyName("dev_eui")]
        private string deviceEui
        {
            get => DeviceEUI?.ToHexString();
            set => DeviceEUI = value?.HexToByteArray();
        }
        /// <summary>
        /// EUI of the device.
        /// </summary>
        [JsonIgnore] public byte[] DeviceEUI { get; private set; }

        [JsonPropertyName("dev_addr")]
        private string deviceAddress
        {
            get => DeviceAddress?.ToHexString();
            set => DeviceAddress = value?.HexToByteArray();
        }
        /// <summary>
        /// Address of the device.
        /// </summary>
        [JsonIgnore]public byte[] DeviceAddress { get; private set; }

        /// <summary>
        /// Message's <see cref="T:TTNet.Data.Metadata"/>.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; private set; }
    }
}