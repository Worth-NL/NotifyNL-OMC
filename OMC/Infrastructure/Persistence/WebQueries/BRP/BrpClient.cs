// © 2026, Worth Systems.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.Properties;
using Common.Settings.Extensions;
using Microsoft.Extensions.Logging;

namespace WebQueries.BRP
{
    /// <summary>
    /// Client for querying the BRP (Basisregistratie Personen) service.
    /// Handles authentication via Keycloak and exchanges tokens for BRP access.
    /// </summary>
    public sealed class BrpClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakTokenService _keycloakTokenService;
        private readonly ILogger<BrpClient> _logger;
        private readonly string _brpBaseUrl;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of <see cref="BrpClient"/>.
        /// </summary>
        /// <param name="httpClient">
        /// The <see cref="HttpClient"/> used to send requests to the BRP API.
        /// This client is expected to be configured for the WS Gateway environment.
        /// </param>
        /// <param name="keycloakTokenService">
        /// The <see cref="KeycloakTokenService"/> responsible for obtaining and
        /// exchanging OAuth access tokens required for BRP access.
        /// </param>
        /// <param name="logger">
        /// The logger used to emit highly verbose logs intended for
        /// remote debugging via GitOps logs.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when required configuration is missing.
        /// </exception>
        public BrpClient(
            HttpClient httpClient,
            KeycloakTokenService keycloakTokenService,
            ILogger<BrpClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _keycloakTokenService = keycloakTokenService ?? throw new ArgumentNullException(nameof(keycloakTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Validate and set BRP base URL
            _brpBaseUrl = ValidateAndGetBrpBaseUrl();

            // Single validation at construction
            ValidateCertificateConfiguration();

            _logger.LogInformation("BRP Client initialized for {BaseUrl}", _brpBaseUrl);
        }

        /// <summary>
        /// Validates and retrieves the BRP base URL from environment variables.
        /// </summary>
        /// <returns>The validated BRP base URL.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when BRP_BASE_URL environment variable is missing or empty.
        /// </exception>
        private string ValidateAndGetBrpBaseUrl()
        {
            string? brpBaseUrl = Environment.GetEnvironmentVariable(ConfigExtensions.BrpBaseUrl);

            if (string.IsNullOrWhiteSpace(brpBaseUrl))
            {
                _logger.LogCritical(
                    "BRP_BASE_URL environment variable is not set. " +
                    "Please configure BRP_BASE_URL with the WS Gateway endpoint.");

                throw new InvalidOperationException(
                    "BRP_BASE_URL environment variable is required.");
            }

            // Validate URL format
            if (!Uri.TryCreate(brpBaseUrl, UriKind.Absolute, out Uri? uri) ||
                !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("BRP_BASE_URL may be invalid: {Url}. Ensure it's a valid HTTPS URL.", brpBaseUrl);
            }

            return brpBaseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Validates certificate configuration once at construction.
        /// </summary>
        private void ValidateCertificateConfiguration()
        {
            try
            {
                _logger.LogDebug("Checking certificate configuration...");

                // Check environment variables
                string? certPath = Environment.GetEnvironmentVariable("BRP_CLIENTCERT_PEM_PATH");
                string? keyPath = Environment.GetEnvironmentVariable("BRP_CLIENTKEY_PEM_PATH");

                bool envVarsSet = !string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(keyPath);

                if (!envVarsSet)
                {
                    _logger.LogInformation(
                        "BRP certificate environment variables not set. " +
                        "BRP API functionality will be unavailable.");
                    return; // Early exit - no need to check further
                }

                // Check file existence
                bool certExists = File.Exists(certPath);
                bool keyExists = File.Exists(keyPath);

                if (!certExists || !keyExists)
                {
                    _logger.LogWarning(
                        "Certificate files not found. Cert: {CertExists}, Key: {KeyExists}",
                        certExists, keyExists);
                    _logger.LogInformation("BRP API functionality will be unavailable.");
                    return;
                }

                // Verify certificates in HttpClientHandler
                VerifyHttpClientCertificates();
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Certificate check completed with warnings");
                // Don't throw - BRP is optional
            }
        }

        private void VerifyHttpClientCertificates()
        {
            HttpClientHandler? handler = GetHttpClientHandler();
            if (handler == null)
            {
                _logger.LogDebug("Could not access HttpClientHandler");
                return;
            }

            if (handler.ClientCertificates.Count == 0)
            {
                _logger.LogInformation(
                    "No client certificates found. " +
                    "BRP API calls will fail with mTLS authentication errors.");
                return;
            }

            _logger.LogDebug("Found {Count} certificate(s) for BRP API", handler.ClientCertificates.Count);
        }

        /// <summary>
        /// Queries the BRP Personen API (v2.0) for a person's data using their BSN.
        /// </summary>
        /// <param name="bsn">
        /// The BSN (Burger Service Nummer) of the person to query.
        /// The BSN value itself is never logged; only its length is recorded.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the raw JSON response returned by the BRP API.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bsn"/> is null or empty.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when any HTTP request (Keycloak or BRP) fails or returns
        /// a non-success status code.
        /// </exception>
        public async Task<string> QueryPersonAsync(string bsn)
        {
            if (string.IsNullOrWhiteSpace(bsn))
            {
                throw new ArgumentNullException(nameof(bsn), CommonResources.BrpClient_QueryPersonAsync_BSN_cannot_be_null_or_empty);
            }

            var correlationId = Guid.NewGuid();

            using IDisposable? loggingScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId.ToString(),
                ["BsnLength"] = bsn.Length,
                ["Operation"] = "QueryPerson"
            });

            try
            {
                _logger.LogInformation("Querying BRP for BSN (length: {BsnLength})", bsn.Length);

                // Step 1: Get service token
                string serviceToken = await _keycloakTokenService.GetServiceTokenAsync();
                _logger.LogDebug("Service token obtained ({Length} chars)", serviceToken.Length);

                // Step 2: Exchange for BRP token
                string brpToken = await _keycloakTokenService.ExchangeForBrpTokenAsync(serviceToken);
                _logger.LogDebug("BRP token obtained ({Length} chars)", brpToken.Length);

                // Validate BRP token audience (critical!)
                ValidateBrpTokenAudience(brpToken);

                // Step 3: Call BRP API
                string result = await CallBrpApiAsync(brpToken, bsn);

                _logger.LogInformation("BRP query successful (response: {Length} chars)", result.Length);
                return result;
            }
            catch (HttpRequestException httpEx) when (IsCertificateError(httpEx))
            {
                _logger.LogCritical(httpEx,
                    "Certificate/TLS error detected. Check mTLS configuration. Status: {StatusCode}",
                    httpEx.StatusCode);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "BRP query failed");
                throw;
            }
        }

