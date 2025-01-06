// © 2024, Worth Systems.

using Common.Constants;
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
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(0)]
        public Uri CaseUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <inheritdoc cref="CommonTaskData.Title"/>
        [JsonRequired]
        [JsonPropertyName("title")]
        [JsonPropertyOrder(1)]
        public string Title { get; set; } = string.Empty;

        /// <inheritdoc cref="CommonTaskData.Status"/>
        [JsonRequired]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(2)]
        public TaskStatuses Status { get; set; }

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