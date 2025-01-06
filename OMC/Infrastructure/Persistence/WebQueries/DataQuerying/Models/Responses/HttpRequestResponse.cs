// © 2024, Worth Systems.

namespace WebQueries.DataQuerying.Models.Responses
{
    /// <summary>
    /// Contains details of HTTP Response from a Web API service after sending to it
    /// <see cref="HttpMethod.Get"/>, <see cref="HttpMethod.Post"/> or similar HTTP Request.
    /// </summary>
    public readonly struct HttpRequestResponse
    {
        /// <summary>
        /// The affirmative status of the HTTP Request.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The negated status of the HTTP Request.
        /// </summary>
        public bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// The JSON response from the Web API service.
        /// </summary>
        public string JsonResponse { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestResponse"/> struct.
        /// </summary>
        private HttpRequestResponse(bool isSuccess, string jsonResponse)
        {
            this.IsSuccess = isSuccess;
            this.JsonResponse = jsonResponse;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        public static HttpRequestResponse Success(string jsonResponse)
            => new(true, jsonResponse);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static HttpRequestResponse Failure(string jsonResponse)
            => new(false, jsonResponse);
    }
}