// © 2024, Worth Systems.

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Task.Converters
{
    /// <summary>
    /// Converts task data from different versions of "Objecten" into a unified <see cref="CommonTaskData"/>.
    /// </summary>
    public static class CommonTaskDataConverter
    {
        /// <summary>
        /// Converts <see cref="vHague.TaskObject"/> (version used by The Hague) from "Objecten" Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonTaskData"/> DTO model.
        /// </returns>
        public static CommonTaskData ConvertToUnified(this vHague.TaskObject taskDataHague)
        {
            return new CommonTaskData
            {
                CaseUri        = taskDataHague.Record.Data.CaseUri,
                CaseId         = taskDataHague.Record.Data.CaseUri.GetGuid(),  // NOTE: URI is given, GUID needs to be extracted
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
        public static CommonTaskData ConvertToUnified(this vNijmegen.TaskObject taskNijmegen)
        {
            return new CommonTaskData
            {
                CaseUri        = taskNijmegen.Record.Data.Coupling.Id.RecreateCaseUri(),  // NOTE: GUID is given, URI needs to be recreated
                CaseId         = taskNijmegen.Record.Data.Coupling.Id,
                Title          = taskNijmegen.Record.Data.Title,
                Status         = taskNijmegen.Record.Data.Status,
                ExpirationDate = taskNijmegen.Record.Data.ExpirationDate,
                Identification = taskNijmegen.Record.Data.Identification
            };
        }
    }
}