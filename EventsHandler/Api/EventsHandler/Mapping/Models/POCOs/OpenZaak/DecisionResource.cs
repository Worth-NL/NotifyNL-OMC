// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The decision resource retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct DecisionResource : IJsonSerializable
    {
        /// <summary>
        /// Gets the URL of the <see cref="InfoObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("informatieobject")]
        [JsonPropertyOrder(0)]
        public Uri InfoObjectUrl { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Gets the URL of the Decision.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("besluit")]
        [JsonPropertyOrder(1)]
        public Uri DecisionUrl { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionResource"/> struct.
        /// </summary>
        public DecisionResource()
        {
        }
    }
}