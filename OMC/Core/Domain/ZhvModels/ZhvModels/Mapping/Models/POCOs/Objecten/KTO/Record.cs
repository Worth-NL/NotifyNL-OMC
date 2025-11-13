// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.KTO
{
    /// <summary>
    /// The record related to the <see cref="KtoObject"/> retrieved from "Objecten" Web API service.
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
        public Data Data { get; [UsedImplicitly] set; }
    }
}