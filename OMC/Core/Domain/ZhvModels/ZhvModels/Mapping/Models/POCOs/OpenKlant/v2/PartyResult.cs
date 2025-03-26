// © 2024, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The sensitive data about the party (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
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

        /// <inheritdoc cref="DigitalAddressShort"/>
        /// <remarks>
        /// Preferred by the user.
        /// </remarks>
        [JsonRequired]
        [JsonPropertyName("voorkeursDigitaalAdres")]
        [JsonPropertyOrder(1)]
        public DigitalAddressShort PreferredDigitalAddress { get; set; }

        /// <inheritdoc cref="PartyIdentification"/>
        [JsonRequired]
        [JsonPropertyName("partijIdentificatie")]
        [JsonPropertyOrder(2)]
        public PartyIdentification Identification { get; set; }

        /// <inheritdoc cref="v2.Expansion"/>
        [JsonRequired]
        [JsonPropertyName("_expand")]
        [JsonPropertyOrder(3)]
        public Expansion Expansion { get; set; }

        /// <summary>
        /// The identification details of the subject.
        /// </summary>
        [JsonPropertyName("subjectIdentificatie")]
        [JsonPropertyOrder(15)]
        public SubjectIdentification SubjectIdentification { get; set; }

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
        public string Gender { get; set; }
    }
}