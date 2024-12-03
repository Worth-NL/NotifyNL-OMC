// © 2024, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;

namespace EventsHandler.Services.DataProcessing.Strategy.Responses
{
    /// <summary>
    /// Contains details of processing operation performed by a specific OMC <see cref="INotifyScenario"/>.
    /// </summary>
    internal readonly struct ProcessingDataResponse
    {
        /// <summary>
        /// The affirmative status of the <see cref="ProcessingDataResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }
        
        /// <summary>
        /// The negated status of the <see cref="ProcessingDataResponse"/>.
        /// </summary>
        internal bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The message (e.g., error or confirmation) captured by the <see cref="ProcessingDataResponse"/>.
        /// </summary>
        internal string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingDataResponse"/> struct.
        /// </summary>
        private ProcessingDataResponse(bool isSuccess, string message)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static ProcessingDataResponse Success()
            => new(true, ApiResources.Processing_SUCCESS_Scenario_DataProcessed);
        
        /// <summary>
        /// Specific failure result.
        /// </summary>
        internal static ProcessingDataResponse Failure_Empty()
            => new(false, ApiResources.Processing_ERROR_Scenario_MissingNotifyData);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static ProcessingDataResponse Failure(string error)
            => new(false, error);
    }
}