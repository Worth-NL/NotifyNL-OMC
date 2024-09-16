// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task
{
    /// <summary>
    /// The data related to the Task (from vHague or vNijmegen versions) retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Common DTO for all versions of "Objecten" Web API service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    internal readonly struct CommonTaskData : IJsonSerializable
    {
        /// <summary>
        /// The reference to related object in <see cref="System.Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(0)]
        internal Uri? Uri { get; init; }  // NOTE: Some task data might not have it

        /// <summary>
        /// The ID of the related object.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        internal Guid Id { get; init; }

        /// <summary>
        /// The title of the task.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(2)]
        internal string Title { get; init; }

        /// <summary>
        /// The status of the task.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(3)]
        internal TaskStatuses Status { get; init; }

        /// <summary>
        /// The deadline by which the task should be completed.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(4)]
        internal DateTime ExpirationDate { get; init; }

        /// <summary>
        /// The identification details of the task.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(5)]
        internal Identification Identification { get; init; }
    }
}