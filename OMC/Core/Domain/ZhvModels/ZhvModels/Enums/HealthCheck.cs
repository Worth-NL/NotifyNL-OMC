// © 2023, Worth Systems.

using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace ZhvModels.Enums
{
    /// <summary>
    /// The validity of the <see cref="NotificationEvent"/>.
    /// </summary>
    public enum HealthCheck
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The <see cref="NotificationEvent"/> is valid and consistent. Can be processed.
        /// </summary>
        OK_Valid = 0,

        /// <summary>
        /// The <see cref="NotificationEvent"/> is valid but has some missing optional or dynamic properties. Can be processed.
        /// </summary>
        OK_Inconsistent = 1,

        /// <summary>
        /// The <see cref="NotificationEvent"/> is invalid. Required properties are missing. Cannot be processed.
        /// </summary>
        ERROR_Invalid = 2
    }
}