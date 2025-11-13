// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.KTO
{
    /// <summary>
    /// The task retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct KtoObject : IJsonSerializable
    {
        /// <summary>
        /// The record related to the <see cref="KtoObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("record")]
        [JsonPropertyOrder(0)]
        public Record Record { get; [UsedImplicitly] set; }
    }
}