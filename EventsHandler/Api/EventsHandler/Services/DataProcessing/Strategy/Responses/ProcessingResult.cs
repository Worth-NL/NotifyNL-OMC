// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;

namespace EventsHandler.Services.DataProcessing.Strategy.Responses
{
    /// <summary>
    /// Contains the result of processing the given notification JSON.
    /// </summary>
    public readonly struct ProcessingResult  // NOTE: Has to be public to be used in Dependency Injection
    {
        /// <summary>
        /// The details of processing result.
        /// </summary>
        internal ProcessingStatus Status { get; }

        /// <summary>
        /// The description of the processing result.
        /// </summary>
        internal string Description { get; }

        /// <summary>
        /// The details of the processing result.
        /// </summary>
        internal BaseEnhancedDetails Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingResult"/> struct.
        /// </summary>
        public ProcessingResult(ProcessingStatus status, string description, object json, BaseEnhancedDetails details)
        {
            this.Status = status;
            this.Description = string.Format(Resources.Processing_STATUS_Notification, description, json);
            this.Details = details;
        }
    }
}