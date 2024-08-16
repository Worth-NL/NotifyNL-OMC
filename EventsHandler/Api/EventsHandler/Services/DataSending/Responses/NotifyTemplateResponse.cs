// © 2024, Worth Systems.

namespace EventsHandler.Services.DataSending.Responses
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
        /// Initializes a new instance of the <see cref="NotifyTemplateResponse"/> struct.
        /// </summary>
        private NotifyTemplateResponse(bool isSuccess, string subject, string body)
        {
            this.IsSuccess = isSuccess;
            this.Subject = subject;
            this.Body = body;
        }
        
        /// <summary>
        /// Success result.
        /// </summary>
        internal static NotifyTemplateResponse Success(string subject, string body)
            => new(true, subject, body);
        
        /// <summary>
        /// Failure result.
        /// </summary>
        internal static NotifyTemplateResponse Failure()
            => new(false, string.Empty, string.Empty);
    }
}