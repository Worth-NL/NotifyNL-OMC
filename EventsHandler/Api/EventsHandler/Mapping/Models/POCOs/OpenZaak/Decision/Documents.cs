// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// The documents liked to the <see cref="Decision"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Documents : IJsonSerializable
    {
        /// <summary>
        /// The collection of:
        /// <inheritdoc cref="Document"/>
        /// </summary>
        [JsonInclude]
        // This property is unnamed in the resulting JSON
        [JsonPropertyOrder(0)]
        public List<Document> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Documents"/> struct.
        /// </summary>
        public Documents()
        {
        }
    }
}