// © 2024, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Interfaces;

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
        private bool IsSuccess { get; }
        
        /// <summary>
        /// The negated status of the <see cref="ProcessingDataResponse"/>.
        /// </summary>
        internal bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingDataResponse"/> struct.
        /// </summary>
        private ProcessingDataResponse(bool isSuccess)
        {
            this.IsSuccess = isSuccess;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static ProcessingDataResponse Success() => new(true);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static ProcessingDataResponse Failure() => new(false);
    }
}