// © 2024, Worth Systems.

using System.Net;

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
        /// The HTTP status code the Web API service replied with.
        /// </summary>
        /// <remarks>
        ///   <see langword="null"/> when the call never produced a response at all - a timeout, a DNS or
        ///   TLS failure, or a cancellation - so a missing value means "no answer", not "answer without a
        ///   status".
        ///   <para>
        ///     Callers need this to tell a resource that genuinely is not there from a service that could
        ///     not be reached: the first must not be retried, the second must be.
        ///   </para>
        /// </remarks>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestResponse"/> struct.
        /// </summary>
        private HttpRequestResponse(bool isSuccess, string jsonResponse, HttpStatusCode? statusCode)
        {
            this.IsSuccess = isSuccess;
            this.JsonResponse = jsonResponse;
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        public static HttpRequestResponse Success(string jsonResponse, HttpStatusCode? statusCode = null)
            => new(true, jsonResponse, statusCode);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static HttpRequestResponse Failure(string jsonResponse, HttpStatusCode? statusCode = null)
            => new(false, jsonResponse, statusCode);
    }
}