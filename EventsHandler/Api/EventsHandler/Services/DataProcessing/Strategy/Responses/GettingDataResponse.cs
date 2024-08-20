// © 2024, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;

namespace EventsHandler.Services.DataProcessing.Strategy.Responses
{
    /// <summary>
    /// Contains details of getting data operation performed by a specific OMC <see cref="INotifyScenario"/>.
    /// </summary>
    internal readonly struct GettingDataResponse
    {
        /// <summary>
        /// The affirmative status of the <see cref="GettingDataResponse"/>.
        /// </summary>
        internal bool IsSuccess { get; }
        
        /// <summary>
        /// The negated status of the <see cref="GettingDataResponse"/>.
        /// </summary>
        internal bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The details about result of <see cref="GettingDataResponse"/>.
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
        /// Initializes a new instance of the <see cref="GettingDataResponse"/> struct.
        /// </summary>
        private GettingDataResponse(bool isSuccess, string message, IReadOnlyCollection<NotifyData> content)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
            this.Content = content;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static GettingDataResponse Success(string message, IReadOnlyCollection<NotifyData> content)
            => new(true, message, content);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static GettingDataResponse Failure(string message)
            => new(false, message, Array.Empty<NotifyData>());
    }
}