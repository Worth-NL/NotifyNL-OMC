// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
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
        /// <summary>
        /// The ID of party (e.g., citizen, organization) in format:
        /// <code>
        /// http(s)://OpenKlantDomain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Id { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <inheritdoc cref="DigitalAddressShort"/>
        /// <remarks>
        /// Preferred by the user.
        /// </remarks>
        [JsonInclude]
        [JsonPropertyName("voorkeursDigitaalAdres")]
        [JsonPropertyOrder(1)]
        public DigitalAddressShort PreferredDigitalAddress { get; internal set; }

        /// <inheritdoc cref="PartyIdentification"/>
        [JsonInclude]
        [JsonPropertyName("partijIdentificatie")]
        [JsonPropertyOrder(2)]
        public PartyIdentification Identification { get; internal set; }

        /// <inheritdoc cref="v2.Expansion"/>
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