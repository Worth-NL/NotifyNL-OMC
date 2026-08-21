// © 2026, Worth Systems.

using System.Text.RegularExpressions;
using Common.Settings.Configuration;
using Common.Settings.Extensions;
using Microsoft.Extensions.Logging;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Print.Interfaces;
using WebQueries.Print.Models;
using WebQueries.Register.Interfaces;
using WebQueries.Tracing;
using ZgwModels.Enums;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Enums.Urns;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.Objecten.Print;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak.Documents;
using ZgwModels.Mapping.Models.Urns;

namespace WebQueries.Print
{
    /// <summary>
    /// Handles the "Print" scenario: an object in the "Objecten" Web API service points at an
    /// already-composed PDF, which is fetched and handed to "Notify NL" to be printed and posted.
    /// </summary>
    /// <remarks>
    ///   OMC composes nothing here. The PDF is the letter, printed verbatim ("zonder oplegger"), so it is
    ///   sent as a <em>precompiled</em> letter rather than through a template - which also means the
    ///   recipient's address is read by "Notify NL" from the PDF's own address window, not passed
    ///   separately. The BSN in the object's URN is only used to resolve the partij the contactmoment is
    ///   registered against.
    /// </remarks>
    public sealed class PrintScenarioImplementation : IPrintScenario
    {
        /// <summary>
        /// A BSN is always exactly nine digits. Matching the shape before attempting a lookup turns an
        /// obviously wrong URN into a precise error instead of a failed party search.
        /// </summary>
        private static readonly Regex s_bsnPattern = new("^[0-9]{9}$", RegexOptions.Compiled);

        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IHttpClientFactory<INotifyClient, string> _notifyClientFactory;
        private readonly ITelemetryService _telemetry;
        private readonly OmcConfiguration _configuration;
        private readonly ILogger<PrintScenarioImplementation> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintScenarioImplementation"/> class.
        /// </summary>
        /// <param name="dataQuery">Resolves an <see cref="IQueryContext"/> for fetching object/party/document data.</param>
        /// <param name="notifyClientFactory">Resolves an <see cref="INotifyClient"/> per send, keyed by object UUID.</param>
        /// <param name="telemetry">Registers the resulting contactmoment.</param>
        /// <param name="configuration">The application configuration (whitelist, endpoints).</param>
        /// <param name="logger">The logger for this scenario.</param>
        public PrintScenarioImplementation(
            IDataQueryService<NotificationEvent> dataQuery,
            IHttpClientFactory<INotifyClient, string> notifyClientFactory,
            ITelemetryService telemetry,
            OmcConfiguration configuration,
            ILogger<PrintScenarioImplementation> logger)  // Dependency Injection (DI)
        {
            this._dataQuery = dataQuery;
            this._notifyClientFactory = notifyClientFactory;
            this._telemetry = telemetry;
            this._configuration = configuration;
            this._logger = logger;
        }

