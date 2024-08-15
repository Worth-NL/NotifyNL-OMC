// © 2024, Worth Systems.

namespace EventsHandler.Services.DataSending.Responses
{
    /// <summary>
    /// Contains details of notification response from "Notify NL" Web API service.
    /// </summary>
    internal readonly struct NotifySendResponse  // NOTE: "NotificationResponse" is restricted name of the model from "Notify.Models.Responses"
    {
        /// <summary>
        /// Gets the status of the <see cref="NotifySendResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }

        /// <summary>
        /// Gets the content of the <see cref="NotifySendResponse"/>.
        /// </summary>
        internal string Content { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifySendResponse"/> struct.
        /// </summary>
        internal NotifySendResponse(bool isSuccess, string? content)
        {
            this.IsSuccess = isSuccess;
            this.Content = content ?? string.Empty;
        }
    }
}