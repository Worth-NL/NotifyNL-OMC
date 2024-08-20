// © 2024, Worth Systems.

namespace EventsHandler.Services.DataSending.Responses
{
    /// <summary>
    /// Contains details of response from Web API service after sending to it GET, POST or similar request.
    /// </summary>
    internal readonly struct ApiResponse
    {
        /// <summary>
        /// The status of the HTTP Request.
        /// </summary>
        internal bool IsSuccess { get; }

        /// <summary>
        /// The JSON response from the Web API service.
        /// </summary>
        internal string JsonResponse { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse"/> struct.
        /// </summary>
        private ApiResponse(bool isSuccess, string jsonResponse)
        {
            this.IsSuccess = isSuccess;
            this.JsonResponse = jsonResponse;
        }
        
        /// <summary>
        /// Success result.
        /// </summary>
        internal static ApiResponse Success(string jsonResponse)
            => new(true, jsonResponse);
        
        /// <summary>
        /// Failure result.
        /// </summary>
        internal static ApiResponse Failure(string jsonResponse)
            => new(false, jsonResponse);
    }
}