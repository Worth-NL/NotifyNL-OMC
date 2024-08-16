// © 2024, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Interfaces;

namespace EventsHandler.Services.DataProcessing.Strategy.Responses
{
    /// <summary>
    /// Contains details of processing response performed by a specific OMC <see cref="INotifyScenario"/>.
    /// </summary>
    internal readonly struct ProcessingResponse
    {
        /// <summary>
        /// The affirmative status of the <see cref="ProcessingResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }
        
        /// <summary>
        /// The negated status of the <see cref="ProcessingResponse"/>.
        /// </summary>
        internal bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingResponse"/> struct.
        /// </summary>
        private ProcessingResponse(bool isSuccess)
        {
            this.IsSuccess = isSuccess;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static ProcessingResponse Success() => new(true);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static ProcessingResponse Failure() => new(false);
    }
}