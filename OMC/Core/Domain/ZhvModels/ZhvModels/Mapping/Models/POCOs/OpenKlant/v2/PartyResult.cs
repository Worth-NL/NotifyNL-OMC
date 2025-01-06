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
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public PartyResult()
        {
        }
    }
}