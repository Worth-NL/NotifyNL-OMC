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
        /// The <see cref="NotificationEvent"/> was not processed (e.g., as part of requirements). DO NOT retry sending.
        /// </summary>
        Skipped = 1,

        /// <summary>
        /// The <see cref="NotificationEvent"/> was not processed (e.g., due to internal errors). Retry is required.
        /// </summary>
        Failure = 2,

        /// <summary>
        /// The <see cref="NotificationEvent"/> was not processed (e.g., due to missing data). DO NOT retry sending.
        /// </summary>
        Aborted = 3
    }
}