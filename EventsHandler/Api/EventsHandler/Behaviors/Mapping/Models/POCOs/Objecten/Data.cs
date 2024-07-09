// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.Objecten;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.Objecten
{
    /// <summary>
    /// The data related to the <see cref="Record"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <summary>
        /// Gets the case <see cref="Uri"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(0)]
        public Uri CaseUrl { get; internal set; } = DefaultValues.Models.EmptyUri;
        
        /// <summary>
        /// Gets the status of the task.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(1)]
        public TaskStatuses Status { get; internal set; }
        
        /// <summary>
        /// Gets the deadline by which the task should be completed.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("verloopdatum")]
        [JsonPropertyOrder(2)]
        public DateTime ExpirationDate { get; internal set; }
        
        /// <summary>
        /// Gets the identification details of the task.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(3)]
        public Identification Identification { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}