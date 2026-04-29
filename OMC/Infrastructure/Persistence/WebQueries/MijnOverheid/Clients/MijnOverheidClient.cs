using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using WebQueries.MijnOverheid.Interfaces;
using WebQueries.MijnOverheid.Models;
using Common.Settings.Extensions;

namespace WebQueries.MijnOverheid.Clients
{
    /// <summary>
    /// Client for sending CloudEvents to MijnOverheid.
    /// Reads the webhook URL from environment variable using <see cref="ConfigExtensions.MijnOverheidWebHookUrl"/>
    /// and uses <see cref="IHttpClientFactory"/>.
    /// </summary>
    public class MijnOverheidClient : IMijnOverheidClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MijnOverheidClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MijnOverheidClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger.</param>
        public MijnOverheidClient(
            IHttpClientFactory httpClientFactory,
            ILogger<MijnOverheidClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(MijnOverheidClient));
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<MijnOverheidResponse> SendAsync(CloudEvent cloudEvent)
        {
            string webhookUrl = Environment.GetEnvironmentVariable(ConfigExtensions.MijnOverheidWebHookUrl) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogError("MijnOverheid webhook URL is not configured (environment variable '{VarName}').", ConfigExtensions.MijnOverheidWebHookUrl);
                return new MijnOverheidResponse
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    ResponseBody = "Webhook URL not configured"
                };
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            string json = JsonSerializer.Serialize(cloudEvent, options);
            using var content = new StringContent(json, Encoding.UTF8, "application/cloudevents+json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(webhookUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("CloudEvent {EventType} sent to MijnOverheid for subject {Subject}",
                        cloudEvent.Type, cloudEvent.Subject);
                }
                else
                {
                    _logger.LogWarning("Failed to send CloudEvent {EventType} to MijnOverheid. Status: {StatusCode}, Response: {ErrorBody}",
                        cloudEvent.Type, response.StatusCode, responseBody);
                }

                return new MijnOverheidResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    ResponseBody = responseBody
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending CloudEvent {EventType} to MijnOverheid", cloudEvent.Type);
                return new MijnOverheidResponse
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    ResponseBody = ex.Message
                };
            }
        }

        #region Static factory methods

        /// <summary>
        /// Creates a "zaak-gemuteerd" (case updated) event.
        /// </summary>
        /// <param name="sourceUrn">URN identifying the responsible organisation, e.g., "urn:nld:rsin:002564440:zakensysteem".</param>
        /// <param name="zaakUuid">The unique UUID of the case (subject).</param>
        /// <param name="eventId">Unique UUID for this event (used for deduplication).</param>
        /// <param name="time">Timestamp when the event occurred (UTC).</param>
        /// <param name="dataref">Relative URL to the case details endpoint.</param>
        /// <param name="externAttenderen">If true, MijnOverheid may send an email notification to the citizen (subject to user preferences).</param>
        /// <returns>A fully populated CloudEvent record.</returns>
        public static CloudEvent CreateZaakGemuteerdEvent(
            string sourceUrn,
            string zaakUuid,
            string eventId,
            DateTimeOffset time,
            string dataref,
            bool externAttenderen)
        {
            return new CloudEvent
            {
                SpecVersion = "1.0",
                Type = "nl.overheid.zaken.zaak-gemuteerd",
                Source = sourceUrn,
                Subject = zaakUuid,
                Id = eventId,
                Time = time,
                DataRef = dataref,
                DataContentType = "application/json",
                Data = null
            };
        }

        /// <summary>
        /// Creates a "zaak-verwijderd" (case deleted) event.
        /// </summary>
        /// <param name="sourceUrn">URN identifying the responsible organisation.</param>
        /// <param name="zaakUuid">The unique UUID of the case (subject).</param>
        /// <param name="eventId">Unique UUID for this event.</param>
        /// <param name="time">Timestamp when the deletion occurred (UTC).</param>
        /// <param name="dataref">Relative URL to the case details (no longer valid).</param>
        /// <returns>A fully populated CloudEvent record with data = null.</returns>
        public static CloudEvent CreateZaakVerwijderdEvent(
            string sourceUrn,
            string zaakUuid,
            string eventId,
            DateTimeOffset time,
            string dataref)
        {
            return new CloudEvent
            {
                SpecVersion = "1.0",
                Type = "nl.overheid.zaken.zaak-verwijderd",
                Source = sourceUrn,
                Subject = zaakUuid,
                Id = eventId,
                Time = time,
                DataRef = dataref,
                DataContentType = "application/json",
                Data = null
            };
        }

        /// <summary>
        /// Creates a "zaak-geopend" (case opened) event.
        /// </summary>
        /// <param name="sourceUrn">URN identifying the responsible organisation.</param>
        /// <param name="zaakUuid">The unique UUID of the case (subject).</param>
        /// <param name="eventId">Unique UUID for this event.</param>
        /// <param name="time">Timestamp when the case was opened (UTC).</param>
        /// <param name="dataref">Relative URL to the case details endpoint.</param>
        /// <returns>A fully populated CloudEvent record with data = null.</returns>
        public static CloudEvent CreateZaakGeopendEvent(
            string sourceUrn,
            string zaakUuid,
            string eventId,
            DateTimeOffset time,
            string dataref)
        {
            return new CloudEvent
            {
                SpecVersion = "1.0",
                Type = "nl.overheid.zaken.zaak-geopend",
                Source = sourceUrn,
                Subject = zaakUuid,
                Id = eventId,
                Time = time,
                DataRef = dataref,
                DataContentType = "application/json",
                Data = null
            };
        }

        #endregion
    }
}