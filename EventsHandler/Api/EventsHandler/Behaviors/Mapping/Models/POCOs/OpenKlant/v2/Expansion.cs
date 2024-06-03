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
        public DigitalAddressLong[] DigitalAddresses { get; internal set; }
    }
}