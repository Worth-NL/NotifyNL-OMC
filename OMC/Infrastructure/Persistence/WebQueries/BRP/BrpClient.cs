using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.Settings.Extensions;
using Microsoft.Extensions.Logging;

namespace WebQueries.BRP
{
    /// <summary>
    /// Client for querying the BRP (Basisregistratie Personen) service.
    /// Handles authentication via Keycloak and exchanges tokens for BRP access.
    /// </summary>
    public class BrpClient
    {
        private readonly HttpClient _http;
        private readonly KeycloakTokenService _tokens;
        private readonly ILogger<BrpClient> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="BrpClient"/>.
        /// </summary>
        /// <param name="http">
        /// The <see cref="HttpClient"/> used to send requests to the BRP API.
        /// This client is expected to be configured for the WS Gateway environment.
        /// </param>
        /// <param name="tokens">
        /// The <see cref="KeycloakTokenService"/> responsible for obtaining and
        /// exchanging OAuth access tokens required for BRP access.
        /// </param>
        /// <param name="logger">
        /// The logger used to emit highly verbose logs intended for
        /// remote debugging via GitOps logs.
        /// </param>
        public BrpClient(
            HttpClient http,
            KeycloakTokenService tokens,
            ILogger<BrpClient> logger)
        {
            _http = http;
            _tokens = tokens;
            _logger = logger;
        }

        /// <summary>
        /// Queries the BRP Personen API (v2.0) for a person's data using their BSN.
        /// </summary>
        /// <remarks>
        /// This method performs multiple externally dependent steps:
        /// <list type="number">
        /// <item>Obtain a service account token from Keycloak</item>
        /// <item>Exchange the service token for a BRP-scoped token</item>
        /// <item>Call the BRP Personen API via the WS Gateway</item>
        /// </list>
        /// Each step is logged in detail to support environments where
        /// interactive debugging is not possible.
        /// </remarks>
        /// <param name="bsn">
        /// The BSN (Burger Service Nummer) of the person to query.
        /// The BSN value itself is never logged; only its length is recorded.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the raw JSON response returned by the BRP API.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when any HTTP request (Keycloak or BRP) fails or returns
        /// a non-success status code.
        /// </exception>
        public async Task<string> QueryPersonAsync(string bsn)
        {
            string correlationId = Guid.NewGuid().ToString();

            string? brpBaseUrl =
                Environment.GetEnvironmentVariable(ConfigExtensions.BrpBaseUrl);

            _logger.LogInformation(
                "BrpClient.QueryPerson.Start CorrelationId={CorrelationId} BsnLength={BsnLength} BrpBaseUrlPresent={BrpBaseUrlPresent}",
                correlationId,
                bsn.Length,
                !string.IsNullOrWhiteSpace(brpBaseUrl)
            );

            try
            {
                // Step 1: obtain service token from Keycloak
                _logger.LogInformation(
                    "BrpClient.GetServiceToken.Start CorrelationId={CorrelationId}",
                    correlationId
                );

                string serviceToken = await _tokens.GetServiceTokenAsync();

                _logger.LogInformation(
                    "BrpClient.GetServiceToken.Success CorrelationId={CorrelationId} TokenLength={TokenLength}",
                    correlationId,
                    serviceToken.Length
                );

                // Step 2: exchange token for BRP audience
                _logger.LogInformation(
                    "BrpClient.ExchangeToken.Start CorrelationId={CorrelationId}",
                    correlationId
                );

                string brpToken = await _tokens.ExchangeForBrpTokenAsync(serviceToken);

                _logger.LogInformation(
                    "BrpClient.ExchangeToken.Success CorrelationId={CorrelationId} TokenLength={TokenLength}",
                    correlationId,
                    brpToken.Length
                );

                // Step 3: call BRP v2.0 via WS Gateway
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", brpToken);

                var requestBody = new
                {
                    type = "RaadpleegMetBurgerservicenummer",
                    burgerservicenummer = new[] { bsn },
                    fields = new[]
                    {
                        "burgerservicenummer",
                        "naam",
                        "geboorte"
                    }
                };

                string json = JsonSerializer.Serialize(requestBody);

                _logger.LogDebug(
                    "BrpClient.HttpPost.Request CorrelationId={CorrelationId} Url={Url} BodyLength={BodyLength}",
                    correlationId,
                    $"{brpBaseUrl}/brp/personen",
                    json.Length
                );

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                string url = $"{brpBaseUrl}/brp/personen";

                HttpResponseMessage response = await _http.PostAsync(url, content);

                _logger.LogInformation(
                    "BrpClient.HttpPost.Response CorrelationId={CorrelationId} StatusCode={StatusCode}",
                    correlationId,
                    (int)response.StatusCode
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "BrpClient.QueryPerson.Success CorrelationId={CorrelationId} ResponseLength={ResponseLength}",
                    correlationId,
                    responseBody.Length
                );

                return responseBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "BrpClient.QueryPerson.Failure CorrelationId={CorrelationId}",
                    correlationId
                );
                throw;
            }
        }
    }
}
