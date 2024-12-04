// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.Objecten;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Task.vHague
{
    /// <summary>
    /// The data related to the <see cref="Record"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by The Hague.
    /// </remarks>
    /// <seealso cref="CommonTaskData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <inheritdoc cref="CommonTaskData.CaseId"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(0)]
        public Uri CaseUri { get; public set; } = CommonValues.Default.Models.EmptyUri;

        /// <inheritdoc cref="CommonTaskData.Title"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("title")]
        [JsonPropertyOrder(1)]
        public string Title { get; public set; } = string.Empty;

        /// <inheritdoc cref="CommonTaskData.Status"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(2)]
        public TaskStatuses Status { get; public set; }

        /// <inheritdoc cref="CommonTaskData.ExpirationDate"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("verloopdatum")]
        [JsonPropertyOrder(3)]
        public DateTime ExpirationDate { get; public set; }

        /// <inheritdoc cref="CommonTaskData.Identification"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(4)]
        public Identification Identification { get; public set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}