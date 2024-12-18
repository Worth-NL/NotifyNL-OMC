// © 2024, Worth Systems.

using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Interfaces;

namespace WebQueries.DataQuerying.Strategies.Queries
{
    /// <summary>
    /// The interface defining that the service will have domain component of the URI.
    /// </summary>
    public interface IDomain
    {
        /// <summary>
        /// Gets the organization-specific (e.g., municipality) domain part of the specific Web API service URI:
        /// <code>
        ///   http(s)://[DOMAIN]/ApiEndpoint
        /// </code>
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        internal string GetDomain();

        /// <summary>
        /// Gets the health check.
        /// </summary>
        /// <param name="networkService"><inheritdoc cref="IHttpNetworkService" path="/summary"/></param>
        /// <returns>
        ///   The status of the service.
        /// </returns>
        internal Task<HttpRequestResponse> GetHealthCheckAsync(IHttpNetworkService networkService);
    }
}