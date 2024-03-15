// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Behaviors.Mapping.Enums
{
    /// <summary>
    /// The status of the core business logic processing.
    /// </summary>
    public enum ProcessingResult
    {
        /// <summary>
        /// The <see cref="NotificationEvent"/> was processed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The <see cref="NotificationEvent"/> was not processed due to expected business cases (to not retry sending)
        /// </summary>
        Skipped = 1,

        /// <summary>
        /// The <see cref="NotificationEvent"/> was not processed due to internal errors or missing data (retry is required).
        /// </summary>
        Failure = 2
    }
}