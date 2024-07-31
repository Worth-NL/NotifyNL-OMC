// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Mapping.Enums
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
        /// The processing of <see cref="NotificationEvent"/> was skipped (intentionally). DO NOT retry sending.
        /// </summary>
        Skipped = 1,

        /// <summary>
        /// The processing of <see cref="NotificationEvent"/> was aborted (intentionally). DO NOT retry sending.
        /// </summary>
        Aborted = 2,

        /// <summary>
        /// The processing of <see cref="NotificationEvent"/> wasn't possible (e.g., due to missing data). DO NOT retry sending.
        /// </summary>
        NotPossible = 3,

        /// <summary>
        /// The <see cref="NotificationEvent"/> was not processed (e.g., due to internal errors). Retry is required.
        /// </summary>
        Failure = 4
    }
}