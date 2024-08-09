// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;
using EventsHandler.Extensions;

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

        /// <summary>
        /// Gets <see cref="InfoObject"/> <see cref="Uri"/>s extracted from <see cref="Document"/>s.
        /// </summary>
        /// <returns>
        ///   The collection of references to <see cref="InfoObject"/> in <see cref="Uri"/> format.
        /// </returns>
        internal IReadOnlyCollection<Uri> GetInfoObjectUris()
        {
            if (this.Results.IsEmpty())
            {
                return Array.Empty<Uri>();
            }

            return this.Results
                .Select(document => document.InfoObjectUri)
                .ToArray();
        }
    }
}