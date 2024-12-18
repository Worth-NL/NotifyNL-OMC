// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Message
{
    /// <summary>
    /// The record related to the <see cref="MessageObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Record : IJsonSerializable
    {
        /// <summary>
        /// The data related to the <see cref="Record"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("data")]
        [JsonPropertyOrder(0)]
        public Data Data { get; set; }
    }
}