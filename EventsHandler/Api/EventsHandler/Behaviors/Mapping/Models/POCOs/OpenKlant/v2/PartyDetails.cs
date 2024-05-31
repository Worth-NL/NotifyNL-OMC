// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The details about the party (e.g., citizen, organization) retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct PartyDetails : IJsonSerializable
    {
        /// <inheritdoc cref="CommonPartyData.Name"/>
        [JsonInclude]
        [JsonPropertyName("voornaam")]
        [JsonPropertyOrder(0)]
        public string Name { get; internal set; } = string.Empty;
        
        /// <inheritdoc cref="CommonPartyData.SurnamePrefix"/>
        [JsonInclude]
        [JsonPropertyName("voorvoegselAchternaam")]
        [JsonPropertyOrder(1)]
        public string SurnamePrefix { get; internal set; } = string.Empty;
        
        /// <inheritdoc cref="CommonPartyData.Surname"/>
        [JsonInclude]
        [JsonPropertyName("achternaam")]
        [JsonPropertyOrder(2)]
        public string Surname { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyDetails"/> struct.
        /// </summary>
        public PartyDetails()
        {
        }
    }
}