        /// <inheritdoc cref="IPrintScenario.ProcessPrintAsync(NotificationEvent)"/>
        async Task<HttpRequestResponse> IPrintScenario.ProcessPrintAsync(NotificationEvent notification)
        {
            // Step 1: Printing has to be switched on for this environment
            if (!this._configuration.ZGW.Whitelist.Print_Allowed())
            {
                string whitelistName = ConfigExtensions.GetWhitelistPrintAllowedEnvVarName();
                TraceContext.Emit("printschakelaar", "abort", $"{whitelistName} is disabled");

                return HttpRequestResponse.Failure($"Printing is not allowed: {whitelistName} is disabled.");
            }
            TraceContext.Emit("printschakelaar", "ok", $"{ConfigExtensions.GetWhitelistPrintAllowedEnvVarName()} is enabled");

            IQueryContext queryContext = this._dataQuery.From(notification);

            // Step 2: Read the object that triggered this print job
            Guid objectId = notification.MainObjectUri.GetGuid();
            TraceContext.Emit("objecten", "start", $"Attempting to retrieve print object with id {objectId}");

            PrintData printData;
            try
            {
                printData = (await queryContext.GetPrintObjectAsync()).Record.Data;
            }
            catch (Exception exception)
            {
                TraceContext.Emit("objecten", "fail", exception.Message);
                throw;
            }
            TraceContext.Emit("objecten", "ok", $"print object with id {objectId} retrieved");

            // Step 3: The PDF URI is supplied by whoever wrote the object, so it is only trusted once it
            // has been shown to point into our own Documenten API
            if (!TryResolveDocumentUuid(printData.PdfUri, out Guid documentUuid, out string pdfUriFailure))
            {
                TraceContext.Emit("documentcheck", "abort", pdfUriFailure);

                return HttpRequestResponse.Failure(pdfUriFailure);
            }
            TraceContext.Emit("documentcheck", "ok", $"pdfurl points at document {documentUuid} in the configured Documenten API");

            // Step 4: Resolve who the letter is about. Only BSN-bearing URNs can be resolved today; anything
            // else is rejected by name rather than silently dropped (KVK needs #171-#173/#205 first).
            if (!TryResolveBsn(printData.BetrokkeneUrn, out string bsnNumber, out string urnFailure))
            {
                TraceContext.Emit("betrokkeneurn", "abort", urnFailure);

                return HttpRequestResponse.Failure(urnFailure);
            }
            TraceContext.Emit("betrokkeneurn", "ok", "contact_betrokkene_urn carries a BSN");

            // Step 5: Resolve the partij the contactmoment will be registered against. The trace log never
            // carries the BSN itself (see TraceEvent's structural-only, no-PII rule).
            //
            // requireDigitalAddress is deliberately false: this flow posts a physical letter, so a citizen
            // with no e-mail or phone number on file is a perfectly normal recipient rather than a failure.
            // Leaving it at its default would reject exactly the people a printed letter exists for.
            TraceContext.Emit("openklant", "start", "Attempting to retrieve klant");
            CommonPartyData party;
            try
            {
                party = await queryContext.GetPartyDataAsync(
                    caseUri: null, bsnNumber: bsnNumber, caseIdentifier: null, requireDigitalAddress: false);
            }
            catch (Exception exception)
            {
                TraceContext.Emit("openklant", "fail", exception.Message);

                return HttpRequestResponse.Failure(
                    $"The party behind contact_betrokkene_urn could not be resolved: {exception.Message}");
            }
            TraceContext.Emit("openklant", "ok", "klant retrieved");

            // Step 6: Fetch the PDF itself
            TraceContext.Emit("documenten", "start", $"Attempting to download document {documentUuid}");
            byte[] pdfContents;
            try
            {
                pdfContents = await DownloadPdfAsync(queryContext, documentUuid);
            }
            catch (Exception exception)
            {
                TraceContext.Emit("documenten", "fail", exception.Message);

                return HttpRequestResponse.Failure($"The PDF referenced by pdfurl could not be downloaded: {exception.Message}");
            }

            if (pdfContents.Length == 0)
            {
                TraceContext.Emit("documenten", "fail", $"document {documentUuid} has no content");

                return HttpRequestResponse.Failure($"The document referenced by pdfurl ({documentUuid}) has no content to print.");
            }
            TraceContext.Emit("documenten", "ok", $"document {documentUuid} downloaded ({pdfContents.Length} bytes)");

            // Step 7: Hand the letter to "Notify NL" verbatim
            TraceContext.Emit("notifynl", "start", "Attempting to send precompiled letter");
            INotifyClient notifyClient = this._notifyClientFactory.GetHttpClient(objectId.ToString());

            NotifySendResponse sendResponse = await notifyClient.SendPrecompiledLetterAsync(
                reference: objectId.ToString(),
                pdfContents: pdfContents);

            if (!sendResponse.IsSuccess)
            {
                TraceContext.Emit("notifynl", "fail", sendResponse.Error);

                return HttpRequestResponse.Failure($"Precompiled letter rejected: {sendResponse.Error}");
            }
            TraceContext.Emit("notifynl", "ok", "precompiled letter accepted");

            // Step 8: Register what just happened. A failure here is reported but does not undo the send -
            // the letter is already on its way, and re-sending it on a retry would post it twice.
            var reference = new PrintNotifyReference
            {
                ObjectId = objectId,
                PartyId = party.Uri.GetGuid(),
                Subject = printData.Subject,
                SubjectObject = printData.SubjectObjectIdentifier,
                AttachmentId = documentUuid
            };

            TraceContext.Emit("openklant", "start", "Attempting to register contactmoment");
            HttpRequestResponse registerResponse = await this._telemetry.ReportPrintCompletionAsync(
                reference,
                NotifyMethods.Letter,
                printData.Subject,
                string.Empty,
                "true",
                DateTime.UtcNow.ToString("O"));

            if (registerResponse.IsFailure)
            {
                TraceContext.Emit("openklant", "fail", registerResponse.JsonResponse);
                this._logger.LogError(
                    "Print job {ObjectId}: the letter was accepted by Notify NL but registering its contactmoment failed. The object is deliberately left in place so the registration can be retried without reprinting.",
                    objectId);

                return HttpRequestResponse.Failure(
                    $"Precompiled letter was sent, but registering the contactmoment failed: {registerResponse.JsonResponse}");
            }
            TraceContext.Emit("openklant", "ok", "contactmoment registered");

            // Step 9: The object exists only to request the print, so it is removed once the print has
            // actually been requested and recorded (MBO-1025: "Verwijderen object na printen").
            TraceContext.Emit("objecten", "start", $"Attempting to delete print object {objectId}");
            HttpRequestResponse deleteResponse = await queryContext.DeleteObjectAsync(objectId);

            if (deleteResponse.IsFailure)
            {
                // Not fatal: the letter is printed and the contactmoment recorded, so failing the whole job
                // here would invite a retry that prints a second copy. Left loud in the log instead.
                TraceContext.Emit("objecten", "fail", deleteResponse.JsonResponse);
                this._logger.LogWarning(
                    "Print job {ObjectId}: the letter was sent and registered, but the print object could not be deleted: {Reason}",
                    objectId, deleteResponse.JsonResponse);

                return HttpRequestResponse.Success(
                    $"Precompiled letter sent and contactmoment registered. The print object could not be deleted: {deleteResponse.JsonResponse}");
            }
            TraceContext.Emit("objecten", "ok", $"print object {objectId} deleted");

            return HttpRequestResponse.Success("Precompiled letter sent, contactmoment registered and print object deleted.");
        }

