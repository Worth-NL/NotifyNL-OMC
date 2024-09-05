// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten
{
    /// <summary>
    /// The identification related to the different Data (associated with <see cref="TaskObject"/>,
    /// <see cref="MessageObject"/>, etc.) retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Identification : IJsonSerializable
    {
        /// <summary>
        /// The type of the <see cref="Identification"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("type")]
        [JsonPropertyOrder(0)]
        public IdTypes Type { get; internal set; }

        /// <summary>
        /// The value of the <see cref="Identification"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("value")]
        [JsonPropertyOrder(1)]
        public string Value { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Identification"/> struct.
        /// </summary>
        public Identification()
        {
        }
    }
}