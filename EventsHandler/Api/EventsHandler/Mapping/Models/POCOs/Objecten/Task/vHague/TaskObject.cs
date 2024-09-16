// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using EventsHandler.Mapping.Models.Interfaces;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague
{
    /// <summary>
    /// The task retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct TaskObject : IJsonSerializable
    {
        /// <summary>
        /// The record related to the <see cref="TaskObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("record")]
        [JsonPropertyOrder(0)]
        public Record Record { get; internal set; }
    }
}