        /// <summary>
        /// Validates that the BRP token contains the required audience.
        /// </summary>
        private void ValidateBrpTokenAudience(string token)
        {
            try
            {
                string[] parts = token.Split('.');
                if (parts.Length != 3) return; // Not a JWT

                string payload = parts[1];
                int padding = 4 - (payload.Length % 4);
                if (padding < 4)
                {
                    payload = payload.PadRight(payload.Length + padding, '=');
                }

                byte[] payloadBytes = Convert.FromBase64String(payload);
                string payloadJson = Encoding.UTF8.GetString(payloadBytes);

                using var document = JsonDocument.Parse(payloadJson);

                if (document.RootElement.TryGetProperty("aud", out JsonElement audElement))
                {
                    string audience = audElement.ToString();
                    if (audience.Contains("haalcentraal", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("BRP token has correct audience: {Audience}", audience);
                    }
                    else
                    {
                        _logger.LogWarning("BRP token missing 'haalcentraal' audience. Actual: {Audience}", audience);
                    }
                }
                else
                {
                    _logger.LogWarning("BRP token has no audience claim");
                }
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Failed to validate BRP token audience");
            }
        }

        /// <summary>
        /// Determines if an exception is related to certificate/TLS issues.
        /// </summary>
        private static bool IsCertificateError(HttpRequestException exception)
        {
            string message = exception.Message;
            return message.Contains("certificate", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("TLS", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("SSL", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("handshake", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Calls the BRP API with the given token and BSN.
        /// </summary>
        private async Task<string> CallBrpApiAsync(string brpToken, string bsn)
        {
            string endpoint = $"{_brpBaseUrl}/brp/personen";
            _logger.LogDebug("Using endpoint: {Endpoint}", endpoint);

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            // Add headers
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", brpToken);
            request.Headers.Add("x-api-version", "2.0");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());

            // Prepare request body
            var requestBody = new
            {
                type = "RaadpleegMetBurgerservicenummer",
                burgerservicenummer = new[] { bsn },
                fields = new[] { "burgerservicenummer", "naam", "adressering", "geslacht", "adresseringBinnenland" }
            };

            string json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending request to BRP API (body: {Length} chars)", json.Length);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            _logger.LogInformation("BRP API response: {StatusCode}", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                throw await CreateHttpExceptionAsync(response);
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("BRP API response successful ({Length} chars)", responseBody.Length);

            return responseBody;
        }

        /// <summary>
        /// Creates a detailed HttpRequestException from a failed response.
        /// </summary>
        private async Task<HttpRequestException> CreateHttpExceptionAsync(HttpResponseMessage response)
        {
            System.Net.HttpStatusCode statusCode = response.StatusCode;
            string statusText = $"{(int)statusCode} ({statusCode})";
            string errorBody = await response.Content.ReadAsStringAsync();

            string errorMessage = $"BRP API error {statusText}";

            // Add response body if available
            if (!string.IsNullOrEmpty(errorBody))
            {
                if (errorBody.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using var errorJson = JsonDocument.Parse(errorBody);
                        errorMessage += $": {errorJson.RootElement}";
                    }
                    catch (JsonException)
                    {
                        errorMessage += $". Response: {errorBody}";
                    }
                }
                else
                {
                    errorMessage += $". Response: {errorBody}";
                }
            }

            // Check for certificate errors
            if (errorBody.Contains("certificate", StringComparison.OrdinalIgnoreCase) ||
                errorBody.Contains("TLS", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage += " (Possible certificate/TLS issue)";
                _logger.LogError("Response indicates certificate/TLS error");
            }

            _logger.LogError("BRP API error: {ErrorMessage}", errorMessage);

            return new HttpRequestException(errorMessage, null, statusCode);
        }

        /// <summary>
        /// Gets the HttpClientHandler via reflection.
        /// </summary>
        private HttpClientHandler? GetHttpClientHandler()
        {
            try
            {
                System.Reflection.FieldInfo? handlerField = typeof(HttpMessageInvoker).GetField(
                    "_handler",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                return handlerField?.GetValue(_httpClient) as HttpClientHandler;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Disposes the resources used by this <see cref="BrpClient"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _httpClient.Dispose();
            _disposed = true;
        }
    }
}