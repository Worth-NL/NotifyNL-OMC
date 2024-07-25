// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1
{
    /// <summary>
    /// The sensitive data about the citizen ("burger") retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="CommonPartyData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct CitizenResult : IJsonSerializable
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

        /// <inheritdoc cref="CommonPartyData.DistributionChannel"/>
        [JsonInclude]
        [JsonPropertyName("aanmaakkanaal")]
        [JsonPropertyOrder(3)]
        public DistributionChannels DistributionChannel { get; internal set; }

        /// <inheritdoc cref="CommonPartyData.TelephoneNumber"/>
        [JsonInclude]
        [JsonPropertyName("telefoonnummer")]
        [JsonPropertyOrder(4)]
        public string TelephoneNumber { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.EmailAddress"/>
        [JsonInclude]
        [JsonPropertyName("emailadres")]
        [JsonPropertyOrder(5)]
        public string EmailAddress { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CitizenResult"/> struct.
        /// </summary>
        public CitizenResult()
        {
        }
    }
}