// © 2026, Worth Systems.

using Common.Settings.Configuration;
using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenProducten;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenProducten.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "Open Product" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    public interface IQueryProducten : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="OmcConfiguration"/>
        protected internal OmcConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenProducten";

        #region Abstract (Product data)
        /// <summary>
        /// Gets the details of a specific product from "Open Product" Web API service.
        /// </summary>
        /// <remarks>
        ///   The response embeds the whole product type, so no follow-up request is needed to resolve it.
        /// </remarks>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="productUri">The <see cref="Uri"/> of the product to be retrieved.</param>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<Product> GetProductAsync(IQueryBase queryBase, Uri productUri);
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.ZGW.Endpoint.OpenProducten();
        #endregion

        #region Polymorphic (Health Check)
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        async Task<HttpRequestResponse> IDomain.GetHealthCheckAsync(IHttpNetworkService networkService)
        {
            // NOTE: There is no dedicated health check endpoint, listing the only collection should be fine
            Uri healthCheckEndpointUri = new($"{GetDomain()}/producten");

            return await networkService.GetAsync(HttpClientTypes.OpenProducten, healthCheckEndpointUri);
        }
        #endregion
    }
}
