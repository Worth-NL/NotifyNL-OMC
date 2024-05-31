// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The sensitive data about the party (e.g., citizen, organization) retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web service.
    /// </remarks>
    /// <seealso cref="CommonPartyData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct PartyResult : IJsonSerializable
    {
        /// <inheritdoc cref="DigitalAddressShort"/>
        [JsonInclude]
        [JsonPropertyName("voorkeursDigitaalAdres")]
        [JsonPropertyOrder(0)]
        public DigitalAddressShort DigitalAddress { get; internal set; }
        
        /// <inheritdoc cref="PartyIdentification"/>
        [JsonInclude]
        [JsonPropertyName("partijIdentificatie")]
        [JsonPropertyOrder(1)]
        public PartyIdentification Identification { get; internal set; }

        /// <inheritdoc cref="v2.Expansion"/>
        [JsonInclude]
        [JsonPropertyName("_expand")]
        [JsonPropertyOrder(2)]
        public Expansion Expansion { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public PartyResult()
        {
        }
    }
}