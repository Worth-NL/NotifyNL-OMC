// © 2023, Worth Systems.

using Common.Constants;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v1
{
    /// <summary>
    /// The sensitive data about the party (e.g., citizen or organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="CommonPartyData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct PartyResult : IJsonSerializable
    {
        /// <inheritdoc cref="CommonPartyData.Uri"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Uri { get; internal set; } = CommonValues.Default.Models.EmptyUri;

        /// <inheritdoc cref="CommonPartyData.Name"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("voornaam")]
        [JsonPropertyOrder(1)]
        public string Name { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.SurnamePrefix"/>
        [JsonInclude]
        [JsonPropertyName("voorvoegselAchternaam")]
        [JsonPropertyOrder(2)]
        public string SurnamePrefix { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.Surname"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("achternaam")]
        [JsonPropertyOrder(3)]
        public string Surname { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.DistributionChannel"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("aanmaakkanaal")]
        [JsonPropertyOrder(4)]
        public DistributionChannels DistributionChannel { get; internal set; }

        /// <inheritdoc cref="CommonPartyData.TelephoneNumber"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("telefoonnummer")]
        [JsonPropertyOrder(5)]
        public string TelephoneNumber { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.EmailAddress"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("emailadres")]
        [JsonPropertyOrder(6)]
        public string EmailAddress { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public PartyResult()
        {
        }
    }
}