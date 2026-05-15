// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.Objecten.Task.vHague
{
    /// <summary>
    /// The task retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by The Hague.
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
    }
}