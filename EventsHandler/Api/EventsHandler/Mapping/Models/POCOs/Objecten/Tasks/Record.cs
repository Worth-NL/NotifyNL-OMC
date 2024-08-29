// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Tasks
{
    /// <summary>
    /// The record related to the <see cref="TaskObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Record : IJsonSerializable
    {
        /// <summary>
        /// The data related to the <see cref="Record"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("data")]
        [JsonPropertyOrder(0)]
        public Data Data { get; internal set; }
    }
}