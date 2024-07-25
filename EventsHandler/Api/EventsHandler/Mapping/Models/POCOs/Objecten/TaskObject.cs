// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten
{
    /// <summary>
    /// The task retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct TaskObject : IJsonSerializable
    {
        /// <summary>
        /// Gets the record related to the <see cref="TaskObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("record")]
        [JsonPropertyOrder(0)]
        public Record Record { get; internal set; }
    }
}