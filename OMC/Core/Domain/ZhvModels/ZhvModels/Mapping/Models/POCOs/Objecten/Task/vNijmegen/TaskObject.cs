// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Task.vNijmegen
{
    /// <summary>
    /// The task retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by Nijmegen.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct TaskObject : IJsonSerializable
    {
        /// <summary>
        /// The record related to the <see cref="TaskObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("record")]
        [JsonPropertyOrder(0)]
        public Record Record { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskObject"/> struct.
        /// </summary>
        public TaskObject()
        {
        }
    }
}