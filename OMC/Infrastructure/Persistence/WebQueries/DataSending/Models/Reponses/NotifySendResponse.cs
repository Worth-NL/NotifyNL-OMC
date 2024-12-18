// © 2024, Worth Systems.

using WebQueries.Properties;

namespace WebQueries.DataSending.Models.Reponses
{
    /// <summary>
    /// Contains details of notification response from "Notify NL" Web API service.
    /// </summary>
    public readonly struct NotifySendResponse  // NOTE: "NotificationResponse" is restricted name of the model from "Notify.Models.Responses"
    {
        /// <summary>
        /// The affirmative status of the <see cref="NotifySendResponse"/>.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the <see cref="NotifySendResponse"/>.
        /// </summary>
        public bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The error which occurred during the communication with "Notify NL".
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifySendResponse"/> struct.
        /// </summary>
        private NotifySendResponse(bool isSuccess, string error)
        {
            this.IsSuccess = isSuccess;
            this.Error = error;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        public static NotifySendResponse Success()
            => new(true, string.Empty);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static NotifySendResponse Failure_Unknown()
            => new(false, QueryResources.Response_ProcessingData_ERROR_DeliveryMethodUnknown);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static NotifySendResponse Failure(string error)
            => new(false, error);
    }
}