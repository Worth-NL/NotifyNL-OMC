// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Message
{
    /// <summary>
    /// The task retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct MessageObject : IJsonSerializable
    {
        /// <summary>
        /// The record related to the <see cref="MessageObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("record")]
        [JsonPropertyOrder(0)]
        public Record Record { get; public set; }
    }
}