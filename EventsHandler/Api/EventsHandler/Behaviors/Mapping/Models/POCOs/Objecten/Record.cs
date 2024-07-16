// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.Objecten
{
    /// <summary>
    /// The record related to the <see cref="TaskObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Record : IJsonSerializable
    {
        /// <summary>
        /// Gets the data related to the <see cref="Record"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("data")]
        [JsonPropertyOrder(0)]
        public Data Data { get; internal set; }
    }
}