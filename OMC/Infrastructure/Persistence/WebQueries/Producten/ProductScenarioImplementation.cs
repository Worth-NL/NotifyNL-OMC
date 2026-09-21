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

            // Step 1: Read the product the notification reported
            Product product = await GetProductAsync(queryContext, notification);

            // Step 2: Only configured product types may notify anyone
            ValidateProductTypeIsWhitelisted(product);

            // Step 3: An unpublished product is not shown to its owners, so it is not announced to them either
            ValidateProductIsPublished(product);

            // TODO: The per-owner party lookup and the fire-and-forget send with its contactmomenten land
            //       in the follow-up commits for Worth-NL/notifynl#114 - #116.
            throw new NotImplementedException();
        }

        #region Validation
        /// <summary>
        /// Rejects a product whose type is not on the whitelist.
        /// </summary>
        /// <remarks>
        ///   Matched on the product type's code rather than on an "identificatie": "Open Product" has no
        ///   such field, and the code is both unique per product type and readable in configuration.
        /// </remarks>
        /// <exception cref="ProcessingAbortedException">The product type is not whitelisted.</exception>
        private void ValidateProductTypeIsWhitelisted(Product product)
        {
            #pragma warning disable IDE0008  // Using "explicit types" wouldn't help with readability of the code
            var whitelistedIDs = this._configuration.ZGW.Whitelist.ProductCreate_IDs();
            #pragma warning restore IDE0008

            string productTypeCode = product.ProductType.Code;

            if (!whitelistedIDs.IsAllowed(productTypeCode))
            {
                // NOTE: IDs.ToString() resolves to the environment variable name, so whoever reads this
                //       knows which setting to change - the same contract BaseScenario.ValidateCaseId uses.
                string reason = $"Product type \"{productTypeCode}\" is not whitelisted in {whitelistedIDs}.";

                TraceContext.Emit("producttypewhitelist", "abort", reason);
                this._logger.LogInformation("{Reason} Product {ProductId} was not notified about.",
                    reason, product.Id);

                throw new ProcessingAbortedException(reason);
            }

            TraceContext.Emit("producttypewhitelist", "ok", $"product type \"{productTypeCode}\" is whitelisted");
        }

        /// <summary>
        /// Rejects a product that is not published.
        /// </summary>
        /// <remarks>
        ///   "gepubliceerd" is read from the product, not from its product type: a published type can still
        ///   hold products that are not meant to be shown yet.
        /// </remarks>
        /// <exception cref="ProcessingAbortedException">The product is not published.</exception>
        private void ValidateProductIsPublished(Product product)
        {
            if (!product.IsPublished)
            {
                const string reason = "Product is not published (gepubliceerd is false).";

                TraceContext.Emit("productgepubliceerd", "abort", reason);
                this._logger.LogInformation("{Reason} Product {ProductId} was not notified about.",
                    reason, product.Id);

                throw new ProcessingAbortedException(reason);
            }

            TraceContext.Emit("productgepubliceerd", "ok", "product is published");
        }
        #endregion

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
