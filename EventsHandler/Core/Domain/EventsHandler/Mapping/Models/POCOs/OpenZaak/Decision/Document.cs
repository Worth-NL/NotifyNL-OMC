// © 2024, Worth Systems.

using Common.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// A single document from <see cref="Documents"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Document : IJsonSerializable
    {
        /// <summary>
        /// The reference to the <see cref="InfoObject"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("informatieobject")]
        [JsonPropertyOrder(0)]
        public Uri InfoObjectUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> struct.
        /// </summary>
        public Document()
        {
        }
    }
}