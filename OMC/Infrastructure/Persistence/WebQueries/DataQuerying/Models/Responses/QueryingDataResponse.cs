// © 2024, Worth Systems.

using WebQueries.DataSending.Models.DTOs;
using WebQueries.Properties;

namespace WebQueries.DataQuerying.Models.Responses
{
    /// <summary>
    /// Contains details of response from Web API querying services.
    /// </summary>
    public readonly struct QueryingDataResponse
    {
        /// <summary>
        /// The affirmative status of the <see cref="QueryingDataResponse"/>.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the <see cref="QueryingDataResponse"/>.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// The details about result of <see cref="QueryingDataResponse"/>.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The result of getting data operation.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public IReadOnlyCollection<NotifyData> Content { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryingDataResponse"/> struct.
        /// </summary>
        private QueryingDataResponse(bool isSuccess, string message, IReadOnlyCollection<NotifyData> content)
        {
            IsSuccess = isSuccess;
            Message = message;
            Content = content;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        public static QueryingDataResponse Success(IReadOnlyCollection<NotifyData> content)
            => new(true, QueryResources.Response_QueryingData_SUCCESS_DataRetrieved, content);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static QueryingDataResponse Failure()
            => new(false, QueryResources.Response_QueryingData_ERROR_NotificationMethodMissing, []);
    }
}