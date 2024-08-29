// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The additional (expanded) information about the party retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct Expansion : IJsonSerializable
    {
        /// <inheritdoc cref="DigitalAddressLong"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("digitaleAdressen")]
        [JsonPropertyOrder(0)]
        public List<DigitalAddressLong> DigitalAddresses { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Expansion"/> struct.
        /// </summary>
        public Expansion()
        {
        }
    }
}