// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Message
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
        [JsonRequired]
        [JsonPropertyName("onderwerp")]
        [JsonPropertyOrder(0)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// The actions perspective of the <see cref="MessageObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("handelingsperspectief")]
        [JsonPropertyOrder(1)]
        public string ActionsPerspective { get; set; } = string.Empty;

        /// <summary>
        /// The identification details of the <see cref="MessageObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(2)]
        public Identification Identification { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}