// © 2026, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.Exceptions;
using WebQueries.Producten.Interfaces;
using WebQueries.Tracing;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenProducten;

namespace WebQueries.Producten
{
    /// <inheritdoc cref="IProductScenario"/>
    public sealed class ProductScenarioImplementation : IProductScenario
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly OmcConfiguration _configuration;
        private readonly ILogger<ProductScenarioImplementation> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductScenarioImplementation"/> class.
        /// </summary>
        /// <param name="dataQuery">Resolves an <see cref="IQueryContext"/> for fetching product and party data.</param>
        /// <param name="configuration">The application configuration (whitelist, endpoints).</param>
        /// <param name="logger">The logger for this scenario.</param>
        public ProductScenarioImplementation(
            IDataQueryService<NotificationEvent> dataQuery,
            OmcConfiguration configuration,
            ILogger<ProductScenarioImplementation> logger)  // Dependency Injection (DI)
        {
            this._dataQuery = dataQuery;
            this._configuration = configuration;
            this._logger = logger;
        }

        /// <inheritdoc cref="IProductScenario.ProcessProductAsync(NotificationEvent)"/>
        async Task<HttpRequestResponse> IProductScenario.ProcessProductAsync(NotificationEvent notification)
        {
            IQueryContext queryContext = this._dataQuery.From(notification);

            Product product = await GetProductAsync(queryContext, notification);

            // TODO: The whitelist and gepubliceerd gates, the per-owner party lookup, and the
            //       fire-and-forget send with its contactmomenten land in the follow-up commits for
            //       Worth-NL/notifynl#113 - #116.
            throw new NotImplementedException();
        }

        #region Helper methods
        /// <summary>
        /// Reads the product the notification reported, and the product type embedded in it.
        /// </summary>
        /// <remarks>
        ///   A product that is not there is a decision, not a defect: "Open Product" answers 404 when it has
        ///   been deleted between publishing the notification and OMC getting to it, and redelivering that
        ///   forever would never succeed. Anything else - unreachable, unauthorised, a server error - stays
        ///   a failure, so the sender does retry it.
        /// </remarks>
        /// <exception cref="ProcessingAbortedException">The product no longer exists.</exception>
        /// <exception cref="HttpRequestException">"Open Product" could not be reached or refused the call.</exception>
        private async Task<Product> GetProductAsync(IQueryContext queryContext, NotificationEvent notification)
        {
            Guid productId = notification.ResourceUri.GetGuid();
            TraceContext.Emit("openproduct", "start", $"Attempting to retrieve product with id {productId}");

            Product product;
            try
            {
                product = await queryContext.GetProductAsync(notification.ResourceUri);
            }
            catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
            {
                string reason = $"Product with id {productId} no longer exists in Open Product.";

                TraceContext.Emit("openproduct", "abort", reason);
                this._logger.LogWarning("{Reason} Notification dropped without notifying anyone.", reason);

                throw new ProcessingAbortedException(reason, exception);
            }
            catch (Exception exception)
            {
                TraceContext.Emit("openproduct", "fail", exception.Message);
                throw;
            }

            TraceContext.Emit("openproduct", "ok",
                $"product with id {productId} retrieved, producttype {product.ProductType.Code}");

            return product;
        }
        #endregion
    }
}
