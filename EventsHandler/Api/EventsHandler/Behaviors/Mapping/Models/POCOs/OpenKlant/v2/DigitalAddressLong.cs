// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The details of digital address retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct DigitalAddressLong : IJsonSerializable
    {
        /// <summary>
        /// The UUID of the digital address.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; internal set; } = Guid.Empty;

        /// <summary>
        /// The value of the digital address.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("adres")]
        [JsonPropertyOrder(1)]
        public string Value { get; internal set; } = string.Empty;
        
        /// <summary>
        /// The type of the digital address.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("soortDigitaalAdres")]
        [JsonPropertyOrder(2)]
        public string Type { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalAddressLong"/> struct.
        /// </summary>
        public DigitalAddressLong()
        {
        }
    }
}