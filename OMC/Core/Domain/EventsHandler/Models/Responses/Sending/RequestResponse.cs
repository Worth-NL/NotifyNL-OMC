// © 2024, Worth Systems.

namespace EventsHandler.Models.Responses.Sending
{
    /// <summary>
    /// Contains details of HTTP Response from a Web / Web API service after sending to it
    /// <see cref="HttpMethod.Get"/>, <see cref="HttpMethod.Post"/> or similar HTTP Request.
    /// </summary>
    internal readonly struct RequestResponse
    {
        /// <summary>
        /// The affirmative status of the HTTP Request.
        /// </summary>
        internal bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the HTTP Request.
        /// </summary>
        internal bool IsFailure => !IsSuccess;

        /// <summary>
        /// The JSON response from the Web API service.
        /// </summary>
        internal string JsonResponse { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponse"/> struct.
        /// </summary>
        private RequestResponse(bool isSuccess, string jsonResponse)
        {
            IsSuccess = isSuccess;
            JsonResponse = jsonResponse;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        internal static RequestResponse Success(string jsonResponse)
            => new(true, jsonResponse);

        /// <summary>
        /// Failure result.
        /// </summary>
        internal static RequestResponse Failure(string jsonResponse)
            => new(false, jsonResponse);
    }
}