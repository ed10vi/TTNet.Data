using System.Text.Json.Serialization;

namespace TTNet.Data
{
    /// <summary>
    /// A error message
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Error's description
        /// </summary>
        [JsonPropertyName("error")]
        public string Text { get; private set; }

        [JsonPropertyName("app_eui")]
        private string appEui
        {
            get { return AppEUI?.ToHexString(); }
            set { AppEUI = value?.HexToByteArray(); }
        }
        /// <summary>
        /// EUI of the application.
        /// </summary>
        [JsonIgnore] public byte[] AppEUI { get; private set; }

        [JsonPropertyName("dev_eui")]
        private string deviceEui
        {
            get { return DeviceEUI?.ToHexString(); }
            set { DeviceEUI = value?.HexToByteArray(); }
        }
        /// <summary>
        /// EUI of the device.
        /// </summary>
        [JsonIgnore] public byte[] DeviceEUI { get; private set; }
    }

    /// <summary>
    /// Describes the type of error
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// The error was produced during the device activation.
        /// </summary>
        Activations,
        /// <summary>
        /// The error was produced in the downlink.
        /// </summary>
        Down,
        /// <summary>
        /// The error was produced in the uplink.
        /// </summary>
        Up
    }
}
