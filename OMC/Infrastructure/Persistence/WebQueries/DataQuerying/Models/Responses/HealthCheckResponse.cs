// © 2024, Worth Systems.

using WebQueries.Properties;

namespace WebQueries.DataQuerying.Models.Responses
{
    /// <summary>
    /// The summary of the Web API service health.
    /// </summary>
    public struct HealthCheckResponse
    {
        /// <summary>
        /// Gets the state of the Web API service health.
        /// </summary>
        public static string Get(bool isSuccess) => isSuccess
            ? QueryResources.Response_HealthCheck_SUCCESS_State
            : QueryResources.Response_HealthCheck_ERROR_State;
    }
}