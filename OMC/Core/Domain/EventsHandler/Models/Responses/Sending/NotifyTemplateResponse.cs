// © 2024, Worth Systems.

namespace EventsHandler.Models.Responses.Sending
{
    /// <summary>
    /// Contains details of template response from "Notify NL" Web API service.
    /// </summary>
    internal readonly struct NotifyTemplateResponse  // NOTE: "TemplateResponse" is restricted name of the model from "Notify.Models.Responses"
    {
        /// <summary>
        /// The affirmative status of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        private bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        internal bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The subject of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        internal string Subject { get; }

        /// <summary>
        /// The body of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        internal string Body { get; }

        /// <summary>
        /// The error which occurred during the communication with "Notify NL".
        /// </summary>
        internal string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyTemplateResponse"/> struct.
        /// </summary>
        private NotifyTemplateResponse(bool isSuccess, string subject, string body, string exception)
        {
            this.IsSuccess = isSuccess;
            this.Subject = subject;
            this.Body = body;
            this.Error = exception;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static NotifyTemplateResponse Success(string subject, string body)
            => new(true, subject, body, string.Empty);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static NotifyTemplateResponse Failure(string error)
            => new(false, string.Empty, string.Empty, error);
    }
}