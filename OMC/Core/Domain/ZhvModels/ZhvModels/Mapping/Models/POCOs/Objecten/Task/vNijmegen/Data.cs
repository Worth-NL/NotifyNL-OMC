// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.Objecten;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Task.vNijmegen
{
    /// <summary>
    /// The data related to the <see cref="TaskObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by Nijmegen.
    /// </remarks>
    /// <seealso cref="CommonTaskData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <inheritdoc cref="CommonTaskData.Title"/>
        [JsonRequired]
        [JsonPropertyName("titel")]
        [JsonPropertyOrder(0)]
        public string Title { get; set; } = string.Empty;

        /// <inheritdoc cref="CommonTaskData.Status"/>
        [JsonRequired]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(1)]
        public TaskStatuses Status { get; set; }

        /// <inheritdoc cref="vNijmegen.Coupling"/>
        [JsonRequired]
        [JsonPropertyName("koppeling")]
        [JsonPropertyOrder(2)]
        public Coupling Coupling { get; set; }

        /// <inheritdoc cref="CommonTaskData.ExpirationDate"/>
        [JsonRequired]
        [JsonPropertyName("verloopdatum")]
        [JsonPropertyOrder(3)]
        public DateTime ExpirationDate { get; set; }

        /// <inheritdoc cref="CommonTaskData.Identification"/>
        [JsonRequired]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(4)]
        public Identification Identification { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}