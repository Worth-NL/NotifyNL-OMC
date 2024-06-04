// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The additional (expanded) information about the party retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct Expansion : IJsonSerializable
    {
        /// <inheritdoc cref="DigitalAddressLong"/>
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