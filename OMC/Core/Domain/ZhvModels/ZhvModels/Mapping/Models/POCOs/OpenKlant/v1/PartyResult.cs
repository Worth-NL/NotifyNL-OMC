// © 2023, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.OpenKlant;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant.v1
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
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Uri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <inheritdoc cref="CommonPartyData.Name"/>
        [JsonRequired]
        [JsonPropertyName("voornaam")]
        [JsonPropertyOrder(1)]
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.SurnamePrefix"/>
        [JsonPropertyName("voorvoegselAchternaam")]
        [JsonPropertyOrder(2)]
        public string SurnamePrefix { get; set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.Surname"/>
        [JsonRequired]
        [JsonPropertyName("achternaam")]
        [JsonPropertyOrder(3)]
        public string Surname { get; set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.DistributionChannel"/>
        [JsonRequired]
        [JsonPropertyName("aanmaakkanaal")]
        [JsonPropertyOrder(4)]
        public DistributionChannels DistributionChannel { get; set; }

        /// <inheritdoc cref="CommonPartyData.TelephoneNumber"/>
        [JsonRequired]
        [JsonPropertyName("telefoonnummer")]
        [JsonPropertyOrder(5)]
        public string TelephoneNumber { get; set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.EmailAddress"/>
        [JsonRequired]
        [JsonPropertyName("emailadres")]
        [JsonPropertyOrder(6)]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public PartyResult()
        {
        }
    }
}