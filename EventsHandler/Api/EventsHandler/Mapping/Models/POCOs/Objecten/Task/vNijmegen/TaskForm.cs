// // © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen
{
    /// <summary>
    /// The task form related to the <see cref="Data"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by Nijmegen.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct TaskForm : IJsonSerializable
    {
        /// <inheritdoc cref="vNijmegen.Coupling"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("koppeling")]
        [JsonPropertyOrder(0)]
        public Coupling Coupling { get; internal set; }

        /// <inheritdoc cref="CommonTaskData.ExpirationDate"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("verloopdatum")]
        [JsonPropertyOrder(1)]
        public DateTime ExpirationDate { get; internal set; }

        /// <inheritdoc cref="CommonTaskData.Identification"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(2)]
        public Identification Identification { get; internal set; }
    }
}