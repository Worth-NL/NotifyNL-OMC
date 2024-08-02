// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten
{
    /// <summary>
    /// The identification related to the <see cref="Data"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Identification : IJsonSerializable
    {
        /// <summary>
        /// Gets the type of the <see cref="Identification"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("type")]
        [JsonPropertyOrder(0)]
        public IdTypes Type { get; internal set; }

        /// <summary>
        /// Gets the value of the <see cref="Identification"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("value")]
        [JsonPropertyOrder(1)]
        public string Value { get; internal set; }
    }
}