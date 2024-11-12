// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Messages.Models.Details;
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
        private ProcessingResult(ProcessingStatus status, string description, BaseEnhancedDetails details)
        {
            this.Status = status;
            this.Description = description;
            this.Details = details;
        }

        #region Responses
        /// <summary>
        /// Success result.
        /// </summary>
        internal static ProcessingResult Success(string description, object? json = null, BaseEnhancedDetails? details = null)
        {
            return new ProcessingResult(ProcessingStatus.Success, GetDescription(description, json), details ?? InfoDetails.Empty);
        }
        
        /// <summary>
        /// Skipped result.
        /// </summary>
        internal static ProcessingResult Skipped(string description, object? json = null, BaseEnhancedDetails? details = null)
        {
            return new ProcessingResult(ProcessingStatus.Skipped, GetDescription(description, json), details ?? InfoDetails.Empty);
        }
        
        /// <summary>
        /// Aborted result.
        /// </summary>
        internal static ProcessingResult Aborted(string description, object? json = null, BaseEnhancedDetails? details = null)
        {
            return new ProcessingResult(ProcessingStatus.Aborted, GetDescription(description, json), details ?? ErrorDetails.Empty);
        }
        
        /// <summary>
        /// NotPossible result.
        /// </summary>
        internal static ProcessingResult NotPossible(string description, object? json = null, BaseEnhancedDetails? details = null)
        {
            return new ProcessingResult(ProcessingStatus.NotPossible, GetDescription(description, json), details ?? ErrorDetails.Empty);
        }
        
        /// <summary>
        /// Failure result.
        /// </summary>
        internal static ProcessingResult Failure(string description, object? json = null, BaseEnhancedDetails? details = null)
        {
            return new ProcessingResult(ProcessingStatus.Failure, GetDescription(description, json), details ?? ErrorDetails.Empty);
        }
        
        /// <summary>
        /// Failure result.
        /// </summary>
        internal static ProcessingResult Unknown(string description, object? json = null, BaseSimpleDetails? details = null)
        {
            return new ProcessingResult(ProcessingStatus.Failure, GetDescription(description, json), (details ?? UnknownDetails.Empty).Expand());
        }
        #endregion

        #region Helper methods
        private static string GetDescription(string description, object? json = null)
        {
            return json == null
                ? description
                : string.Format(Resources.Processing_STATUS_Notification, description, json);
        }
        #endregion
    }
}