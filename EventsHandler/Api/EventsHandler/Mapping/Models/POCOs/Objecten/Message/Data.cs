// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Message
{
    /// <summary>
    /// The data related to the <see cref="Record"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <summary>
        /// The subject of the <see cref="MessageObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("onderwerp")]
        [JsonPropertyOrder(0)]
        public string Subject { get; internal set; } = string.Empty;

        /// <summary>
        /// The actions perspective of the <see cref="MessageObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("handelingsperspectief")]
        [JsonPropertyOrder(1)]
        public string ActionsPerspective { get; internal set; } = string.Empty;

        /// <summary>
        /// The identification details of the <see cref="MessageObject"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(2)]
        public Identification Identification { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}