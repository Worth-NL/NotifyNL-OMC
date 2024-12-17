// © 2024, Worth Systems.

namespace WebQueries.DataSending.Models.Reponses
{
    /// <summary>
    /// Contains details of template response from "Notify NL" Web API service.
    /// </summary>
    public readonly struct NotifyTemplateResponse  // NOTE: "TemplateResponse" is restricted name of the model from "Notify.Models.Responses"
    {
        /// <summary>
        /// The affirmative status of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        public bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The subject of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// The body of the <see cref="NotifyTemplateResponse"/>.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// The error which occurred during the communication with "Notify NL".
        /// </summary>
        public string Error { get; }

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
        public static NotifyTemplateResponse Success(string subject, string body)
            => new(true, subject, body, string.Empty);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static NotifyTemplateResponse Failure(string error)
            => new(false, string.Empty, string.Empty, error);
    }
}