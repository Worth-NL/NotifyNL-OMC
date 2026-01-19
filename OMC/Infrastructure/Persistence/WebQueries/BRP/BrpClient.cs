// © 2026, Worth Systems.

using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
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
        public BrpClient(
            HttpClient httpClient,
            KeycloakTokenService keycloakTokenService,
            ILogger<BrpClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _keycloakTokenService = keycloakTokenService ?? throw new ArgumentNullException(nameof(keycloakTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brpBaseUrl = Environment.GetEnvironmentVariable(ConfigExtensions.BrpBaseUrl)
                ?? "https://wsgateway.ot.denhaag.nl/haalcentraal/api";

            LogHttpClientConfiguration();

            // ONLY verify the certificate is present and valid - don't load it again
            VerifyCertificateConfiguration();
        }

        /// <summary>
        /// Verifies that the certificate configured in program.cs is present and valid.
        /// </summary>
        private void VerifyCertificateConfiguration()
        {
            try
            {
                _logger.LogInformation("=== Certificate Verification ===");

                // 1. Check environment variables are set
                string? certPath = Environment.GetEnvironmentVariable("BRP_CLIENTCERT_PEM_PATH");
                string? keyPath = Environment.GetEnvironmentVariable("BRP_CLIENTKEY_PEM_PATH");

                if (string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(keyPath))
                {
                    _logger.LogError(
                        "⚠️ BRP certificate environment variables not set! " +
                        "BRP_CLIENTCERT_PEM_PATH: {CertPath}, BRP_CLIENTKEY_PEM_PATH: {KeyPath}",
                        certPath ?? "NOT SET", keyPath ?? "NOT SET");

                    // Don't throw here - program.cs might have loaded it differently
                    _logger.LogWarning("Certificate may be loaded via different mechanism");
                }
                else
                {
                    // 2. Check if certificate files exist
                    bool certExists = File.Exists(certPath);
                    bool keyExists = File.Exists(keyPath);

                    if (!certExists || !keyExists)
                    {
                        _logger.LogError(
                            "⚠️ Certificate files not found! " +
                            "Cert exists: {CertExists}, Key exists: {KeyExists}",
                            certExists, keyExists);
                    }
                    else
                    {
                        _logger.LogInformation("✓ Certificate environment variables and files are OK");
                    }
                }

                // 3. Check HttpClientHandler for certificates
                HttpClientHandler? handler = GetHttpClientHandler();
                if (handler == null)
                {
                    _logger.LogWarning("Could not access HttpClientHandler for verification");
                }
                else if (handler.ClientCertificates.Count == 0)
                {
                    _logger.LogError("⚠️ NO CLIENT CERTIFICATES FOUND in HttpClientHandler!");
                    _logger.LogError("This will cause mTLS authentication failures with WS Gateway");
                }
                else
                {
                    _logger.LogInformation("✓ Found {CertificateCount} certificate(s) in HttpClientHandler",
                        handler.ClientCertificates.Count);

                    // Log details of each certificate
                    foreach (X509Certificate x509Certificate in handler.ClientCertificates)
                    {
                        if (x509Certificate is X509Certificate2 certificate)
                        {
                            LogCertificateVerification(certificate);
                        }
                    }
                }

                _logger.LogInformation("=== End Certificate Verification ===");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error during certificate verification");
                // Don't throw - we'll let the actual API call fail with meaningful error
            }
        }

        /// <summary>
        /// Logs verification details for a certificate.
        /// </summary>
        private void LogCertificateVerification(X509Certificate2 certificate)
        {
            try
            {
                _logger.LogInformation("Certificate Details:");
                _logger.LogInformation("  Subject: {Subject}", certificate.Subject);
                _logger.LogInformation("  Issuer: {Issuer}", certificate.Issuer);

                // Critical checks
                bool hasPrivateKey = certificate.HasPrivateKey;
                bool isNotExpired = DateTime.Now <= certificate.NotAfter;
                bool isNotBefore = DateTime.Now >= certificate.NotBefore;

                _logger.LogInformation("  HasPrivateKey: {HasPrivateKey} {Status}",
                    hasPrivateKey,
                    hasPrivateKey ? "✓" : "⚠️ (mTLS will fail!)");

                _logger.LogInformation("  Valid from: {NotBefore} {Status}",
                    certificate.NotBefore.ToString("yyyy-MM-dd"),
                    isNotBefore ? "✓" : "⚠️ (Not yet valid!)");

                _logger.LogInformation("  Valid to: {NotAfter} {Status}",
                    certificate.NotAfter.ToString("yyyy-MM-dd"),
                    isNotExpired ? "✓" : "⚠️ (EXPIRED!)");

                // Check for required CN/OU patterns
                string subject = certificate.Subject;
                if (subject.Contains("CN=zgw-klantp", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("  CN pattern: ✓ Contains 'zgw-klantp'");
                }
                else
                {
                    _logger.LogWarning("  CN pattern: ⚠️ Missing 'zgw-klantp' in CN");
                }

                // Overall status
                if (hasPrivateKey && isNotExpired && isNotBefore)
                {
                    _logger.LogInformation("  Overall: ✓ Certificate appears valid for mTLS");
                }
                else
                {
                    _logger.LogError("  Overall: ⚠️ Certificate has issues that may cause mTLS failures");
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to log certificate verification details");
            }
        }

        /// <summary>
        /// Queries the BRP Personen API (v2.0) for a person's data using their BSN.
        /// </summary>
        public async Task<string> QueryPersonAsync(string bsn)
        {
            if (string.IsNullOrWhiteSpace(bsn))
            {
                throw new ArgumentNullException(nameof(bsn), @"BSN cannot be null or empty");
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
                _logger.LogInformation(
                    "BrpClient.QueryPerson.Start BrpBaseUrl={BrpBaseUrl}",
                    _brpBaseUrl
                );

                // Step 1: Get service token from Keycloak
                _logger.LogInformation("BrpClient.GetServiceToken.Start");
                string serviceToken = await GetServiceTokenAsync();
                _logger.LogInformation(
                    "BrpClient.GetServiceToken.Success TokenLength={TokenLength}",
                    serviceToken.Length
                );

                // Debug the service token
                DebugToken(serviceToken, "Service Token");

                // Step 2: Exchange for BRP token
                _logger.LogInformation("BrpClient.ExchangeToken.Start");
                string brpToken = await ExchangeForBrpTokenAsync(serviceToken);
                _logger.LogInformation(
                    "BrpClient.ExchangeToken.Success TokenLength={TokenLength}",
                    brpToken.Length
                );

                // Debug the BRP token (most important!)
                DebugToken(brpToken, "BRP Token");

                // Step 3: Call BRP API with the single correct endpoint
                string result = await CallBrpApiAsync(brpToken, bsn);

                _logger.LogInformation(
                    "BrpClient.QueryPerson.Success ResponseLength={ResponseLength}",
                    result.Length
                );

                return result;
            }
            catch (HttpRequestException httpEx) when (httpEx.Message.Contains("certificate", StringComparison.OrdinalIgnoreCase) ||
                                                     httpEx.Message.Contains("TLS", StringComparison.OrdinalIgnoreCase) ||
                                                     httpEx.Message.Contains("SSL", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogCritical(httpEx,
                    "CERTIFICATE/TLS ERROR: Likely mTLS authentication failure. " +
                    "Check certificate configuration in program.cs. " +
                    "Response status: {StatusCode}",
                    httpEx.StatusCode);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "BrpClient.QueryPerson.Failure");
                throw;
            }
        }

        private async Task<string> GetServiceTokenAsync()
        {
            return await _keycloakTokenService.GetServiceTokenAsync();
        }

        private async Task<string> ExchangeForBrpTokenAsync(string sourceToken)
        {
            return await _keycloakTokenService.ExchangeForBrpTokenAsync(sourceToken);
        }

        private async Task<string> CallBrpApiAsync(string brpToken, string bsn)
        {
            // Use the single correct endpoint based on the working CURL example
            string endpoint = $"{_brpBaseUrl}/brp/personen";

            _logger.LogInformation("Using BRP endpoint: {Endpoint}", endpoint);

            return await CallBrpApiAsync(endpoint, brpToken, bsn);
        }

        private async Task<string> CallBrpApiAsync(string url, string brpToken, string bsn)
        {
            try
            {
                // Create new request to avoid header contamination
                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                // Add Authorization header (same as CURL example)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", brpToken);

                // Add required headers for WS Gateway
                request.Headers.Add("x-api-version", "2.0");
                request.Headers.Add("Accept", "application/json");

                // Note about certificate
                _logger.LogDebug("Certificate-based mTLS should be configured via HttpClientHandler");

                // Optional: Add correlation ID
                request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());

                // Log request details
                LogRequestDetails(request, bsn);

                // Prepare request body EXACTLY like the CURL example
                object requestBody = new
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

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                string json = JsonSerializer.Serialize(requestBody, serializerOptions);

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug(
                    "Sending request to {Url} with body length {BodyLength}",
                    url, json.Length
                );

                _logger.LogDebug("Request body: {RequestBody}", json);

                // Send request
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // Log response details
                await LogResponseDetailsAsync(response);

                if (!response.IsSuccessStatusCode)
                {
                    throw await CreateHttpExceptionAsync(response);
                }

                // Success - return response
                string responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogDebug(
                    "Response preview (first 200 chars): {Preview}",
                    responseBody.Length > 200 ? responseBody.Substring(0, 200) + "..." : responseBody
                );

                return responseBody;
            }
            catch (HttpRequestException)
            {
                throw; // Re-throw HttpRequestException as-is
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error calling BRP API");
                throw new HttpRequestException($"Failed to call BRP API: {exception.Message}", exception);
            }
        }

        private void LogHttpClientConfiguration()
        {
            try
            {
                _logger.LogInformation("BRP Client Configuration:");
                _logger.LogInformation("  Base URL from env: {BaseUrl}", _brpBaseUrl);
                _logger.LogInformation("  HttpClient Timeout: {Timeout}", _httpClient.Timeout);
                _logger.LogInformation("  HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error while logging HttpClient configuration");
            }
        }

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

        private void LogRequestDetails(HttpRequestMessage request, string bsn)
        {
            try
            {
                _logger.LogDebug("=== Request Details ===");
                _logger.LogDebug("URL: {Method} {Url}", request.Method, request.RequestUri);
                _logger.LogDebug("BSN length: {BsnLength}", bsn.Length);

                _logger.LogDebug("Headers:");
                foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
                {
                    if (header.Key == "Authorization")
                    {
                        _logger.LogDebug("  Authorization: Bearer [REDACTED]");
                    }
                    else
                    {
                        _logger.LogDebug("  {Key}: {Value}",
                            header.Key, string.Join(", ", header.Value));
                    }
                }

                if (request.Content?.Headers != null)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
                    {
                        _logger.LogDebug("  Content-{Key}: {Value}",
                            header.Key, string.Join(", ", header.Value));
                    }
                }

                _logger.LogDebug("=== End Request Details ===");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to log request details");
            }
        }

        private Task LogResponseDetailsAsync(HttpResponseMessage response)
        {
            try
            {
                _logger.LogInformation("Received response: {StatusCode} {ReasonPhrase}",
                    (int)response.StatusCode, response.ReasonPhrase);

                _logger.LogDebug("=== Response Details ===");
                _logger.LogDebug("Status: {StatusCode} {ReasonPhrase}",
                    (int)response.StatusCode, response.ReasonPhrase);
                _logger.LogDebug("Version: {Version}", response.Version);

                _logger.LogDebug("Headers:");
                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
                {
                    _logger.LogDebug("  {Key}: {Value}",
                        header.Key, string.Join(", ", header.Value));
                }

                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers)
                {
                    _logger.LogDebug("  Content-{Key}: {Value}",
                        header.Key, string.Join(", ", header.Value));
                }

                _logger.LogDebug("=== End Response Details ===");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to log response details");
            }

            return Task.CompletedTask;
        }

        private async Task<HttpRequestException> CreateHttpExceptionAsync(HttpResponseMessage response)
        {
            System.Net.HttpStatusCode statusCode = response.StatusCode;
            string statusText = $"{(int)statusCode} ({statusCode})";

            string errorBody = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "BRP API returned error {StatusCode}. Response body: {ErrorBody}",
                statusText,
                errorBody.Length > 500 ? errorBody.Substring(0, 500) + "..." : errorBody
            );

            // Special handling for certificate/TLS errors
            string errorMessage = $"BRP API returned error {statusText}";

            // Check for common certificate/TLS errors in response
            if (errorBody.Contains("certificate", StringComparison.OrdinalIgnoreCase) ||
                errorBody.Contains("TLS", StringComparison.OrdinalIgnoreCase) ||
                errorBody.Contains("SSL", StringComparison.OrdinalIgnoreCase) ||
                errorBody.Contains("handshake", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage += " (Possible certificate/TLS issue)";
                _logger.LogCritical(
                    "Detected possible certificate/TLS error in response. " +
                    "Check mTLS configuration in program.cs.");
            }

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

            return new HttpRequestException(errorMessage, null, statusCode);
        }

        private void DebugToken(string token, string tokenName)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("{TokenName} is null or empty", tokenName);
                    return;
                }

                // Check if it's a JWT
                string[] parts = token.Split('.');
                if (parts.Length != 3)
                {
                    _logger.LogDebug("{TokenName} doesn't appear to be a JWT", tokenName);
                    return;
                }

                try
                {
                    // Decode the payload (middle part)
                    string payload = parts[1];
                    // Fix base64 padding
                    int padding = 4 - (payload.Length % 4);
                    if (padding < 4)
                    {
                        payload = payload.PadRight(payload.Length + padding, '=');
                    }

                    byte[] payloadBytes = Convert.FromBase64String(payload);
                    string payloadJson = Encoding.UTF8.GetString(payloadBytes);

                    using var document = JsonDocument.Parse(payloadJson);

                    _logger.LogDebug("=== {TokenName} Payload ===", tokenName);

                    var claims = new Dictionary<string, string>();

                    // Extract important claims
                    string[] importantClaims = ["aud", "iss", "sub", "exp", "iat", "azp", "scope", "client_id"];

                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                    {
                        if (importantClaims.Contains(property.Name))
                        {
                            claims[property.Name] = property.Value.ToString();
                        }
                    }

                    foreach (KeyValuePair<string, string> claim in claims)
                    {
                        if (claim.Key is "exp" or "iat")
                        {
                            if (long.TryParse(claim.Value, out long timestamp))
                            {
                                var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                                _logger.LogDebug("  {Claim}: {Value} ({DateTime})",
                                    claim.Key, claim.Value, dateTime.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                            }
                            else
                            {
                                _logger.LogDebug("  {Claim}: {Value}", claim.Key, claim.Value);
                            }
                        }
                        else
                        {
                            _logger.LogDebug("  {Claim}: {Value}", claim.Key, claim.Value);
                        }
                    }

                    // Check if audience contains "haalcentraal" (critical!)
                    if (claims.TryGetValue("aud", out string? audience))
                    {
                        if (audience.Contains("haalcentraal", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("✓ Token has 'haalcentraal' in audience");
                        }
                        else
                        {
                            _logger.LogWarning("✗ Token does NOT have 'haalcentraal' in audience. Actual audience: {Audience}", audience);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("✗ Token has no audience (aud) claim");
                    }

                    _logger.LogDebug("=== End {TokenName} Payload ===", tokenName);
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(exception, "Failed to decode {TokenName} payload", tokenName);
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error while debugging {TokenName}", tokenName);
            }
        }

        /// <summary>
        /// Disposes the resources used by this <see cref="BrpClient"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }
    }
}