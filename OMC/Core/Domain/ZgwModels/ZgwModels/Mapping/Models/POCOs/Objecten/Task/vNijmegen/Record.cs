// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.Objecten.Task.vNijmegen
{
    /// <summary>
    /// The record related to the <see cref="TaskObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by Nijmegen.
    /// </remarks>
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