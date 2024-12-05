// © 2024, Worth Systems.

using EventsHandler.Properties;

namespace EventsHandler.Models.Responses.Sending
{
    /// <summary>
    /// Contains details of notification response from "Notify NL" Web API service.
    /// </summary>
    internal readonly struct NotifySendResponse  // NOTE: "NotificationResponse" is restricted name of the model from "Notify.Models.Responses"
    {
        /// <summary>
        /// The affirmative status of the <see cref="NotifySendResponse"/>.
        /// </summary>
        private bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the <see cref="NotifySendResponse"/>.
        /// </summary>
        internal bool IsFailure => !IsSuccess;

        /// <summary>
        /// The error which occurred during the communication with "Notify NL".
        /// </summary>
        internal string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifySendResponse"/> struct.
        /// </summary>
        private NotifySendResponse(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static NotifySendResponse Success()
            => new(true, string.Empty);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static NotifySendResponse Failure_Unknown()
            => new(false, ApiResources.Processing_ERROR_Notification_DeliveryMethodUnknown);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static NotifySendResponse Failure(string error)
            => new(false, error);
    }
}