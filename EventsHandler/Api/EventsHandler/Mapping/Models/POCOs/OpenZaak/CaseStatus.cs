// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// A single status from <see cref="CaseStatuses"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseStatus : IJsonSerializable
    {
        /// <summary>
        /// The type of the status.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("statustype")]
        [JsonPropertyOrder(0)]
        public Uri Type { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// The date and time when the status was created.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("datumStatusGezet")]
        [JsonPropertyOrder(1)]
        public DateTime Created { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatus"/> struct.
        /// </summary>
        public CaseStatus()
        {
        }
    }
}