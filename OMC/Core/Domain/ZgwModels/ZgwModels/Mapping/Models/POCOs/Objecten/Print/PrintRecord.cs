// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.Objecten.Print
{
    /// <summary>
    /// The record related to the <see cref="PrintObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct PrintRecord : IJsonSerializable
    {
        /// <summary>
        /// The data related to the <see cref="PrintRecord"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("data")]
        [JsonPropertyOrder(0)]
        public PrintData Data { get; [UsedImplicitly] set; }
    }
}
