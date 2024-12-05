// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.Objecten;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Task
{
    /// <summary>
    /// The data related to the Task (from vHague or vNijmegen versions) retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Common DTO for all versions of "Objecten" Web API service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public readonly struct CommonTaskData : IJsonSerializable
    {
        /// <summary>
        /// The reference to related object in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyOrder(0)]
        public Uri CaseUri { get; init; }  // NOTE: Some task data might not have it (recreating of URI is necessary)

        /// <summary>
        /// The ID of the related object.
        /// </summary>
        [JsonRequired]
        [JsonPropertyOrder(1)]
        public Guid CaseId { get; init; }  // NOTE: Some task data might have only URI (extracting GUID is necessary)

        /// <summary>
        /// The title of the task.
        /// </summary>
        [JsonRequired]
        [JsonPropertyOrder(2)]
        public string Title { get; init; }

        /// <summary>
        /// The status of the task.
        /// </summary>
        [JsonRequired]
        [JsonPropertyOrder(3)]
        public TaskStatuses Status { get; init; }

        /// <summary>
        /// The deadline by which the task should be completed.
        /// </summary>
        [JsonRequired]
        [JsonPropertyOrder(4)]
        public DateTime ExpirationDate { get; init; }

        /// <summary>
        /// The identification details of the task.
        /// </summary>
        [JsonRequired]
        [JsonPropertyOrder(5)]
        public Identification Identification { get; init; }
    }
}