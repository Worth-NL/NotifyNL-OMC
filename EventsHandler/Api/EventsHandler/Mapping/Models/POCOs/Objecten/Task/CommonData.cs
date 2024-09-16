// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task
{
    /// <summary>
    /// The data related to the Task (from vHague or vNijmegen versions) retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Common DTO for all versions of "Objecten" Web API service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    internal readonly struct CommonData : IJsonSerializable
    {
        /// <summary>
        /// The reference to <see cref="Case"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        internal Uri CaseUri { get; init; }

        /// <summary>
        /// The title of the task.
        /// </summary>
        internal string Title { get; init; }

        /// <summary>
        /// The status of the task.
        /// </summary>
        internal TaskStatuses Status { get; init; }

        /// <summary>
        /// The deadline by which the task should be completed.
        /// </summary>
        internal DateTime ExpirationDate { get; init; }

        /// <summary>
        /// The identification details of the task.
        /// </summary>
        internal Identification Identification { get; init; }
    }
}