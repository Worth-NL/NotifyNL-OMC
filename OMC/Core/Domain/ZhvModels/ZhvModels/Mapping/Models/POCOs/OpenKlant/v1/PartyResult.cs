// © 2023, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
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
        public DistributionChannels DistributionChannel { get; [UsedImplicitly] set; }

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
        /// The identification details of the subject.
        /// </summary>
        [JsonPropertyName("subjectIdentificatie")]
        [JsonPropertyOrder(15)]
        public SubjectIdentification SubjectIdentification { get; [UsedImplicitly] set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public PartyResult()
        {
        }
    }

    /// <summary>
    /// Represents the identification details of a subject (person or organization) in the "OpenKlant" system.
    /// </summary>
    public struct SubjectIdentification
    {
        /// <summary>
        /// Gets or sets the Burger Service Nummer (BSN), which is a unique personal number for Dutch citizens.
        /// </summary>
        [JsonPropertyName("inpBsn")]
        public string Bsn { get; set; }

        /// <summary>
        /// Gets or sets the Administrative Number Plate (ANP) identification, used for administrative purposes.
        /// </summary>
        [JsonPropertyName("anpIdentificatie")]
        public string AnpIdentification { get; set; }

        /// <summary>
        /// Gets or sets the A-number, which is an identification number for foreign nationals in the Netherlands.
        /// </summary>
        [JsonPropertyName("inpANummer")]
        public string ANumber { get; set; }

        /// <summary>
        /// Gets or sets the gender indication of the subject.
        /// Possible values might include: "m" (male), "v" (female), "o" (other), or empty.
        /// </summary>
        [JsonPropertyName("geslachtsaanduiding")]
        public string Gender { get; [UsedImplicitly] set; }
    }
}