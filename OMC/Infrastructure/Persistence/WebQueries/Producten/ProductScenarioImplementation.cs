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
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenProducten;

namespace WebQueries.Producten
{
    /// <inheritdoc cref="IProductScenario"/>
    public sealed class ProductScenarioImplementation : IProductScenario
    {
        /// <summary>
        /// The "codeSoortObjectId" OpenKlant stores a citizen's BSN under.
        /// </summary>
        private const string CodeSoortObjectIdBsn = "bsn";

        /// <summary>
        /// The "codeSoortObjectId" OpenKlant stores an organization's KVK number under.
        /// </summary>
        private const string CodeSoortObjectIdKvk = "kvk";

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

            // Step 4: Every owner has to resolve to a party before anyone is notified
            IReadOnlyList<CommonPartyData> parties = await ResolveOwnersAsync(queryContext, product);

            // TODO: The fire-and-forget send with its contactmomenten lands in the follow-up commits for
            //       Worth-NL/notifynl#115 - #116.
            throw new NotImplementedException();
        }

        #region Owners
        /// <summary>
        /// Resolves the party behind every owner of the product.
        /// </summary>
        /// <remarks>
        ///   All or nothing: one owner that cannot be resolved stops the whole notification, and nobody is
        ///   notified. Sending to the others and recording the odd one out is not an option, because the
        ///   record OMC would write is a klantcontact with a "betrokkene" - and the one thing missing here
        ///   is precisely the party that field names. Aborting keeps every failure that is reported later
        ///   attached to a party that actually exists.
        ///   <para>
        ///     Digital addresses are not required yet. Whether an owner can actually be reached is decided
        ///     during delivery, where a party with no e-mail address becomes a failed contactmoment rather
        ///     than a reason to drop the whole product.
        ///   </para>
        /// </remarks>
        /// <exception cref="ProcessingAbortedException">The product has no owners, or one of them did not resolve.</exception>
        private async Task<IReadOnlyList<CommonPartyData>> ResolveOwnersAsync(IQueryContext queryContext, Product product)
        {
            if (product.Owners.IsEmpty())
            {
                const string reason = "Product has no eigenaren to notify.";

                TraceContext.Emit("openklant", "abort", reason);
                this._logger.LogInformation("{Reason} Product {ProductId} was not notified about.", reason, product.Id);

                throw new ProcessingAbortedException(reason);
            }

            TraceContext.Emit("openklant", "start", $"Attempting to resolve {product.Owners.Count} eigenaar(s)");

            List<CommonPartyData> parties = new(product.Owners.Count);

            for (int index = 0; index < product.Owners.Count; index++)
            {
                parties.Add(await ResolveOwnerAsync(queryContext, product, product.Owners[index], index));
            }

            TraceContext.Emit("openklant", "ok", $"{parties.Count} eigenaar(s) resolved to a partij");

            return parties;
        }

        /// <summary>
        /// Resolves the party behind a single owner.
        /// </summary>
        /// <exception cref="ProcessingAbortedException">The owner carries no usable identifier, or has no party.</exception>
        private async Task<CommonPartyData> ResolveOwnerAsync(
            IQueryContext queryContext, Product product, Eigenaar owner, int index)
        {
            // "Open Product" validates that an owner carries either a BSN (and/or a customer number) or a
            // KVK number, never both, so these two branches cannot both apply. BSN is still checked first,
            // so that an owner carrying both because that rule ever loosens resolves as a citizen.
            (string codeSoortObjectId, string objectId) = owner switch
            {
                { BsnNumber.Length: > 0 } => (CodeSoortObjectIdBsn, owner.BsnNumber),
                { KvkNumber.Length: > 0 } => (CodeSoortObjectIdKvk, owner.KvkNumber),

                _ => (string.Empty, string.Empty)
            };

            if (codeSoortObjectId.Length == 0)
            {
                // A customer number is the remaining possibility. OpenKlant only matches it through a
                // filter its own schema marks deprecated, so V1 does not chase it.
                string reason = $"Eigenaar {index + 1} of {product.Owners.Count} carries no BSN or KVK number.";

                TraceContext.Emit("openklant", "abort", reason);
                this._logger.LogInformation("{Reason} Product {ProductId} was not notified about.", reason, product.Id);

                throw new ProcessingAbortedException(reason);
            }

            try
            {
                // The digital address is resolved here too, but not required: see ResolveOwnersAsync.
                return await queryContext.GetPartyDataByIdentifierAsync(
                    codeSoortObjectId, objectId, reference: null, requireDigitalAddress: false);
            }
            catch (Exception exception) when (exception is HttpRequestException or KeyNotFoundException)
            {
                // The identifier itself is never traced or logged - it is a BSN or a KVK number.
                string reason =
                    $"Eigenaar {index + 1} of {product.Owners.Count} ({codeSoortObjectId}) has no partij in OpenKlant.";

                TraceContext.Emit("openklant", "abort", reason);
                this._logger.LogInformation("{Reason} Product {ProductId} was not notified about.", reason, product.Id);

                throw new ProcessingAbortedException(reason, exception);
            }
        }
        #endregion

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
