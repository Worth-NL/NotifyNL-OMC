// © 2026, Worth Systems.

using System.Text.Json;
using Common.Settings.Extensions;
using Microsoft.Extensions.Logging;

namespace WebQueries.BRP
{
    /// <summary>
    /// Service for obtaining and exchanging Keycloak access tokens using
    /// client credentials and token exchange OAuth flows.
    /// </summary>
    public class KeycloakTokenService
    {
        private readonly HttpClient _http;
        private readonly ILogger<KeycloakTokenService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeycloakTokenService"/> class.
        /// </summary>
        /// <param name="http">
        /// The <see cref="HttpClient"/> used to communicate with the Keycloak authorization server.
        /// This client is expected to be preconfigured (e.g. base settings, handlers, timeouts).
        /// </param>
        /// <param name="logger">
        /// The logger used to emit highly verbose, correlation-based logs intended for
        /// remote debugging scenarios where interactive debugging is not possible.
        /// </param>
        public KeycloakTokenService(
            HttpClient http,
            ILogger<KeycloakTokenService> logger)
        {
            _http = http;
            _logger = logger;
        }

        /// <summary>
        /// Requests a service account access token from Keycloak using the
        /// OAuth 2.0 client credentials grant.
        /// </summary>
        /// <remarks>
        /// This method is intentionally heavily logged to support debugging via logs only
        /// in restricted client environments (e.g. behind gateways or key vaults).
        /// Sensitive values such as secrets and tokens are never logged directly.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the access token issued by Keycloak.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Keycloak response does not contain an <c>access_token</c>.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP request to Keycloak fails or returns a non-success status code.
        /// </exception>
        public async Task<string> GetServiceTokenAsync()
        {
            string correlationId = Guid.NewGuid().ToString();

            string? authServerUrl =
                Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakAuthServerUrl);
            string? clientId =
                Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientId);
            string? clientSecret =
                Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientSecret);

            _logger.LogInformation(
                "GetServiceToken.Start CorrelationId={CorrelationId} AuthServerUrlPresent={AuthServerUrlPresent} ClientIdPresent={ClientIdPresent} ClientSecretPresent={ClientSecretPresent}",
                correlationId,
                !string.IsNullOrWhiteSpace(authServerUrl),
                !string.IsNullOrWhiteSpace(clientId),
                !string.IsNullOrWhiteSpace(clientSecret)
            );

            string url = $"{authServerUrl}/token";

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!
            };

            try
            {
                _logger.LogDebug(
                    "GetServiceToken.HttpPost.Start CorrelationId={CorrelationId} Url={Url} FormKeys={FormKeys}",
                    correlationId,
                    url,
                    string.Join(",", form.Keys)
                );

                HttpResponseMessage response =
                    await _http.PostAsync(url, new FormUrlEncodedContent(form));

                _logger.LogInformation(
                    "GetServiceToken.HttpPost.Response CorrelationId={CorrelationId} StatusCode={StatusCode}",
                    correlationId,
                    (int)response.StatusCode
                );

                response.EnsureSuccessStatusCode();

                string rawResponse = await response.Content.ReadAsStringAsync();

                _logger.LogDebug(
                    "GetServiceToken.HttpPost.Body CorrelationId={CorrelationId} ResponseLength={Length}",
                    correlationId,
                    rawResponse.Length
                );

                JsonElement json = JsonDocument.Parse(rawResponse).RootElement;

                bool hasAccessToken = json.TryGetProperty("access_token", out JsonElement tokenElement);

                _logger.LogInformation(
                    "GetServiceToken.ParseResult CorrelationId={CorrelationId} HasAccessToken={HasAccessToken}",
                    correlationId,
                    hasAccessToken
                );

                if (!hasAccessToken)
                {
                    _logger.LogError(
                        "GetServiceToken.MissingAccessToken CorrelationId={CorrelationId} Json={Json}",
                        correlationId,
                        json.ToString()
                    );

                    throw new InvalidOperationException("Keycloak response did not contain access_token");
                }

                string token = tokenElement.GetString()!;

                _logger.LogInformation(
                    "GetServiceToken.Success CorrelationId={CorrelationId} TokenLength={TokenLength}",
                    correlationId,
                    token.Length
                );

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GetServiceToken.Failure CorrelationId={CorrelationId}",
                    correlationId
                );
                throw;
            }
        }

        /// <summary>
        /// Exchanges an existing access token for a new access token targeted
        /// at the BRP system using Keycloak's OAuth 2.0 token exchange flow.
        /// </summary>
        /// <param name="subjectToken">
        /// The original access token that will be exchanged.
        /// The token value itself is never logged; only its length is recorded.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the exchanged access token for the configured audience.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Keycloak response does not contain an <c>access_token</c>.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP request to Keycloak fails or returns a non-success status code.
        /// </exception>
        public async Task<string> ExchangeForBrpTokenAsync(string subjectToken)
        {
            string correlationId = Guid.NewGuid().ToString();

            string? authServerUrl =
                Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakAuthServerUrl);
            string? clientId =
                Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientId);
            string? clientSecret =
                Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientSecret);
            string? audience =
                Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakTokenExchangeAudience);

            _logger.LogInformation(
                "ExchangeForBrpToken.Start CorrelationId={CorrelationId} SubjectTokenLength={SubjectTokenLength} AuthServerUrlPresent={AuthServerUrlPresent} AudiencePresent={AudiencePresent}",
                correlationId,
                subjectToken.Length,
                !string.IsNullOrWhiteSpace(authServerUrl),
                !string.IsNullOrWhiteSpace(audience)
            );

            string url = $"{authServerUrl}/token";

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["subject_token"] = subjectToken,
                ["requested_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                ["audience"] = audience!
            };

            try
            {
                _logger.LogDebug(
                    "ExchangeForBrpToken.HttpPost.Start CorrelationId={CorrelationId} Url={Url} FormKeys={FormKeys}",
                    correlationId,
                    url,
                    string.Join(",", form.Keys)
                );

                HttpResponseMessage response =
                    await _http.PostAsync(url, new FormUrlEncodedContent(form));

                _logger.LogInformation(
                    "ExchangeForBrpToken.HttpPost.Response CorrelationId={CorrelationId} StatusCode={StatusCode}",
                    correlationId,
                    (int)response.StatusCode
                );

                response.EnsureSuccessStatusCode();

                string rawResponse = await response.Content.ReadAsStringAsync();

                _logger.LogDebug(
                    "ExchangeForBrpToken.HttpPost.Body CorrelationId={CorrelationId} ResponseLength={Length}",
                    correlationId,
                    rawResponse.Length
                );

                JsonElement json = JsonDocument.Parse(rawResponse).RootElement;

                bool hasAccessToken = json.TryGetProperty("access_token", out JsonElement tokenElement);

                _logger.LogInformation(
                    "ExchangeForBrpToken.ParseResult CorrelationId={CorrelationId} HasAccessToken={HasAccessToken}",
                    correlationId,
                    hasAccessToken
                );

                if (!hasAccessToken)
                {
                    _logger.LogError(
                        "ExchangeForBrpToken.MissingAccessToken CorrelationId={CorrelationId} Json={Json}",
                        correlationId,
                        json.ToString()
                    );

                    throw new InvalidOperationException("Keycloak token exchange response did not contain access_token");
                }

                string token = tokenElement.GetString()!;

                _logger.LogInformation(
                    "ExchangeForBrpToken.Success CorrelationId={CorrelationId} TokenLength={TokenLength}",
                    correlationId,
                    token.Length
                );

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ExchangeForBrpToken.Failure CorrelationId={CorrelationId}",
                    correlationId
                );
                throw;
            }
        }
    }
}
