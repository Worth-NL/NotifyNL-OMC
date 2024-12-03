// © 2024, Worth Systems.

using Common.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v2
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
        [JsonInclude]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Uri { get; internal set; } = CommonValues.Default.Models.EmptyUri;

        /// <inheritdoc cref="DigitalAddressShort"/>
        /// <remarks>
        /// Preferred by the user.
        /// </remarks>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("voorkeursDigitaalAdres")]
        [JsonPropertyOrder(1)]
        public DigitalAddressShort PreferredDigitalAddress { get; internal set; }

        /// <inheritdoc cref="PartyIdentification"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("partijIdentificatie")]
        [JsonPropertyOrder(2)]
        public PartyIdentification Identification { get; internal set; }

        /// <inheritdoc cref="v2.Expansion"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("_expand")]
        [JsonPropertyOrder(3)]
        public Expansion Expansion { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public PartyResult()
        {
        }
    }
}