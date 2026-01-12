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
                ?? "https://wsgateway.ot.denhaag.nl/haalcentral/api";

            LogHttpClientConfiguration();
            LogCertificateInfo();
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

                // Step 3: Call BRP API with endpoint fallback
                string result = await CallBrpApiWithFallbackAsync(brpToken, bsn);

                _logger.LogInformation(
                    "BrpClient.QueryPerson.Success ResponseLength={ResponseLength}",
                    result.Length
                );

                return result;
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

        private async Task<string> CallBrpApiWithFallbackAsync(string brpToken, string bsn)
        {
            // List of possible endpoints based on documentation
            string[] endpoints =
            [
                // Documented endpoints from the PDF
                "https://wsgateway.ot.denhaag.nl/haalcentral/api/bro/personen",     // v2.0
                "https://wsgateway.ot.denhaag.nl/haalcentral/api/brpv2/personen",   // v2.0 with BRP Update API
                
                // Alternative patterns
                $"{_brpBaseUrl}/bro/personen",
                $"{_brpBaseUrl}/brpv2/personen",
                $"{_brpBaseUrl}/brp/personen",
                
                // What you were originally trying
                "https://wsgateway.ot.denhaag.nl/haalcentraal/api/brp/personen",
                "https://wsgateway.ot.denhaag.nl/haalcentraal/api/bro/personen"
            ];

            Exception? lastException = null;

            foreach (string endpoint in endpoints)
            {
                try
                {
                    _logger.LogInformation("Trying BRP endpoint: {Endpoint}", endpoint);
                    return await CallBrpApiAsync(endpoint, brpToken, bsn);
                }
                catch (HttpRequestException httpException) when (
                    httpException.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    httpException.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                    httpException.StatusCode == (System.Net.HttpStatusCode)470) // WS Gateway custom error
                {
                    lastException = httpException;
                    _logger.LogWarning(
                        "Endpoint failed with auth error {StatusCode}: {Endpoint}",
                        httpException.StatusCode, endpoint
                    );
                }
                catch (HttpRequestException httpException) when (httpException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    lastException = httpException;
                    _logger.LogWarning("Endpoint not found (404): {Endpoint}", endpoint);
                }
                catch (Exception exception)
                {
                    lastException = exception;
                    _logger.LogWarning(exception, "Endpoint failed: {Endpoint}", endpoint);
                }
            }

            _logger.LogError(lastException, "All BRP endpoints failed");
            throw new HttpRequestException("All BRP API endpoints failed", lastException);
        }

        private async Task<string> CallBrpApiAsync(string url, string brpToken, string bsn)
        {
            try
            {
                // Create new request to avoid header contamination
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

                // Add Authorization header
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", brpToken);

                // Add required headers for WS Gateway
                request.Headers.Add("x-api-version", "2.0");
                request.Headers.Add("Accept", "application/json");

                // Optional: Add correlation ID
                request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());

                // Log request details
                LogRequestDetails(request, bsn);

                // Prepare request body according to BRP v2.0 spec
                object requestBody = new
                {
                    type = "RaadpleegMetBurgerservicenummer",
                    burgerservicenummer = new[] { bsn },
                    fields = new[]
                    {
                        "burgerservicenummer",
                        "naam",
                        "geboorte",
                        "verblijfplaats",
                        "geslachtsaanduiding"
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

        private void LogCertificateInfo()
        {
            try
            {
                _logger.LogInformation("Checking certificate configuration...");

                HttpClientHandler? handler = GetHttpClientHandler();
                if (handler == null)
                {
                    _logger.LogWarning("Could not access HttpClientHandler for certificate check");
                    return;
                }

                _logger.LogDebug("HttpClientHandler type: {HandlerType}", handler.GetType().Name);

                if (handler.ClientCertificates.Count > 0)
                {
                    _logger.LogInformation("Found {CertificateCount} client certificate(s)",
                        handler.ClientCertificates.Count);

                    foreach (X509Certificate x509Certificate in handler.ClientCertificates)
                    {
                        if (x509Certificate is X509Certificate2 certificate)
                        {
                            LogCertificateDetails(certificate);
                        }
                        else
                        {
                            _logger.LogWarning("Certificate is not X509Certificate2 type: {Type}",
                                x509Certificate.GetType().Name);
                        }
                    }
                }
                else
                {
                    _logger.LogError("NO CLIENT CERTIFICATES FOUND! WS Gateway requires mutual TLS (mTLS).");
                    _logger.LogError("Check BRP_CLIENTCERT_PEM_PATH and BRP_CLIENTKEY_PEM_PATH environment variables.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while checking certificate configuration");
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

        private void LogCertificateDetails(X509Certificate2 certificate)
        {
            try
            {
                _logger.LogInformation("=== Certificate Details ===");

                // Basic properties
                if (!string.IsNullOrEmpty(certificate.Subject))
                {
                    _logger.LogInformation("Subject: {Subject}", certificate.Subject);
                    ExtractAndLogSubjectDetails(certificate.Subject);
                }

                if (!string.IsNullOrEmpty(certificate.Issuer))
                {
                    _logger.LogInformation("Issuer: {Issuer}", certificate.Issuer);
                }

                // Thumbprint
                try
                {
                    string thumbprint = certificate.Thumbprint;
                    if (!string.IsNullOrEmpty(thumbprint))
                    {
                        _logger.LogInformation("Thumbprint: {Thumbprint}", thumbprint);
                    }
                }
                catch
                {
                    _logger.LogInformation("Thumbprint: Not available");
                }

                // Validity dates
                try
                {
                    _logger.LogInformation("Valid from: {NotBefore}", certificate.NotBefore.ToString("yyyy-MM-dd"));
                    _logger.LogInformation("Valid to: {NotAfter}", certificate.NotAfter.ToString("yyyy-MM-dd"));
                }
                catch
                {
                    _logger.LogInformation("Validity dates: Not available");
                }

                // Key info
                try
                {
                    _logger.LogInformation("HasPrivateKey: {HasPrivateKey}", certificate.HasPrivateKey);
                    _logger.LogInformation("KeyAlgorithm: {KeyAlgorithm}", certificate.GetKeyAlgorithm());
                }
                catch
                {
                    // Ignore
                }

                // Extensions
                try
                {
                    if (certificate.Extensions.Count > 0)
                    {
                        _logger.LogDebug("Extensions ({Count}):", certificate.Extensions.Count);
                        foreach (X509Extension extension in certificate.Extensions)
                        {
                            _logger.LogTrace("  - {Oid}", extension.Oid?.FriendlyName ?? extension.Oid?.Value);
                        }
                    }
                }
                catch
                {
                    // Ignore extension errors
                }

                _logger.LogInformation("=== End Certificate Details ===");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to log certificate details");
            }
        }

        private void ExtractAndLogSubjectDetails(string subject)
        {
            try
            {
                List<string> parts = subject.Split(',')
                    .Select(part => part.Trim())
                    .ToList();

                foreach (string part in parts)
                {
                    if (part.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Common Name (CN): {Value}", part.Substring(3));
                    }
                    else if (part.StartsWith("OU=", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Organizational Unit (OU): {Value}", part.Substring(3));
                    }
                    else if (part.StartsWith("O=", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("Organization (O): {Value}", part.Substring(2));
                    }
                    else if (part.StartsWith("C=", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("Country (C): {Value}", part.Substring(2));
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Error parsing subject details");
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

            // Try to parse JSON error
            string errorMessage = $"BRP API returned error {statusText}";
            if (!string.IsNullOrEmpty(errorBody))
            {
                if (errorBody.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using JsonDocument errorJson = JsonDocument.Parse(errorBody);
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

                    using JsonDocument document = JsonDocument.Parse(payloadJson);

                    _logger.LogDebug("=== {TokenName} Payload ===", tokenName);

                    Dictionary<string, string> claims = new Dictionary<string, string>();

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
                        if (claim.Key == "exp" || claim.Key == "iat")
                        {
                            if (long.TryParse(claim.Value, out long timestamp))
                            {
                                DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
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