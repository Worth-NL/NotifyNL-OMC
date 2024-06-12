// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant
{
    /// <summary>
    /// The response from "OpenKlant" feedback API endpoint.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct ContactMoment : IJsonSerializable
    {
        /// <summary>
        /// The ID of the <see cref="ContactMoment"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; internal set; } = Guid.Empty;

        /// <summary>
        /// The URL to the <see cref="ContactMoment"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(1)]
        public Uri Url { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactMoment"/> struct.
        /// </summary>
        public ContactMoment()
        {
        }
    }
}