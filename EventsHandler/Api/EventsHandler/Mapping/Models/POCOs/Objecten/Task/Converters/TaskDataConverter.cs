// © 2024, Worth Systems.

using EventsHandler.Extensions;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.Converters
{
    /// <summary>
    /// Converts task data from different versions of "Objecten" into a unified <see cref="CommonTaskData"/>.
    /// </summary>
    internal static class TaskDataConverter
    {
        /// <summary>
        /// Converts <see cref="vHague.TaskObject"/> (version used by The Hague) from "Objecten" Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonTaskData"/> DTO model.
        /// </returns>
        internal static CommonTaskData ConvertToUnified(this vHague.TaskObject taskDataHague)
        {
            return new CommonTaskData
            {
                Uri            = taskDataHague.Record.Data.CaseUri,
                Id             = taskDataHague.Record.Data.CaseUri.GetGuid(),
                Title          = taskDataHague.Record.Data.Title,
                Status         = taskDataHague.Record.Data.Status,
                ExpirationDate = taskDataHague.Record.Data.ExpirationDate,
                Identification = taskDataHague.Record.Data.Identification
            };
        }

        /// <summary>
        /// Converts <see cref="vNijmegen.TaskObject"/> (version used by Nijmegen) from "Objecten" Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonTaskData"/> DTO model.
        /// </returns>
        internal static CommonTaskData ConvertToUnified(this vNijmegen.TaskObject taskNijmegen)
        {
            return new CommonTaskData
            {
                Uri            = null,
                Id             = taskNijmegen.Coupling.Id,
                Title          = taskNijmegen.Title,
                Status         = taskNijmegen.Status,
                ExpirationDate = taskNijmegen.ExpirationDate,
                Identification = taskNijmegen.Identification
            };
        }
    }
}