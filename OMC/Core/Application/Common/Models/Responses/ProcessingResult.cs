// © 2024, Worth Systems.

using Common.Enums.Responses;
using Common.Extensions;
using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using Common.Properties;

namespace Common.Models.Responses
{
    /// <summary>
    /// Contains the result of processing the given notification JSON.
    /// </summary>
    public readonly struct ProcessingResult
    {
        /// <summary>
        /// The details of processing result.
        /// </summary>
        public ProcessingStatus Status { get; }

        /// <summary>
        /// The description of the processing result.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The details of the processing result.
        /// </summary>
        public BaseEnhancedDetails Details { get; }

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
        public static ProcessingResult Success(string description, object? json = null, BaseEnhancedDetails? details = null)
            => new(ProcessingStatus.Success, GetDescription(description, json), details ?? InfoDetails.Empty);

        /// <summary>
        /// Skipped result.
        /// </summary>
        public static ProcessingResult Skipped(string description, object? json = null, BaseEnhancedDetails? details = null)
            => new(ProcessingStatus.Skipped, GetDescription(description, json), details ?? InfoDetails.Empty);

        /// <summary>
        /// Aborted result.
        /// </summary>
        public static ProcessingResult Aborted(string description, object? json = null, BaseEnhancedDetails? details = null)
            => new(ProcessingStatus.Aborted, GetDescription(description, json), details ?? ErrorDetails.Empty);

        /// <summary>
        /// NotPossible result.
        /// </summary>
        public static ProcessingResult NotPossible(string description, object? json = null, BaseEnhancedDetails? details = null)
            => new(ProcessingStatus.NotPossible, GetDescription(description, json), details ?? ErrorDetails.Empty);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static ProcessingResult Failure(string description, object? json = null, BaseEnhancedDetails? details = null)
            => new(ProcessingStatus.Failure, GetDescription(description, json), details ?? ErrorDetails.Empty);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static ProcessingResult Unknown(string description, object? json = null, BaseSimpleDetails? details = null)
            => new(ProcessingStatus.Failure, GetDescription(description, json), (details ?? UnknownDetails.Empty).Expand());
        #endregion

        #region Helper methods
        private static string GetDescription(string description, object? json = null)
        {
            return json == null
                ? description
                : string.Format(CommonResources.Response_Processing_STATUS_NotificationOperation, description, json);
        }
        #endregion
    }
}