// © 2026, Worth Systems.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly string _authServerUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _audience;
        private readonly JwtSecurityTokenHandler _tokenHandler;

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
            _tokenHandler = new JwtSecurityTokenHandler();

            // Load and validate configuration
            _authServerUrl = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakAuthServerUrl)
                ?? throw new InvalidOperationException("KEYCLOAK_AUTH_SERVER_URL environment variable is required");
            _clientId = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientId)
                ?? throw new InvalidOperationException("KEYCLOAK_CLIENT_ID environment variable is required");
            _clientSecret = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientSecret)
                ?? throw new InvalidOperationException("KEYCLOAK_CLIENT_SECRET environment variable is required");
            _audience = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakTokenExchangeAudience)
                ?? "haalcentraal";

            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            try
            {
                _logger.LogInformation("=== Keycloak Configuration ===");
                _logger.LogInformation("Auth Server URL: {AuthServerUrl}", MaskUrl(_authServerUrl));
                _logger.LogInformation("Client ID: {ClientId}", _clientId);
                _logger.LogInformation("Client Secret: [REDACTED] (Length: {Length})", _clientSecret.Length);
                _logger.LogInformation("Target Audience: {Audience}", _audience);
                _logger.LogInformation("=== End Configuration ===");

                // Validate URL
                if (!Uri.TryCreate(_authServerUrl, UriKind.Absolute, out Uri? authUri))
                {
                    throw new InvalidOperationException($"Invalid Keycloak URL: {_authServerUrl}");
                }

                if (!authUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Keycloak URL is not using HTTPS. This is insecure for production!");
                }

                // Validate required values
                if (string.IsNullOrWhiteSpace(_clientSecret))
                {
                    throw new InvalidOperationException("KEYCLOAK_CLIENT_SECRET cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(_audience))
                {
                    throw new InvalidOperationException("Keycloak audience cannot be empty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Keycloak configuration validation failed");
                throw;
            }
        }

        private string MaskUrl(string url)
        {
            // Mask sensitive parts of URL
            try
            {
                var uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
            }
            catch
            {
                return "[Invalid URL]";
            }
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

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Operation"] = "GetServiceToken"
            }))
            {
                try
                {
                    _logger.LogInformation(
                        "GetServiceToken.Start AuthServerUrlPresent={AuthServerUrlPresent} ClientIdPresent={ClientIdPresent}",
                        !string.IsNullOrWhiteSpace(_authServerUrl),
                        !string.IsNullOrWhiteSpace(_clientId)
                    );

                    string url = $"{_authServerUrl}/token";

                    var form = new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials",
                        ["client_id"] = _clientId,
                        ["client_secret"] = _clientSecret
                    };

                    _logger.LogDebug(
                        "GetServiceToken.HttpPost.Start Url={Url} FormKeys={FormKeys}",
                        MaskUrl(url),
                        string.Join(",", form.Keys)
                    );

                    using var content = new FormUrlEncodedContent(form);
                    HttpResponseMessage response = await _http.PostAsync(url, content);

                    _logger.LogInformation(
                        "GetServiceToken.HttpPost.Response StatusCode={StatusCode}",
                        (int)response.StatusCode
                    );

                    // Don't just assume 200 means success - validate the response
                    string rawResponse = await response.Content.ReadAsStringAsync();

                    _logger.LogDebug(
                        "GetServiceToken.HttpPost.Body ResponseLength={Length}",
                        rawResponse.Length
                    );

                    // Validate response is JSON
                    if (string.IsNullOrWhiteSpace(rawResponse))
                    {
                        throw new InvalidOperationException("Keycloak returned empty response");
                    }

                    if (!rawResponse.TrimStart().StartsWith("{"))
                    {
                        _logger.LogError("Keycloak response is not JSON. Response: {Response}",
                            rawResponse.Length > 200 ? rawResponse.Substring(0, 200) + "..." : rawResponse);
                        throw new InvalidOperationException("Keycloak response is not valid JSON");
                    }

                    JsonElement json;
                    try
                    {
                        json = JsonDocument.Parse(rawResponse).RootElement;
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to parse Keycloak response as JSON");
                        throw new InvalidOperationException("Failed to parse Keycloak response as JSON", jsonEx);
                    }

                    // Check for error in response (even with 200 status!)
                    if (json.TryGetProperty("error", out JsonElement errorElement))
                    {
                        string error = errorElement.GetString() ?? "Unknown error";
                        string errorDescription = json.TryGetProperty("error_description", out JsonElement descElement)
                            ? descElement.GetString() ?? string.Empty
                            : string.Empty;

                        _logger.LogError(
                            "Keycloak returned error: {Error}. Description: {Description}",
                            error, errorDescription
                        );
                        throw new InvalidOperationException($"Keycloak error: {error}. {errorDescription}");
                    }

                    // Validate token exists
                    if (!json.TryGetProperty("access_token", out JsonElement tokenElement))
                    {
                        _logger.LogError(
                            "Keycloak response missing access_token. Response: {Response}",
                            json.ToString()
                        );
                        throw new InvalidOperationException("Keycloak response did not contain access_token");
                    }

                    string token = tokenElement.GetString()!;

                    // Validate token is not empty
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        _logger.LogError("Keycloak returned empty access_token");
                        throw new InvalidOperationException("Keycloak returned empty access_token");
                    }

                    _logger.LogInformation(
                        "GetServiceToken.Success TokenLength={TokenLength}",
                        token.Length
                    );

                    // Validate the JWT token structure
                    ValidateJwtToken(token, "Service Token", expectedAudience: null);

                    return token;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetServiceToken.Failure");
                    throw;
                }
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

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Operation"] = "ExchangeForBrpToken"
            }))
            {
                try
                {
                    _logger.LogInformation(
                        "ExchangeForBrpToken.Start SubjectTokenLength={SubjectTokenLength} Audience={Audience}",
                        subjectToken.Length,
                        _audience
                    );

                    // First validate the subject token
                    ValidateJwtToken(subjectToken, "Subject Token", expectedAudience: null);

                    string url = $"{_authServerUrl}/token";

                    var form = new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
                        ["client_id"] = _clientId,
                        ["client_secret"] = _clientSecret,
                        ["subject_token"] = subjectToken,
                        ["requested_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                        ["audience"] = _audience
                    };

                    _logger.LogDebug(
                        "ExchangeForBrpToken.HttpPost.Start Url={Url} FormKeys={FormKeys}",
                        MaskUrl(url),
                        string.Join(",", form.Keys)
                    );

                    using var content = new FormUrlEncodedContent(form);
                    HttpResponseMessage response = await _http.PostAsync(url, content);

                    _logger.LogInformation(
                        "ExchangeForBrpToken.HttpPost.Response StatusCode={StatusCode}",
                        (int)response.StatusCode
                    );

                    // Validate response
                    string rawResponse = await response.Content.ReadAsStringAsync();

                    _logger.LogDebug(
                        "ExchangeForBrpToken.HttpPost.Body ResponseLength={Length}",
                        rawResponse.Length
                    );

                    // Check for empty response
                    if (string.IsNullOrWhiteSpace(rawResponse))
                    {
                        throw new InvalidOperationException("Keycloak token exchange returned empty response");
                    }

                    // Check if response is JSON
                    if (!rawResponse.TrimStart().StartsWith("{"))
                    {
                        _logger.LogError("Keycloak token exchange response is not JSON. Response: {Response}",
                            rawResponse.Length > 200 ? rawResponse.Substring(0, 200) + "..." : rawResponse);
                        throw new InvalidOperationException("Keycloak token exchange response is not valid JSON");
                    }

                    JsonElement json;
                    try
                    {
                        json = JsonDocument.Parse(rawResponse).RootElement;
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to parse token exchange response as JSON");
                        throw new InvalidOperationException("Failed to parse token exchange response as JSON", jsonEx);
                    }

                    // Check for error in response
                    if (json.TryGetProperty("error", out JsonElement errorElement))
                    {
                        string error = errorElement.GetString() ?? "Unknown error";
                        string errorDescription = json.TryGetProperty("error_description", out JsonElement descElement)
                            ? descElement.GetString() ?? string.Empty
                            : string.Empty;

                        _logger.LogError(
                            "Keycloak token exchange error: {Error}. Description: {Description}",
                            error, errorDescription
                        );
                        throw new InvalidOperationException($"Keycloak token exchange error: {error}. {errorDescription}");
                    }

                    // Validate token exists
                    if (!json.TryGetProperty("access_token", out JsonElement tokenElement))
                    {
                        _logger.LogError(
                            "Keycloak token exchange response missing access_token. Response: {Response}",
                            json.ToString()
                        );
                        throw new InvalidOperationException("Keycloak token exchange response did not contain access_token");
                    }

                    string token = tokenElement.GetString()!;

                    // Validate token is not empty
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        _logger.LogError("Keycloak returned empty access_token from token exchange");
                        throw new InvalidOperationException("Keycloak returned empty access_token from token exchange");
                    }

                    _logger.LogInformation(
                        "ExchangeForBrpToken.Success TokenLength={TokenLength}",
                        token.Length
                    );

                    // CRITICAL: Validate the exchanged token has correct audience
                    ValidateJwtToken(token, "BRP Token", expectedAudience: _audience);

                    return token;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ExchangeForBrpToken.Failure");
                    throw;
                }
            }
        }

        private void ValidateJwtToken(string token, string tokenName, string? expectedAudience)
        {
            try
            {
                _logger.LogDebug("Validating {TokenName}...", tokenName);

                // Basic validation
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException($"{tokenName} is null or empty");
                }

                // Check if it looks like a JWT (3 parts separated by dots)
                string[] parts = token.Split('.');
                if (parts.Length != 3)
                {
                    throw new InvalidOperationException($"{tokenName} is not a valid JWT (doesn't have 3 parts)");
                }

                // Try to read the JWT
                if (!_tokenHandler.CanReadToken(token))
                {
                    throw new InvalidOperationException($"{tokenName} is not a readable JWT");
                }

                JwtSecurityToken? jwtToken = _tokenHandler.ReadJwtToken(token);

                _logger.LogDebug("=== {TokenName} Validation ===", tokenName);

                // Check expiration
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    _logger.LogError("{TokenName} is EXPIRED! ValidTo: {ValidTo} (UTC Now: {UtcNow})",
                        tokenName, jwtToken.ValidTo, DateTime.UtcNow);
                    throw new InvalidOperationException($"{tokenName} is expired");
                }

                // Check audience (most important for BRP token!)
                if (jwtToken.Audiences.Any())
                {
                    var audiences = jwtToken.Audiences.ToList();
                    _logger.LogDebug("Audiences: {Audiences}", string.Join(", ", audiences));

                    if (expectedAudience != null)
                    {
                        bool hasExpectedAudience = audiences.Any(aud =>
                            aud.Contains(expectedAudience, StringComparison.OrdinalIgnoreCase));

                        if (hasExpectedAudience)
                        {
                            _logger.LogInformation("✓ {TokenName} has expected audience: {Audience}",
                                tokenName, expectedAudience);
                        }
                        else
                        {
                            _logger.LogError("✗ {TokenName} does NOT have expected audience: {ExpectedAudience}. Actual audiences: {ActualAudiences}",
                                tokenName, expectedAudience, string.Join(", ", audiences));
                            throw new InvalidOperationException($"{tokenName} does not have expected audience '{expectedAudience}'");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("{TokenName} has no audiences claim", tokenName);
                }

                // Log other important claims
                LogTokenClaims(jwtToken, tokenName);

                _logger.LogDebug("=== End {TokenName} Validation ===", tokenName);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Failed to validate {TokenName}", tokenName);
                throw new InvalidOperationException($"Failed to validate {tokenName}: {ex.Message}", ex);
            }
        }

        private void LogTokenClaims(JwtSecurityToken jwtToken, string tokenName)
        {
            try
            {
                var importantClaims = new Dictionary<string, string>();

                // Extract important claims
                foreach (Claim? claim in jwtToken.Claims)
                {
                    switch (claim.Type)
                    {
                        case "iss":
                            importantClaims["Issuer"] = claim.Value;
                            break;
                        case "sub":
                            importantClaims["Subject"] = claim.Value;
                            break;
                        case "exp":
                            if (long.TryParse(claim.Value, out long expTimestamp))
                            {
                                var expTime = DateTimeOffset.FromUnixTimeSeconds(expTimestamp);
                                importantClaims["Expires"] = $"{expTime:yyyy-MM-dd HH:mm:ss UTC}";
                            }
                            break;
                        case "iat":
                            if (long.TryParse(claim.Value, out long iatTimestamp))
                            {
                                var iatTime = DateTimeOffset.FromUnixTimeSeconds(iatTimestamp);
                                importantClaims["Issued At"] = $"{iatTime:yyyy-MM-dd HH:mm:ss UTC}";
                            }
                            break;
                        case "azp":
                            importantClaims["Authorized Party"] = claim.Value;
                            break;
                        case "scope":
                            importantClaims["Scope"] = claim.Value;
                            break;
                        case "client_id":
                            importantClaims["Client ID"] = claim.Value;
                            break;
                    }
                }

                if (!importantClaims.Any())
                {
                    return;
                }

                {
                    _logger.LogDebug("{TokenName} claims:", tokenName);
                    foreach (KeyValuePair<string, string> claim in importantClaims)
                    {
                        _logger.LogDebug("  {Key}: {Value}", claim.Key, claim.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to log {TokenName} claims", tokenName);
            }
        }
    }
}