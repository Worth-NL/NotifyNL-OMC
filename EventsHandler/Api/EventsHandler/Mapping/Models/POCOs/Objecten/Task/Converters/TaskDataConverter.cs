// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.Converters
{
    /// <summary>
    /// Converts task data from different versions of "Objecten" into a unified <see cref="CommonTaskData"/>.
    /// </summary>
    internal static class TaskDataConverter
    {
        /// <summary>
        /// Converts <see cref="Data"/> from <see cref="Record"/> from <see cref="TaskObject"/> from "Objecten" Web API service (version used by The Hague).
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonTaskData"/> DTO model.
        /// </returns>
        internal static CommonTaskData ConvertToUnified(this Data taskDataHague)
        {
            return new CommonTaskData
            {
                CaseUri = taskDataHague.CaseUri,
                Title = taskDataHague.Title,
                Status = taskDataHague.Status,
                ExpirationDate = taskDataHague.ExpirationDate,
                Identification = taskDataHague.Identification
            };
        }
    }
}