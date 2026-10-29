// © 2026, Worth Systems.

namespace WebQueries.DataSending.Models.Reponses
{
    /// <summary>
    /// Whether "Notify NL" already holds a notification with a given reference.
    /// </summary>
    public readonly struct NotifyLookupResponse
    {
        /// <summary>
        /// Whether the lookup itself succeeded. When it did not, nothing is known about earlier sends.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the <see cref="NotifyLookupResponse"/>.
        /// </summary>
        public bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// Whether a notification with the reference exists.
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// The id of the existing notification, or empty when there is none.
        /// </summary>
        public string NotificationId { get; }

        /// <summary>
        /// The status of the existing notification, or empty when there is none.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// The error which occurred during the communication with "Notify NL".
        /// </summary>
        public string Error { get; }

        private NotifyLookupResponse(bool isSuccess, bool exists, string notificationId, string status, string error)
        {
            this.IsSuccess = isSuccess;
            this.Exists = exists;
            this.NotificationId = notificationId;
            this.Status = status;
            this.Error = error;
        }

        /// <summary>
        /// A notification with the reference exists.
        /// </summary>
        public static NotifyLookupResponse Found(string notificationId, string status)
            => new(true, true, notificationId, status, string.Empty);

        /// <summary>
        /// No notification with the reference exists.
        /// </summary>
        public static NotifyLookupResponse NotFound()
            => new(true, false, string.Empty, string.Empty, string.Empty);

        /// <summary>
        /// The lookup failed.
        /// </summary>
        public static NotifyLookupResponse Failure(string error)
            => new(false, false, string.Empty, string.Empty, error);
    }
}
