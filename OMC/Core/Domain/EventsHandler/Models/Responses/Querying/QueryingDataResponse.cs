// © 2024, Worth Systems.

using EventsHandler.Models.DTOs.Processing;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;

namespace EventsHandler.Models.Responses.Querying
{
    /// <summary>
    /// Contains details of getting data operation performed by a specific OMC <see cref="INotifyScenario"/>.
    /// </summary>
    internal readonly struct QueryingDataResponse
    {
        /// <summary>
        /// The affirmative status of the <see cref="QueryingDataResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the <see cref="QueryingDataResponse"/>.
        /// </summary>
        internal bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The details about result of <see cref="QueryingDataResponse"/>.
        /// </summary>
        internal string Message { get; }

        /// <summary>
        /// The result of getting data operation.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        internal IReadOnlyCollection<NotifyData> Content { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryingDataResponse"/> struct.
        /// </summary>
        private QueryingDataResponse(bool isSuccess, string message, IReadOnlyCollection<NotifyData> content)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
            this.Content = content;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static QueryingDataResponse Success(IReadOnlyCollection<NotifyData> content)
            => new(true, ApiResources.Processing_SUCCESS_Scenario_DataRetrieved, content);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static QueryingDataResponse Failure()
            => new(false, ApiResources.Processing_ERROR_Scenario_NotificationMethod, []);
    }
}