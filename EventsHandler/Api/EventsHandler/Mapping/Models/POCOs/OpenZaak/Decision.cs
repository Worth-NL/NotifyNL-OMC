// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The decision retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Decision : IJsonSerializable
    {
        /// <summary>
        /// Gets the URL of the <see cref="InfoObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("informatieobject")]
        [JsonPropertyOrder(0)]
        public Uri InfoObjectUrl { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Gets the URL of the <see cref="Decision"/>.
        /// </summary>
        /// <remarks>
        /// The same as "hoofdObject" from <see cref="NotificationEvent"/>
        /// </remarks>
        [JsonInclude]
        [JsonPropertyName("besluit")]
        [JsonPropertyOrder(1)]
        public Uri DecisionUrl { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="Decision"/> struct.
        /// </summary>
        public Decision()
        {
        }
    }
}