        #region Helper methods
        /// <summary>
        /// Validates that <paramref name="pdfUri"/> points into the configured "Documenten" Web API service
        /// and extracts the document's UUID from it.
        /// </summary>
        /// <remarks>
        ///   Without this check, anyone able to write an object could make OMC fetch an arbitrary URL with
        ///   OMC's own credentials attached - a fetch-anything proxy. The comparison is on scheme, host and
        ///   port rather than a string prefix, so neither a different port nor a lookalike path
        ///   ("https://evil.test/https://documenten.internal/...") can slip past.
        /// </remarks>
        private bool TryResolveDocumentUuid(Uri? pdfUri, out Guid documentUuid, out string failureReason)
        {
            documentUuid = Guid.Empty;

            if (pdfUri is null)
            {
                failureReason = "The print object carries no pdfurl.";

                return false;
            }

            if (!pdfUri.IsAbsoluteUri)
            {
                failureReason = $"The pdfurl \"{pdfUri}\" is not an absolute URI.";

                return false;
            }

            string configuredDomain = this._configuration.ZGW.Endpoint.Documenten();

            if (!Uri.TryCreate(EnsureScheme(configuredDomain), UriKind.Absolute, out Uri? documentenUri))
            {
                failureReason = $"The configured Documenten API domain (\"{configuredDomain}\") is not a valid absolute URI.";

                return false;
            }

            bool sameOrigin =
                string.Equals(pdfUri.Scheme, documentenUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(pdfUri.Host, documentenUri.Host, StringComparison.OrdinalIgnoreCase) &&
                pdfUri.Port == documentenUri.Port;

            if (!sameOrigin)
            {
                failureReason =
                    $"The pdfurl host \"{pdfUri.Host}\" is not the configured Documenten API (\"{documentenUri.Host}\"); refusing to fetch it.";

                return false;
            }

            documentUuid = pdfUri.GetGuid();

            if (documentUuid == Guid.Empty)
            {
                failureReason = $"The pdfurl \"{pdfUri}\" does not end in a document UUID.";

                return false;
            }

            failureReason = string.Empty;

            return true;
        }

        /// <summary>
        /// Supplies the scheme the configured domain may have been written without, so that it can be
        /// compared as a URI rather than as text.
        /// </summary>
        private static string EnsureScheme(string domain)
            => domain.Contains("://", StringComparison.Ordinal) ? domain : $"https://{domain}";

        /// <summary>
        /// Extracts the BSN from a "betrokkene" URN, rejecting anything OMC cannot resolve yet.
        /// </summary>
        private static bool TryResolveBsn(string betrokkeneUrn, out string bsnNumber, out string failureReason)
        {
            bsnNumber = string.Empty;

            if (!BetrokkeneUrn.TryParse(betrokkeneUrn, out BetrokkeneUrn parsed))
            {
                failureReason = string.IsNullOrWhiteSpace(betrokkeneUrn)
                    ? "The print object carries no contact_betrokkene_urn."
                    : $"The contact_betrokkene_urn \"{betrokkeneUrn}\" is not a URN OMC can read.";

                return false;
            }

            if (parsed.Namespace != UrnNamespaces.Bsn)
            {
                // Deliberately names the namespace that was supplied: a KVK URN is a known, expected case
                // that simply is not supported until KVK party lookup lands, and saying so is more useful
                // than "unsupported URN".
                failureReason =
                    $"The contact_betrokkene_urn identifies a \"{parsed.NamespaceSegment}\", but only BSN-based URNs can be resolved to a partij.";

                return false;
            }

            if (!s_bsnPattern.IsMatch(parsed.Value))
            {
                failureReason = "The contact_betrokkene_urn names a BSN, but its value is not a nine-digit number.";

                return false;
            }

            bsnNumber = parsed.Value;
            failureReason = string.Empty;

            return true;
        }

        /// <summary>
        /// Downloads the PDF behind a document and returns its raw bytes.
        /// </summary>
        /// <remarks>
        ///   "inhoud" is a download link on GET, not the file content itself (see
        ///   <see cref="SingularInformationObject.Content"/>'s remarks), so the bytes need a second,
        ///   explicit fetch. That fetch hands back Base64, which the precompiled-letter call does not want -
        ///   it takes the raw bytes.
        /// </remarks>
        private static async Task<byte[]> DownloadPdfAsync(IQueryContext queryContext, Guid documentUuid)
        {
            SingularInformationObject document = await queryContext.GetDocumentAsync(documentUuid);

            if (string.IsNullOrEmpty(document.Content))
            {
                return [];
            }

            string base64Content = await queryContext.GetDocumentContentAsync(new Uri(document.Content));

            return string.IsNullOrEmpty(base64Content)
                ? []
                : Convert.FromBase64String(base64Content);
        }
        #endregion
    }
}
