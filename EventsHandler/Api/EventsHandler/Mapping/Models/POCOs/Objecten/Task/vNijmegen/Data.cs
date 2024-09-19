// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen
{
    /// <summary>
    /// The data related to the <see cref="TaskObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by Nijmegen.
    /// </remarks>
    /// <seealso cref="CommonTaskData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <inheritdoc cref="CommonTaskData.Title"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("titel")]
        [JsonPropertyOrder(0)]
        public string Title { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonTaskData.Status"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(1)]
        public TaskStatuses Status { get; internal set; }

        /// <inheritdoc cref="vNijmegen.TaskForm"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("formtaak")]
        [JsonPropertyOrder(2)]
        public TaskForm TaskForm { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}