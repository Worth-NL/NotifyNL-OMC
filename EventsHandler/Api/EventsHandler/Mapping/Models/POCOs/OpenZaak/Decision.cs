// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The decision related to the <see cref="DecisionResource"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Decision : IJsonSerializable
    {
        /// <summary>
        /// The type of the <see cref="Decision"/> in <see cref="Uri"/> format.
        /// </summary>
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        [JsonInclude]
        [JsonPropertyName("besluittype")]
        [JsonPropertyOrder(0)]
        public Uri TypeUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// The reference to the <see cref="Case"/> in <see cref="Uri"/> format.
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(1)]
        public Uri CaseUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainObject"/> struct.
        /// </summary>
        public Decision()
        {
        }
    }
}