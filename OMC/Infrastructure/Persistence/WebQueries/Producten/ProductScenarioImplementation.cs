// © 2026, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Register.Interfaces;
using ZgwModels.Enums;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using ZgwModels.Serialization.Interfaces;
using WebQueries.Exceptions;
using WebQueries.Producten.Interfaces;
using WebQueries.Producten.Models;
using WebQueries.Tracing;
using ZgwModels.Exceptions;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Enums.OpenKlant;
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
        private const string CodeSoortObjectIdKvk = "kvk_nummer";

        /// <summary>
        /// The "referentie" marking the digital address a party chose for portal correspondence.
        /// </summary>
        /// <remarks>
        ///   A product notification has no per-product address of its own to prefer, so the party's portal
        ///   preference is what it uses.
        /// </remarks>
        private const string PortaalvoorkeurReference = "portaalvoorkeur";

        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IHttpClientFactory<INotifyClient, string> _notifyClientFactory;
        private readonly ISerializationService _serializer;
        private readonly ITelemetryService _telemetry;
        private readonly OmcConfiguration _configuration;
        private readonly ILogger<ProductScenarioImplementation> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductScenarioImplementation"/> class.
        /// </summary>
        /// <param name="dataQuery">Resolves an <see cref="IQueryContext"/> for fetching product and party data.</param>
        /// <param name="notifyClientFactory">Resolves an <see cref="INotifyClient"/> to send with.</param>
        /// <param name="serializer">Serializes the reference round-tripped through "Notify NL".</param>
        /// <param name="telemetry">Registers the contactmoment for an owner who could not be notified.</param>
        /// <param name="configuration">The application configuration (whitelist, endpoints, templates).</param>
        /// <param name="logger">The logger for this scenario.</param>
        public ProductScenarioImplementation(
            IDataQueryService<NotificationEvent> dataQuery,
            IHttpClientFactory<INotifyClient, string> notifyClientFactory,
            ISerializationService serializer,
            ITelemetryService telemetry,
            OmcConfiguration configuration,
            ILogger<ProductScenarioImplementation> logger)  // Dependency Injection (DI)
        {
            this._dataQuery = dataQuery;
            this._notifyClientFactory = notifyClientFactory;
            this._serializer = serializer;
            this._telemetry = telemetry;
            this._configuration = configuration;
            this._logger = logger;
        }

        /// <inheritdoc cref="IProductScenario.ProcessProductAsync(NotificationEvent)"/>
        async Task<HttpRequestResponse> IProductScenario.ProcessProductAsync(NotificationEvent notification)
        {
            // Step 0: A deployment without "Open Product" has switched this scenario off
            ValidateOpenProductenIsConfigured();

            IQueryContext queryContext = this._dataQuery.From(notification);

            // Step 1: Read the product the notification reported
            Product product = await GetProductAsync(queryContext, notification);

            // Step 2: Only configured product types may notify anyone
            ValidateProductTypeIsWhitelisted(product);

            // Step 3: An unpublished product is not shown to its owners, so it is not announced to them either
            ValidateProductIsPublished(product);

            // Step 4: A product that is going to be announced needs a template to announce it with. Checked
            // before anyone is resolved or notified, so a missing one fails the whole notification (and is
            // retried) instead of quietly turning into a failed contactmoment per owner.
            Guid templateId = GetTemplateId();

            // Step 5: Every owner has to resolve to a party before anyone is notified
            IReadOnlyList<ProductRecipient> recipients = await ResolveOwnersAsync(queryContext, product);

            // Step 6: Everything that decides whether this notification can be honoured has now been
            // checked, so the answer is settled here. The sending still happens on this call, but nothing
            // about the answer depends on how it goes - what delivery can report, it reports as a
            // contactmoment, and the status below is the same either way.
            await DeliverAsync(product, recipients, templateId);

            return HttpRequestResponse.Success(
                $"Product {product.Id} accepted; notifying {recipients.Count} eigenaar(s).");
        }

        #region Delivery
        /// <summary>
        /// Notifies every owner of an already-validated product, and registers what happened to each.
        /// </summary>
        /// <remarks>
        ///   The answer to this notification is settled before this runs, so nothing here may throw its
        ///   way out: a delivery that fails is a failed contactmoment, never a different status code.
        ///   <para>
        ///     One owner failing does not stop the rest either - validating every party up front is what
        ///     bought the ability to record each failure on its own.
        ///   </para>
        /// </remarks>
        private async Task DeliverAsync(Product product, IReadOnlyList<ProductRecipient> recipients, Guid templateId)
        {
            int sent = 0;

            try
            {
                foreach (ProductRecipient recipient in recipients)
                {
                    if (await DeliverToAsync(product, recipient, templateId))
                    {
                        sent++;
                    }
                }
            }
            catch (Exception exception)
            {
                // Both the send and the registration of a failure handle their own exceptions, so nothing is
                // expected to land here. It is caught all the same: the notification has been accepted by
                // this point, and nothing that happens here is allowed to say otherwise.
                TraceContext.Emit("productbezorging", "fail", exception.Message);

                this._logger.LogError(exception,
                    "Delivering the notifications for product {ProductId} failed.", product.Id);

                return;
            }

            TraceContext.Emit(
                "productbezorging", sent == recipients.Count ? "ok" : "fail",
                $"{sent} of {recipients.Count} eigenaar(s) handed to \"Notify NL\"");
        }

        /// <summary>
        /// Notifies one owner, or records why it could not.
        /// </summary>
        /// <returns><see langword="true"/> when "Notify NL" accepted the notification.</returns>
        private async Task<bool> DeliverToAsync(Product product, ProductRecipient recipient, Guid templateId)
        {
            ProductNotifyReference reference = new()
            {
                ProductId = product.Id,
                PartyId = recipient.Party.Uri.GetGuid(),
                ProductName = product.Name,
                ProductTypeCode = product.ProductType.Code,
                OriginalResourceUrl = product.Uri?.AbsoluteUri ?? string.Empty,

                // Read from the ambient trace rather than passed in: delivery runs on the same call chain
                // as the notification that started it, so this is that notification's own trace.
                TraceId = TraceContext.CurrentTraceId,
                SentAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            if (!recipient.IsReachable)
            {
                await RegisterFailureAsync(reference,
                    "Er is geen e-mailadres bekend om deze notificatie naar te versturen.");

                return false;
            }

            NotifySendResponse response;
            try
            {
                // Sent through the client directly rather than through INotifyService, whose reference is
                // typed as NotifyReference and so cannot carry this one. Without the reference reaching
                // "Notify NL", the delivery receipt comes back unrecognisable and the contactmoment for a
                // notification that did arrive would never be written. Same reason the print flow does it.
                INotifyClient notifyClient = this._notifyClientFactory.GetHttpClient(product.Id.ToString());

                response = await notifyClient.SendEmailAsync(
                    emailAddress: recipient.EmailAddress,
                    templateId: templateId.ToString(),
                    personalization: GetPersonalization(product, recipient.Party),
                    reference: await this._serializer.Serialize(reference).CompressGZipAsync(CancellationToken.None));
            }
            catch (Exception exception)
            {
                // A throw here is the same outcome as a refusal, and it must not stop the other owners.
                await RegisterFailureAsync(reference, exception.Message);

                return false;
            }

            if (response.IsFailure)
            {
                await RegisterFailureAsync(reference, response.Error);

                return false;
            }

            // Nothing is registered on success. "Notify NL" accepting the request is not delivery, and the
            // contactmoment is written from the delivery receipt - the same way every other channel does
            // it, and the reason a receipt that never arrives leaves no contactmoment claiming otherwise.
            this._logger.LogInformation(
                "Notification for product {ProductId} sent to one eigenaar, awaiting delivery confirmation.",
                product.Id);

            return true;
        }

        /// <summary>
        /// Records an owner OMC could not notify, against that owner's party.
        /// </summary>
        /// <remarks>
        ///   These never reach "Notify NL", so no delivery receipt is coming and nothing can fetch back
        ///   what was rendered. The configured failure wording stands in for it, the same wording the
        ///   e-mail and SMS scenarios use when a receipt reports a failure.
        /// </remarks>
        private async Task RegisterFailureAsync(ProductNotifyReference reference, string reason)
        {
            this._logger.LogWarning(
                "Could not notify an eigenaar of product {ProductId}: {Reason}", reference.ProductId, reason);

            try
            {
                HttpRequestResponse response = await this._telemetry.ReportProductCompletionAsync(
                    reference,
                    NotifyMethods.Email,
                    messages:
                    [
                        this._configuration.AppSettings.Variables.UxMessages.Email_Failure_Subject(),
                        AppendReason(this._configuration.AppSettings.Variables.UxMessages.Email_Failure_Body(), reason),
                        "false",
                        DateTime.Now.ToString("O")
                    ]);

                if (response.IsFailure)
                {
                    // The notification is already lost; losing its record as well is worth an error, but
                    // there is nobody left to report it to.
                    this._logger.LogError(
                        "Registering the failed contactmoment for product {ProductId} also failed: {Error}",
                        reference.ProductId, response.JsonResponse);
                }
            }
            catch (Exception exception)
            {
                // A register that throws rather than reporting a failure is the same outcome, and this one
                // owner's record is not worth the remaining owners' notifications.
                this._logger.LogError(exception,
                    "Registering the failed contactmoment for product {ProductId} also failed.",
                    reference.ProductId);
            }
        }

        /// <summary>
        /// Appends the reason a notification failed to the configured failure wording.
        /// </summary>
        /// <remarks>
        ///   The reason is what makes a failed contactmoment useful to whoever reads it: without it, an owner with
        ///   no e-mail address, a refused send and a thrown exception all read the same. It is kept short, because
        ///   it can carry the text of an exception or of an error from "Notify NL".
        /// </remarks>
        private static string AppendReason(string failureBody, string reason)
        {
            const int maxReasonLength = 300;

            string trimmedReason = reason.Trim();

            if (trimmedReason.Length > maxReasonLength)
            {
                trimmedReason = $"{trimmedReason[..maxReasonLength]}…";
            }

            return string.IsNullOrWhiteSpace(failureBody)
                ? $"Reden: {trimmedReason}"
                : $"{failureBody}\n\nReden: {trimmedReason}";
        }

        /// <summary>
        /// Builds the template personalization for one owner.
        /// </summary>
        /// <remarks>
        ///   A fresh dictionary per recipient, deliberately. The other scenarios hand back a shared static
        ///   one guarded by a lock that covers writing it but not the caller's reading of it, which is
        ///   survivable for a single recipient and is not for a fan-out.
        /// </remarks>
        private static Dictionary<string, object> GetPersonalization(Product product, CommonPartyData party)
        {
            return new Dictionary<string, object>
            {
                ["klant.voornaam"] = party.Name,
                ["klant.voorvoegselAchternaam"] = party.SurnamePrefix,
                ["klant.achternaam"] = party.Surname,

                ["producttype.naam"] = product.ProductType.Name,
                ["product.naam"] = product.Name,
                ["product.status"] = product.Status
            };
        }
        #endregion

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
        private async Task<IReadOnlyList<ProductRecipient>> ResolveOwnersAsync(IQueryContext queryContext, Product product)
        {
            if (product.Owners.IsEmpty())
            {
                const string reason = "Product has no eigenaren to notify.";

                TraceContext.Emit("openklant", "abort", reason);
                this._logger.LogInformation("{Reason} Product {ProductId} was not notified about.", reason, product.Id);

                throw new ProcessingAbortedException(reason);
            }

            TraceContext.Emit("openklant", "start", $"Attempting to resolve {product.Owners.Count} eigenaar(s)");

            List<ProductRecipient> recipients = new(product.Owners.Count);

            for (int index = 0; index < product.Owners.Count; index++)
            {
                CommonPartyData party = await ResolveOwnerAsync(queryContext, product, product.Owners[index], index);

                recipients.Add(new ProductRecipient
                {
                    Party = party,

                    // Only e-mail in V1, and the lookup was restricted to that channel, so this is either
                    // an e-mail address or nothing - never a phone number.
                    EmailAddress = party.EmailAddress
                });
            }

            int reachable = recipients.Count(recipient => recipient.IsReachable);

            TraceContext.Emit("openklant", "ok",
                $"{recipients.Count} eigenaar(s) resolved to a partij, {reachable} with an e-mail address");

            if (reachable < recipients.Count)
            {
                // Not a reason to stop - these become failed contactmomenten during delivery.
                this._logger.LogInformation(
                    "{Unreachable} of {Total} eigenaren of product {ProductId} have no e-mail address on file.",
                    recipients.Count - reachable, recipients.Count, product.Id);
            }

            return recipients;
        }

        /// <summary>
        /// Resolves the party behind a single owner.
        /// </summary>
        /// <exception cref="ProcessingAbortedException">The owner carries no usable identifier, or has no party.</exception>
        /// <exception cref="HttpRequestException">"OpenKlant" could not be reached or refused the call; the sender retries.</exception>
        private async Task<CommonPartyData> ResolveOwnerAsync(
            IQueryContext queryContext, Product product, Owner owner, int index)
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
                string reason = $"Owner {index + 1} of {product.Owners.Count} carries no BSN or KVK number.";

                TraceContext.Emit("openklant", "abort", reason);
                this._logger.LogInformation("{Reason} Product {ProductId} was not notified about.", reason, product.Id);

                throw new ProcessingAbortedException(reason);
            }

            try
            {
                // The digital address is resolved here too, but not required: see ResolveOwnersAsync.
                //
                // Passing the reference makes an address whose "referentie" is "portaalvoorkeur" win
                // outright, falling back to the party's own preferred address and then to any e-mail it
                // has. That is the precedence a product notification wants, and it is already implemented
                // by PartyResults - see its IsPreferredFound.
                //
                // Restricting the channel matters as much as the reference does. Without it, a party whose
                // preferred address happens to be a phone number reads as having no e-mail at all: the
                // search settles on that address and never reaches the e-mail behind it, so an owner who
                // could have been notified is recorded as unreachable instead.
                return await queryContext.GetPartyDataByIdentifierAsync(
                    codeSoortObjectId, objectId,
                    reference: PortaalvoorkeurReference, requireDigitalAddress: false,
                    requiredChannel: DistributionChannels.Email);
            }
            // Only a party that is genuinely not there aborts. OpenKlant being down, timing out or refusing
            // the call is a plain HttpRequestException and is left to propagate as a failure, so the sender
            // retries it - an outage must not be mistaken for missing data and dropped for good.
            catch (PartyNotFoundException exception)
            {
                // The identifier itself is never traced or logged - it is a BSN or a KVK number. Its kind is,
                // so whoever reads the reason knows which register to correct.
                string identifierKind = codeSoortObjectId == CodeSoortObjectIdBsn ? "BSN" : "KVK number";

                string reason =
                    $"No partij found in OpenKlant for the {identifierKind} of owner {index + 1} of " +
                    $"{product.Owners.Count} of product {product.Id} (partijIdentificator \"{codeSoortObjectId}\").";

                TraceContext.Emit("openklant", "abort", reason);
                this._logger.LogWarning("{Reason} Nobody was notified about this product.", reason);

                throw new ProcessingAbortedException(reason, exception);
            }
            catch (Exception exception)
            {
                TraceContext.Emit("openklant", "fail", exception.Message);
                throw;
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// Rejects every product notification when this deployment does not use "Open Product".
        /// </summary>
        /// <remarks>
        ///   "Open Product" is optional, and leaving its endpoint unset is how a deployment opts out. That is a
        ///   decision, not a defect, so an event that arrives anyway (e.g. a subscription to the "producten"
        ///   kanaal left in place) is aborted rather than failed: retrying it would only fail the same way.
        /// </remarks>
        /// <exception cref="ProcessingAbortedException">"Open Product" is not configured.</exception>
        private void ValidateOpenProductenIsConfigured()
        {
            if (this._configuration.ZGW.Endpoint.IsOpenProductenConfigured())
            {
                return;
            }

            const string reason =
                "Open Product is not configured (ZGW_ENDPOINT_OPENPRODUCTEN is not set), " +
                "so the \"Product created\" scenario is disabled.";

            TraceContext.Emit("openproduct", "abort", reason);
            this._logger.LogInformation("{Reason} Notification dropped without notifying anyone.", reason);

            throw new ProcessingAbortedException(reason);
        }

        /// <summary>
        /// Reads the e-mail template a product announcement is sent with.
        /// </summary>
        /// <remarks>
        ///   The setting is optional for deployments that do not use "Open Product", but one that does and has
        ///   whitelisted a product type cannot do without it. That is a misconfiguration, so it fails - and is
        ///   retried - rather than being aborted like a product that simply should not be announced.
        /// </remarks>
        /// <exception cref="InvalidOperationException">No template is configured.</exception>
        private Guid GetTemplateId()
        {
            Guid templateId = this._configuration.Notify.TemplateId.Email.ProductCreated();

            if (templateId != Guid.Empty)
            {
                return templateId;
            }

            const string reason =
                "No e-mail template is configured for the \"Product created\" scenario " +
                "(NOTIFY_TEMPLATEID_EMAIL_PRODUCTCREATED is not set).";

            TraceContext.Emit("productbezorging", "fail", reason);

            throw new InvalidOperationException(reason);
        }

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
