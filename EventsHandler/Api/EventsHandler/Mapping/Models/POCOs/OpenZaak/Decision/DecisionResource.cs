// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// The decision resource retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct DecisionResource : IJsonSerializable
    {
        /// <summary>
        /// The reference to the <see cref="InfoObject"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("informatieobject")]
        [JsonPropertyOrder(0)]
        public Uri InfoObjectUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// The reference to the <see cref="Decision"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("besluit")]
        [JsonPropertyOrder(1)]
        public Uri DecisionUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionResource"/> struct.
        /// </summary>
        public DecisionResource()
        {
        }
    }
}