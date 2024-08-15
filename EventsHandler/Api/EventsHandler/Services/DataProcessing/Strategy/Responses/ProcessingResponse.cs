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
        /// The status of the <see cref="ProcessingResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingResponse"/> struct.
        /// </summary>
        public ProcessingResponse(bool isSuccess)
        {
            this.IsSuccess = isSuccess;
        }
    }
}