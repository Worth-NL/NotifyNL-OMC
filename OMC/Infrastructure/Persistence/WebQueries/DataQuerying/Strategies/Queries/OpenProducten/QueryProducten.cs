// © 2026, Worth Systems.

using Common.Settings.Configuration;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenProducten.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenProducten;
using ZgwModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenProducten
{
    /// <inheritdoc cref="IQueryProducten"/>
    /// <remarks>
    ///   Version: "Open Product" (Producten API 1.7.0) Web API service.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryProducten : IQueryProducten
    {
        /// <inheritdoc cref="IQueryProducten.Configuration"/>
        OmcConfiguration IQueryProducten.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.7.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProducten"/> class.
        /// </summary>
        public QueryProducten(OmcConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryProducten)this).Configuration = configuration;
        }

        #region Abstract (Product data)
        /// <inheritdoc cref="IQueryProducten.GetProductAsync(IQueryBase, Uri)"/>
        async Task<Product> IQueryProducten.GetProductAsync(IQueryBase queryBase, Uri productUri)
        {
            return await queryBase.ProcessGetAsync<Product>(
                httpClientType: HttpClientTypes.OpenProducten,
                uri: productUri,  // Request URL
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoProduct);
        }
        #endregion
    }
}
