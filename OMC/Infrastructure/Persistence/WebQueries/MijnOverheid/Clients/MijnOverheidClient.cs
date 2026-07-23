using System.Net.Http.Headers;
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
    /// Handles OAuth2 token acquisition and caching.
    /// Reads configuration from environment variables.
    /// </summary>
    public class MijnOverheidClient : IMijnOverheidClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MijnOverheidClient> _logger;

        // Token caching
        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);
        private string? _cachedAccessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MijnOverheidClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory used to create the HTTP client.</param>
        /// <param name="logger">The logger used for logging.</param>
        public MijnOverheidClient(
            IHttpClientFactory httpClientFactory,
            ILogger<MijnOverheidClient> logger)
        {
            // Use a named client (recommended) – replace "MijnOverheidClient" with your actual client name if configured.
            _httpClient = httpClientFactory.CreateClient(nameof(MijnOverheidClient));
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<MijnOverheidResponse> SendAsync(
            CloudEvent cloudEvent,
            CancellationToken cancellationToken = default)
        {
            // 1. Get the webhook URL
            string webhookUrl = Environment.GetEnvironmentVariable(ConfigExtensions.MijnOverheidWebHookUrl) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogError("MijnOverheid webhook URL is not configured (env var '{VarName}').", ConfigExtensions.MijnOverheidWebHookUrl);
                return new MijnOverheidResponse
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    ResponseBody = "Webhook URL not configured"
                };
            }

            // 2. Obtain a valid access token
            string accessToken;
            try
            {
                accessToken = await GetAccessTokenAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain access token for MijnOverheid.");
                return new MijnOverheidResponse
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    ResponseBody = $"Token retrieval failed: {ex.Message}"
                };
            }

            // 3. Serialize the CloudEvent
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            string json = JsonSerializer.Serialize(cloudEvent, options);

            // 4. Build the request (per‑request headers)
            using var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/cloudevents+json");

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Request to MijnOverheid was cancelled.");
                return new MijnOverheidResponse
                {
                    IsSuccess = false,
                    StatusCode = 499,
                    ResponseBody = "Request cancelled"
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

        /// <summary>
        /// Gets a valid access token, using a cached one if it is still valid.
        /// </summary>
        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            // Quick check without locking (read‑only)
            if (_cachedAccessToken != null && _tokenExpiry > DateTime.UtcNow.AddMinutes(1))
                return _cachedAccessToken;

            await _tokenSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Double‑check after acquiring the lock
                if (_cachedAccessToken != null && _tokenExpiry > DateTime.UtcNow.AddMinutes(1))
                    return _cachedAccessToken;

                _logger.LogDebug("Fetching new access token from MijnOverheid.");

                // Read environment variables directly
                string tokenEndpoint = Environment.GetEnvironmentVariable(ConfigExtensions.MijnOverheidTokenEndpoint) ?? string.Empty;
                string clientId = Environment.GetEnvironmentVariable(ConfigExtensions.MijnOverheidClientId) ?? string.Empty;
                string clientSecret = Environment.GetEnvironmentVariable(ConfigExtensions.MijnOverheidSecret) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(tokenEndpoint) ||
                    string.IsNullOrWhiteSpace(clientId) ||
                    string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new InvalidOperationException(
                        $"MijnOverheid OAuth2 configuration missing: check {ConfigExtensions.MijnOverheidTokenEndpoint}, " +
                        $"{ConfigExtensions.MijnOverheidClientId}, {ConfigExtensions.MijnOverheidSecret}.");
                }

                // Build the token request
                using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
                // HTTP Basic Authentication
                string credentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                request.Content = new FormUrlEncodedContent(
                    new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Token request failed. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseBody);
                    throw new HttpRequestException($"Token request failed: {response.StatusCode} - {responseBody}");
                }

                // Parse JSON response
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;
                if (!root.TryGetProperty("access_token", out JsonElement accessTokenElement) ||
                    !root.TryGetProperty("expires_in", out JsonElement expiresInElement))
                {
                    throw new InvalidOperationException("Token response missing access_token or expires_in.");
                }

                string newToken = accessTokenElement.GetString() ?? throw new InvalidOperationException("access_token is null.");
                int expiresIn = expiresInElement.GetInt32();

                // Cache the token
                _cachedAccessToken = newToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);

                _logger.LogDebug("New access token obtained, expires in {ExpiresIn} seconds.", expiresIn);

                return newToken;
            }
            finally
            {
                _tokenSemaphore.Release();
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