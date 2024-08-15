// © 2024, Worth Systems.

namespace EventsHandler.Services.DataSending.Responses
{
    /// <summary>
    /// Contains details of response from "Notify NL" Web API service.
    /// </summary>
    internal readonly struct NotifyResponse
    {
        /// <summary>
        /// Gets the status of the <see cref="NotifyResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }

        /// <summary>
        /// Gets the content of the <see cref="NotifyResponse"/>.
        /// </summary>
        internal string Content { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyResponse"/> struct.
        /// </summary>
        internal NotifyResponse(bool isSuccess, string? content)
        {
            this.IsSuccess = isSuccess;
            this.Content = content ?? string.Empty;
        }
    }
}