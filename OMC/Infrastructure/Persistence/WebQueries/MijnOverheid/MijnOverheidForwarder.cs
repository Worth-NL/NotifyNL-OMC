using Common.Constants;
using Common.Settings.Configuration;
using Microsoft.Extensions.Logging;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.MijnOverheid.Enums;
using WebQueries.MijnOverheid.Interfaces;
using WebQueries.MijnOverheid.Models;
using WebQueries.Tracing;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

// Type aliases to distinguish the two CloudEvent types
using IncomingCloudEvent = ZgwModels.Mapping.Events.CloudEvent;
using OutgoingCloudEvent = WebQueries.MijnOverheid.Models.CloudEvent;

namespace WebQueries.MijnOverheid
{
    /// <summary>
    /// Forwards CloudEvents to MijnOverheid after applying relevant business rules,
    /// including whitelist filtering and timestamp‑based deduplication.
    /// </summary>
    public class MijnOverheidForwarder : IMijnOverheidForwarder
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IMijnOverheidClient _mijnOverheidClient;
        private readonly OmcConfiguration _configuration;
        private readonly ILogger<MijnOverheidForwarder> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MijnOverheidForwarder"/> class.
        /// </summary>
        /// <param name="dataQuery">Data query service for OpenZaak.</param>
        /// <param name="mijnOverheidClient">Client to send events to MijnOverheid.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="logger">Logger instance.</param>
        public MijnOverheidForwarder(
            IDataQueryService<NotificationEvent> dataQuery,
            IMijnOverheidClient mijnOverheidClient,
            OmcConfiguration configuration,
            ILogger<MijnOverheidForwarder> logger)
        {
            _dataQuery = dataQuery;
            _mijnOverheidClient = mijnOverheidClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Determines whether the incoming CloudEvent should be forwarded to MijnOverheid,
        /// and if so, sends it.
        /// </summary>
        /// <param name="cloudEvent">The incoming CloudEvent (from ZhvModels).</param>
        /// <returns>A <see cref="MijnOverheidResponse"/> if forwarded; otherwise <c>null</c>.</returns>
        public async Task<MijnOverheidResponse?> ForwardIfNeededAsync(IncomingCloudEvent cloudEvent)
        {
            // 1. Validate Subject
            if (string.IsNullOrEmpty(cloudEvent.Subject))
            {
                _logger.LogWarning("CloudEvent has no Subject.");
                return null;
            }

            if (!Guid.TryParse(cloudEvent.Subject, out Guid caseUuid))
            {
                _logger.LogWarning("Invalid case UUID in subject: {Subject}", cloudEvent.Subject);
                return null;
            }

            // 2. Parse event type
            MijnOverheidEventType eventType = ParseEventType(cloudEvent.Type);
            if (eventType == MijnOverheidEventType.Unknown)
            {
                _logger.LogWarning("Unknown CloudEvent type: {EventType}, skipping.", cloudEvent.Type);
                return null;
            }

            // Tells the dashboard which of the three MijnZaken flows to animate — set as soon as
            // we know it, before any register/filter step below emits its first trace event.
            TraceContext.SetScenario(eventType switch
            {
                MijnOverheidEventType.CaseMutated => "mijnzaken-gemuteerd",
                MijnOverheidEventType.CaseOpened => "mijnzaken-geopend",
                MijnOverheidEventType.CaseDeleted => "mijnzaken-verwijderd",
                _ => "mijnzaken-gemuteerd" // unreachable — Unknown already returned above
            });

            // 3. Create a single query context using the case URI
            string caseUrl = $"{_configuration.ZGW.Endpoint.OpenZaak()}/zaken/{caseUuid}";
            var caseUri = new Uri(caseUrl);

            var dummyNotification = new NotificationEvent
            {
                MainObjectUri = caseUri,
                Channel = Channels.Cases,
                Resource = Resources.Case,
                Action = Actions.Update
            };

            IQueryContext queryContext = _dataQuery.From(dummyNotification);

            // 4. Delegate to type-specific handler
            return eventType switch
            {
                MijnOverheidEventType.CaseDeleted => await HandleDeletedAsync(cloudEvent),
                MijnOverheidEventType.CaseOpened => await HandleOpenedAsync(cloudEvent, queryContext, caseUuid),
                MijnOverheidEventType.CaseMutated => await HandleMutatedAsync(cloudEvent, queryContext, caseUuid),
                _ => null
            };
        }

        #region Event handlers

        /// <summary>
        /// Handles a "zaak-verwijderd" event – forwards it unconditionally without fetching case data.
        /// </summary>
        /// <param name="cloudEvent">The incoming CloudEvent.</param>
        /// <returns>The response from MijnOverheid, or <c>null</c> if sending fails.</returns>
        private async Task<MijnOverheidResponse?> HandleDeletedAsync(IncomingCloudEvent cloudEvent)
        {
            _logger.LogInformation("Processing deletion event for case {Subject}. Forwarding directly.", cloudEvent.Subject);

            OutgoingCloudEvent outgoingEvent = CreateOutgoingEvent(cloudEvent);
            return await SendAndTraceAsync(outgoingEvent);
        }

        /// <summary>
        /// Handles a "zaak-geopend" event – forwards only if the event time corresponds
        /// to the case's current <see cref="Case.LastOpenedDate"/> (stateless check).
        /// </summary>
        /// <param name="cloudEvent">The incoming CloudEvent.</param>
        /// <param name="queryContext">The shared query context for OpenZaak.</param>
        /// <param name="caseUuid">The UUID of the case.</param>
        /// <returns>The response from MijnOverheid, or <c>null</c> if the event is stale or sending fails.</returns>
        private async Task<MijnOverheidResponse?> HandleOpenedAsync(
            IncomingCloudEvent cloudEvent,
            IQueryContext queryContext,
            Guid caseUuid)
        {
            _logger.LogInformation("Processing 'geopend' event for case {Subject}.", cloudEvent.Subject);

            // Fetch the case to get the current LatestOpenedDate
            Uri caseUri;
            Case caseData;
            TraceContext.Emit("openzaak", "start", $"Attempting to retrieve zaak with id {caseUuid}");
            try
            {
                caseUri = new Uri($"{_configuration.ZGW.Endpoint.OpenZaak()}/zaken/{caseUuid}");
                caseData = await queryContext.GetCaseAsync(caseUri);
                TraceContext.Emit("openzaak", "ok", $"zaak with id {caseUuid} retrieved");
            }
            catch (Exception ex)
            {
                TraceContext.Emit("openzaak", "fail", ex.Message);
                _logger.LogError(ex, "Failed to fetch case {CaseUuid} for 'geopend' event.", caseUuid);
                return null;
            }

            // If the case has no LatestOpenedDate, this is the first open – forward.
            if (!caseData.LastOpenedDate.HasValue)
            {
                _logger.LogDebug("Case {CaseId} has no LatestOpenedDate; forwarding 'geopend' event.", caseData.Identification);
                OutgoingCloudEvent outgoing = CreateOutgoingEvent(cloudEvent);
                return await SendAndTraceAsync(outgoing);
            }

            // Check initiator type
            TraceContext.Emit("naturalpersoncheck", "start", "Attempting to verify the initiator is a natural person");
            bool isNaturalPerson = await IsInitiatorNaturalPersonAsync(queryContext, caseUri, caseData.Identification);
            if (!isNaturalPerson)
            {
                TraceContext.Emit("naturalpersoncheck", "abort", "initiator is not a natural person");
                _logger.LogInformation("Skipping 'geopend' event for case {CaseId}: initiator is not a natural person.", caseData.Identification);
                return null;
            }
            TraceContext.Emit("naturalpersoncheck", "ok", "initiator is a natural person");

            // Compare event time with the case's LatestOpenedDate
            DateTime eventTimeUtc = cloudEvent.Time; // already in UTC
            DateTime latestOpenedUtc = caseData.LastOpenedDate.Value;

            if (eventTimeUtc >= latestOpenedUtc)
            {
                TraceContext.Emit("mijnzaken-staleness", "ok", $"event time {eventTimeUtc:O} >= laatstGeopend {latestOpenedUtc:O}");
                _logger.LogDebug("Forwarding 'geopend' for case {CaseId} (event time {EventTime} >= LatestOpenedDate {OpenDate}).",
                    caseData.Identification, eventTimeUtc, latestOpenedUtc);

                OutgoingCloudEvent outgoing = CreateOutgoingEvent(cloudEvent, latestOpenedUtc);
                return await SendAndTraceAsync(outgoing);
            }
            else
            {
                TraceContext.Emit("mijnzaken-staleness", "abort", $"event time {eventTimeUtc:O} is older than laatstGeopend {latestOpenedUtc:O}");
                _logger.LogDebug("Skipping 'geopend' for case {CaseId}: event time {EventTime} is older than LatestOpenedDate {OpenDate}.",
                    caseData.Identification, eventTimeUtc, latestOpenedUtc);
                return null;
            }
        }

        /// <summary>
        /// Handles a "zaak-gemuteerd" event – applies whitelist, notification‑expected filters,
        /// and timestamp verification using <see cref="Case.LatestMutationDate"/>.
        /// </summary>
        /// <param name="cloudEvent">The incoming CloudEvent.</param>
        /// <param name="queryContext">The shared query context for OpenZaak.</param>
        /// <param name="caseUuid">The UUID of the case.</param>
        /// <returns>The response from MijnOverheid, or <c>null</c> if filters fail, event is stale, or sending fails.</returns>
        private async Task<MijnOverheidResponse?> HandleMutatedAsync(
            IncomingCloudEvent cloudEvent,
            IQueryContext queryContext,
            Guid caseUuid)
        {
            _logger.LogDebug("Processing 'gemuteerd' event for case {Subject}.", cloudEvent.Subject);

            // Fetch case
            Uri caseUri;
            Case caseData;
            TraceContext.Emit("openzaak", "start", $"Attempting to retrieve zaak with id {caseUuid}");
            try
            {
                caseUri = new Uri($"{_configuration.ZGW.Endpoint.OpenZaak()}/zaken/{caseUuid}");
                caseData = await queryContext.GetCaseAsync(caseUri);
                TraceContext.Emit("openzaak", "ok", $"zaak with id {caseUuid} retrieved");
            }
            catch (Exception ex)
            {
                TraceContext.Emit("openzaak", "fail", ex.Message);
                _logger.LogError(ex, "Failed to fetch case {CaseUuid} for mutation event.", caseUuid);
                return null;
            }

            // Check initiator type
            TraceContext.Emit("naturalpersoncheck", "start", "Attempting to verify the initiator is a natural person");
            bool isNaturalPerson = await IsInitiatorNaturalPersonAsync(queryContext, caseUri, caseData.Identification);
            if (!isNaturalPerson)
            {
                TraceContext.Emit("naturalpersoncheck", "abort", "initiator is not a natural person");
                _logger.LogInformation("Skipping mutation event for case {CaseId}: initiator is not a natural person.", caseData.Identification);
                return null;
            }
            TraceContext.Emit("naturalpersoncheck", "ok", "initiator is a natural person");

            // Fetch status
            if (caseData.StatusUri == CommonValues.Default.Models.EmptyUri)
            {
                TraceContext.Emit("openzaak", "fail", "case has no status URI");
                _logger.LogWarning("Case {CaseId} has no status URI; cannot apply filters.", caseData.Identification);
                return null;
            }

            CaseStatus caseStatus;
            TraceContext.Emit("openzaak", "start", $"Attempting to retrieve status for case {caseData.Identification}");
            try
            {
                caseStatus = await queryContext.GetCaseStatusAsync(caseData.StatusUri);
                TraceContext.Emit("openzaak", "ok", "status retrieved");
            }
            catch (Exception ex)
            {
                TraceContext.Emit("openzaak", "fail", ex.Message);
                _logger.LogError(ex, "Failed to fetch status for case {CaseId}.", caseData.Identification);
                return null;
            }

            // Fetch status type
            CaseStatusType statusType;
            TraceContext.Emit("openzaak", "start", $"Attempting to retrieve status type for case {caseData.Identification}");
            try
            {
                statusType = await queryContext.GetCaseStatusTypeAsync(caseStatus.TypeUri);
                TraceContext.Emit("openzaak", "ok", "status type retrieved");
            }
            catch (Exception ex)
            {
                TraceContext.Emit("openzaak", "fail", ex.Message);
                _logger.LogError(ex, "Failed to fetch status type for case {CaseId}.", caseData.Identification);
                return null;
            }

            // Apply whitelist and notification-expected filters
            bool isWhitelisted = IsWhitelisted(statusType, out string scenarioName);
            TraceContext.Emit("zaaktypewhitelist", isWhitelisted ? "ok" : "abort",
                $"zaaktype {statusType.Identification} whitelisted={isWhitelisted} (scenario={scenarioName})");

            bool notificationExpected = statusType.IsNotificationExpected;
            TraceContext.Emit("informerencheck", notificationExpected ? "ok" : "abort",
                $"status type {statusType.Identification} has \"informeren\" set to {notificationExpected}");

            if (!isWhitelisted || !notificationExpected)
            {
                _logger.LogInformation(
                    "Skipping mutation event for case {CaseId}: whitelisted={Whitelisted}, notificationExpected={NotificationExpected}",
                    caseData.Identification,
                    isWhitelisted,
                    notificationExpected);
                return null;
            }

            // Timestamp verification
            DateTime eventTimeUtc = cloudEvent.Time; // already in UTC
            DateTime? latestMutationUtc = caseData.LatestMutationDate;

            if (latestMutationUtc.HasValue && eventTimeUtc < latestMutationUtc.Value)
            {
                TraceContext.Emit("mijnzaken-staleness", "abort",
                    $"event time {eventTimeUtc:O} is older than laatstGemuteerd {latestMutationUtc.Value:O}");
                _logger.LogDebug("Skipping mutation event for case {CaseId}: event time {EventTime} is older than LatestMutationDate {MutationDate}.",
                    caseData.Identification, eventTimeUtc, latestMutationUtc.Value);
                return null;
            }
            TraceContext.Emit("mijnzaken-staleness", "ok",
                latestMutationUtc.HasValue ? $"event time {eventTimeUtc:O} >= laatstGemuteerd {latestMutationUtc.Value:O}" : "laatstGemuteerd not populated by source system — check skipped");

            _logger.LogDebug("Mutation event for case {CaseId} passed filters ({Scenario}) and timestamp check. Forwarding.", caseData.Identification, scenarioName);

            OutgoingCloudEvent outgoingEvent = CreateOutgoingEvent(cloudEvent, latestMutationUtc);
            return await SendAndTraceAsync(outgoingEvent);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Parses the CloudEvent type string into a <see cref="MijnOverheidEventType"/>.
        /// </summary>
        /// <param name="type">The event type string from the CloudEvent.</param>
        /// <returns>The corresponding enum value, or <see cref="MijnOverheidEventType.Unknown"/>.</returns>
        private static MijnOverheidEventType ParseEventType(string? type)
        {
            return type switch
            {
                "nl.overheid.zaken.zaak-gemuteerd" => MijnOverheidEventType.CaseMutated,
                "nl.overheid.zaken.zaak-geopend" => MijnOverheidEventType.CaseOpened,
                "nl.overheid.zaken.zaak-verwijderd" => MijnOverheidEventType.CaseDeleted,
                _ => MijnOverheidEventType.Unknown
            };
        }

        /// <summary>
        /// Creates a new outgoing CloudEvent from the incoming one, preserving the original Time and other fields.
        /// The <c>Data</c> property is set to <c>null</c> (since externAttenderen is removed).
        /// </summary>
        /// <param name="incoming">The incoming CloudEvent (from ZhvModels).</param>
        /// <param name="actualEventTime">The actual timestamp of the underlying case event, if known.</param>
        /// <returns>A new CloudEvent instance suitable for forwarding (from WebQueries).</returns>
        private static OutgoingCloudEvent CreateOutgoingEvent(IncomingCloudEvent incoming, DateTime? actualEventTime = null)
        {
            return new OutgoingCloudEvent
            {
                SpecVersion = incoming.SpecVersion,
                Type = incoming.Type,
                Source = incoming.Source,
                Subject = incoming.Subject,
                Id = incoming.Id,
                Time = actualEventTime ?? incoming.Time,
                DataRef = incoming.DataRef ?? "",
                DataContentType = incoming.DataContentType ?? "",
                Data = null
            };
        }

        /// <summary>
        /// Determines if the status type is whitelisted for the corresponding scenario.
        /// </summary>
        /// <param name="statusType">The status type to check.</param>
        /// <param name="scenarioName">When this method returns, contains the scenario name (create, close, update).</param>
        /// <returns><c>true</c> if the status type is whitelisted; otherwise <c>false</c>.</returns>
        private bool IsWhitelisted(CaseStatusType statusType, out string scenarioName)
        {
            if (statusType.SerialNumber == 1)
            {
                scenarioName = "create";
                return _configuration.ZGW.Whitelist.ZaakCreate_IDs().IsAllowed(statusType.Identification);
            }

            if (statusType.IsFinalStatus)
            {
                scenarioName = "close";
                return _configuration.ZGW.Whitelist.ZaakClose_IDs().IsAllowed(statusType.Identification);
            }

            scenarioName = "update";
            return _configuration.ZGW.Whitelist.ZaakUpdate_IDs().IsAllowed(statusType.Identification);
        }

        /// <summary>
        /// Checks whether the initiator of the case is a natural person.
        /// </summary>
        /// <param name="queryContext">The query context.</param>
        /// <param name="caseUri">The case URI.</param>
        /// <param name="caseId">The case identification for logging.</param>
        /// <returns><c>true</c> if the initiator is a natural person; otherwise <c>false</c>.</returns>
        private async Task<bool> IsInitiatorNaturalPersonAsync(IQueryContext queryContext, Uri caseUri, string caseId)
        {
            try
            {
                return await queryContext.CheckIfInitiatorIsNaturalPersonAsync(caseUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check initiator type for case {CaseId}. Skipping.", caseId);
                return false;
            }
        }

        /// <summary>
        /// Sends the outgoing CloudEvent to Logius MijnZaken (the MijnOverheid webhook), emitting
        /// the dashboard's single trace step for this shared exit point regardless of which of the
        /// three event-type branches called it.
        /// </summary>
        private async Task<MijnOverheidResponse> SendAndTraceAsync(OutgoingCloudEvent outgoingEvent)
        {
            TraceContext.Emit("logius-mijnzaken", "start", "Attempting to forward CloudEvent to Logius MijnZaken");
            MijnOverheidResponse response = await _mijnOverheidClient.SendAsync(outgoingEvent, CancellationToken.None);
            TraceContext.Emit("logius-mijnzaken", response.IsSuccess ? "ok" : "fail",
                response.IsSuccess ? $"HTTP {response.StatusCode}" : $"HTTP {response.StatusCode}: {response.ResponseBody}");
            return response;
        }
        #endregion
    }
}