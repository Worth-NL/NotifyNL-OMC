// © 2023, Worth Systems.

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
        [JsonInclude]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Uri { get; public set; } = CommonValues.Default.Models.EmptyUri;

        /// <inheritdoc cref="CommonPartyData.Name"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("voornaam")]
        [JsonPropertyOrder(1)]
        public string Name { get; public set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.SurnamePrefix"/>
        [JsonInclude]
        [JsonPropertyName("voorvoegselAchternaam")]
        [JsonPropertyOrder(2)]
        public string SurnamePrefix { get; public set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.Surname"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("achternaam")]
        [JsonPropertyOrder(3)]
        public string Surname { get; public set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.DistributionChannel"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("aanmaakkanaal")]
        [JsonPropertyOrder(4)]
        public DistributionChannels DistributionChannel { get; public set; }

        /// <inheritdoc cref="CommonPartyData.TelephoneNumber"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("telefoonnummer")]
        [JsonPropertyOrder(5)]
        public string TelephoneNumber { get; public set; } = string.Empty;

        /// <inheritdoc cref="CommonPartyData.EmailAddress"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("emailadres")]
        [JsonPropertyOrder(6)]
        public string EmailAddress { get; public set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public PartyResult()
        {
        }
    }
}