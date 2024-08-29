// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task
{
    /// <summary>
    /// The data related to the <see cref="Record"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <summary>
        /// The reference to <see cref="Case"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(0)]
        public Uri CaseUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// The title of the <see cref="TaskObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("title")]
        [JsonPropertyOrder(1)]
        public string Title { get; internal set; } = string.Empty;

        /// <summary>
        /// The status of the <see cref="TaskObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(2)]
        public TaskStatuses Status { get; internal set; }

        /// <summary>
        /// The deadline by which the <see cref="TaskObject"/> should be completed.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("verloopdatum")]
        [JsonPropertyOrder(3)]
        public DateTime ExpirationDate { get; internal set; }

        /// <summary>
        /// The identification details of the <see cref="TaskObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(4)]
        public Identification Identification { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}