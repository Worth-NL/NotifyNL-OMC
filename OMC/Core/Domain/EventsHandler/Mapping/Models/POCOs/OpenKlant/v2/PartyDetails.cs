// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The details about the party (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct PartyDetails : IJsonSerializable
    {
        /// <inheritdoc cref="CommonPartyData.Name"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("voornaam")]
        [JsonPropertyOrder(0)]
        public string Name { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.SurnamePrefix"/>
        [JsonInclude]
        [JsonPropertyName("voorvoegselAchternaam")]
        [JsonPropertyOrder(1)]
        public string SurnamePrefix { get; internal set; } = string.Empty;  // NOTE: Will be absent if the company data is retrieved

        /// <inheritdoc cref="CommonPartyData.Surname"/>
        [JsonInclude]
        [JsonPropertyName("achternaam")]
        [JsonPropertyOrder(2)]
        public string Surname { get; internal set; } = string.Empty;  // NOTE: Will be absent if the company data is retrieved

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyDetails"/> struct.
        /// </summary>
        public PartyDetails()
        {
        }
    }
}