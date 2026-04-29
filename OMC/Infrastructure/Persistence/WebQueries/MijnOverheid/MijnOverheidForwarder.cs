using Common.Constants;
using Common.Settings.Configuration;
using Microsoft.Extensions.Logging;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.MijnOverheid.Interfaces;
using WebQueries.MijnOverheid.Models;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.MijnOverheid
{
    /// <summary>
    /// 
    /// </summary>
    public class MijnOverheidForwarder : IMijnOverheidForwarder
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IMijnOverheidClient _mijnOverheidClient;
        private readonly OmcConfiguration _configuration;
        private readonly ILogger<MijnOverheidForwarder> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataQuery"></param>
        /// <param name="mijnOverheidClient"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
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
        /// 
        /// </summary>
        /// <param name="cloudEvent"></param>
        /// <returns></returns>
        public async Task<MijnOverheidResponse?> ForwardIfNeededAsync(CloudEvent cloudEvent)
        {
            // Step 1: Validate CloudEvent and its Subject
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

            string caseUrl = $"{_configuration.ZGW.Endpoint.OpenZaak()}/zaken/{caseUuid}";
            var caseUri = new Uri(caseUrl);

            // Dummy notification to get query context
            var dummyNotification = new NotificationEvent
            {
                MainObjectUri = caseUri,
                Channel = Channels.Cases,
                Resource = Resources.Case,
                Action = Actions.Update
            };
            IQueryContext queryContext = _dataQuery.From(dummyNotification);

            Case caseData;
            try
            {
                caseData = await queryContext.GetCaseAsync(caseUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Case not found for UUID {CaseUuid}", caseUuid);
                return null;
            }

            // Fetch current status from case's StatusUri
            if (caseData.StatusUri == CommonValues.Default.Models.EmptyUri)
            {
                _logger.LogWarning("Case {CaseId} has no status URI, cannot determine scenario", caseData.Identification);
                return null;
            }

            CaseStatus caseStatus;
            try
            {
                caseStatus = await queryContext.GetCaseStatusAsync(caseData.StatusUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch status for case {CaseId}", caseData.Identification);
                return null;
            }

            CaseStatusType statusType;
            try
            {
                statusType = await queryContext.GetCaseStatusTypeAsync(caseStatus.TypeUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch status type for case {CaseId}", caseData.Identification);
                return null;
            }

            // Determine which whitelist to use
            bool isWhitelisted;
            string scenarioName;
            if (statusType.SerialNumber == 1)
            {
                isWhitelisted = _configuration.ZGW.Whitelist.ZaakCreate_IDs().IsAllowed(statusType.Identification);
                scenarioName = "create";
            }
            else if (statusType.IsFinalStatus)
            {
                isWhitelisted = _configuration.ZGW.Whitelist.ZaakClose_IDs().IsAllowed(statusType.Identification);
                scenarioName = "close";
            }
            else
            {
                isWhitelisted = _configuration.ZGW.Whitelist.ZaakUpdate_IDs().IsAllowed(statusType.Identification);
                scenarioName = "update";
            }

            if (!isWhitelisted)
            {
                _logger.LogInformation("Status type {StatusTypeId} not whitelisted for {Scenario}, skipping forward", statusType.Identification, scenarioName);
                return null;
            }

            if (!statusType.IsNotificationExpected)
            {
                _logger.LogInformation("Notification not expected for status type {StatusTypeId}, skipping forward", statusType.Identification);
                return null;
            }

            // All checks passed – forward the original CloudEvent
            // Prepare the outgoing CloudEvent by cloning the incoming one and modifying Time and Data
            DateTimeOffset eventTime = caseData.LatestMutationDate.HasValue
                ? new DateTimeOffset(caseData.LatestMutationDate.Value, TimeSpan.Zero)
                : DateTimeOffset.UtcNow;

            var outgoingEvent = new CloudEvent
            {
                SpecVersion = cloudEvent.SpecVersion,
                Type = cloudEvent.Type,
                Source = cloudEvent.Source,
                Subject = cloudEvent.Subject,
                Id = cloudEvent.Id,           // keep original ID (or generate new? requirement unclear – I keep original)
                Time = eventTime,
                DataRef = cloudEvent.DataRef,
                DataContentType = cloudEvent.DataContentType,
                Data = null                    // always null
            };

            _logger.LogDebug("Forwarding {EventType} for case {CaseId} as {Scenario}", cloudEvent.Type, caseData.Identification, scenarioName);
            return await _mijnOverheidClient.SendAsync(outgoingEvent);
        }
    }
}