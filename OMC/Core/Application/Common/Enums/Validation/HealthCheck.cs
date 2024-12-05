// © 2023, Worth Systems.

namespace Common.Enums.Validation
{
    /// <summary>
    /// The validity of the <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/>.
    /// </summary>
    public enum HealthCheck
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/> is valid and consistent. Can be processed.
        /// </summary>
        OK_Valid = 0,

        /// <summary>
        /// The <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/> is valid but has some missing optional or dynamic properties. Can be processed.
        /// </summary>
        OK_Inconsistent = 1,

        /// <summary>
        /// The <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/> is invalid. Required properties are missing. Cannot be processed.
        /// </summary>
        ERROR_Invalid = 2
    }
}