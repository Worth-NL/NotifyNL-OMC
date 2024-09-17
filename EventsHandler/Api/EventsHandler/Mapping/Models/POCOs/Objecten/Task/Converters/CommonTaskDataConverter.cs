// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Services.Settings.Extensions;
using ConfigurationExtensions = EventsHandler.Services.Settings.Extensions.ConfigurationExtensions;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.Converters
{
    /// <summary>
    /// Converts task data from different versions of "Objecten" into a unified <see cref="CommonTaskData"/>.
    /// </summary>
    internal static class CommonTaskDataConverter
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
        internal static CommonTaskData ConvertToUnified(this vNijmegen.TaskObject taskNijmegen)
        {
            return new CommonTaskData
            {
                CaseUri        = RecreateCaseUri(taskNijmegen.Coupling.Id),  // NOTE: GUID is given, URI needs to be recreated
                CaseId         = taskNijmegen.Coupling.Id,
                Title          = taskNijmegen.Title,
                Status         = taskNijmegen.Status,
                ExpirationDate = taskNijmegen.ExpirationDate,
                Identification = taskNijmegen.Identification
            };
        }

        #region Helper methods
        private const string CaseUri = "https://wwww.{0}/zaken/api/v1/zaken/{1}";

        private static Uri RecreateCaseUri(Guid caseId)
            => string.Format(CaseUri, ConfigurationExtensions.OpenZaakDomain(), caseId)
                     .GetValidUri();
        #endregion
    }
}