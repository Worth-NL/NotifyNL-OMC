// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The identification of the party (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct PartyIdentification : IJsonSerializable
    {
        /// <inheritdoc cref="PartyDetails"/>
        [JsonInclude]
        [JsonPropertyName("contactnaam")]
        [JsonPropertyOrder(0)]
        public PartyDetails Details { get; internal set; }
    }
}