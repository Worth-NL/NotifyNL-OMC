// © 2024, Worth Systems.

using EventsHandler.Properties;

namespace EventsHandler.Services.DataSending.Responses
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
        internal bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The body of the <see cref="NotifySendResponse"/>.
        /// </summary>
        internal string Body { get; }

        /// <summary>
        /// The error which occurred during the communication with "Notify NL".
        /// </summary>
        internal string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifySendResponse"/> struct.
        /// </summary>
        private NotifySendResponse(bool isSuccess, string body, string error)
        {
            this.IsSuccess = isSuccess;
            this.Body = body;
            this.Error = error;
        }
        
        /// <summary>
        /// Success result.
        /// </summary>
        internal static NotifySendResponse Success(string body)
            => new(true, body, string.Empty);
        
        /// <summary>
        /// Failure result.
        /// </summary>
        internal static NotifySendResponse Failure(string? error = null)
            => new(false, string.Empty, error ?? Resources.Processing_ERROR_NotifyResponseNull);
    }
}