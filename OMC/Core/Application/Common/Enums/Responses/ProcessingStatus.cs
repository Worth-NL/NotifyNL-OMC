// © 2023, Worth Systems.

namespace Common.Enums.Responses
{
    /// <summary>
    /// The status of the core business logic processing.
    /// </summary>
    public enum ProcessingStatus
    {
        /// <summary>
        /// The processing was successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The processing was skipped (intentionally). DO NOT retry sending.
        /// </summary>
        Skipped = 1,

        /// <summary>
        /// The processing was aborted (intentionally). DO NOT retry sending.
        /// </summary>
        Aborted = 2,

        /// <summary>
        /// The processing wasn't possible (e.g., due to missing data). DO NOT retry sending.
        /// </summary>
        NotPossible = 3,

        /// <summary>
        /// The processing was failed (e.g., due to public errors). Retry is required.
        /// </summary>
        Failure = 4
    }
}