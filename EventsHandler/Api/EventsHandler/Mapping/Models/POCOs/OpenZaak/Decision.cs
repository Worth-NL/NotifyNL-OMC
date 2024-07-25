// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
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
        /// Gets the URL of the <see cref="Case"/> type.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(0)]
        public Uri CaseUrl { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Gets the date when the decision will be in effect.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("ingangsdatum")]
        [JsonPropertyOrder(1)]
        public DateTime StartDate { get; internal set; }

        /// <summary>
        /// Gets the final date up to which user can respond / appeal to the <see cref="Decision"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("uiterlijkeReactiedatum")]
        [JsonPropertyOrder(2)]
        public DateTime FinalResponseDate { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decision"/> struct.
        /// </summary>
        public Decision()
        {
        }
    }
}