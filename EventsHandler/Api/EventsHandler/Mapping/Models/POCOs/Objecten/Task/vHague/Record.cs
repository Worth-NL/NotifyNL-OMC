// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using EventsHandler.Mapping.Models.Interfaces;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague
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
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("data")]
        [JsonPropertyOrder(0)]
        public Data Data { get; internal set; }
    }
}