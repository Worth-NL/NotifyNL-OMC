// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The details of digital address retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct DigitalAddressLong : IJsonSerializable
    {
        /// <summary>
        /// The value of the digital address.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("adres")]
        [JsonPropertyOrder(0)]
        public string Address { get; internal set; } = string.Empty;
        
        /// <summary>
        /// The type of the digital address.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("soortDigitaalAdres")]
        [JsonPropertyOrder(1)]
        public string Type { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalAddressLong"/> struct.
        /// </summary>
        public DigitalAddressLong()
        {
        }
    }
}