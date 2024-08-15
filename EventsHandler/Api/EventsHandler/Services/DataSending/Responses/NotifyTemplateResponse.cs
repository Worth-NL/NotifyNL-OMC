// © 2024, Worth Systems.

namespace EventsHandler.Services.DataSending.Responses
{
    /// <summary>
    /// Contains details of template response from "Notify NL" Web API service.
    /// </summary>
    internal readonly struct NotifyTemplateResponse  // NOTE: "TemplateResponse" is restricted name of the model from "Notify.Models.Responses"
    {
        /// <summary>
        /// Gets the status of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }

        /// <summary>
        /// Gets the subject of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        internal string Subject { get; }

        /// <summary>
        /// Gets the body of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        internal string Body { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyTemplateResponse"/> struct.
        /// </summary>
        internal NotifyTemplateResponse(bool isSuccess, string? subject, string? body)
        {
            this.IsSuccess = isSuccess;
            this.Subject = subject ?? string.Empty;
            this.Body = body ?? string.Empty;
        }
    }
}