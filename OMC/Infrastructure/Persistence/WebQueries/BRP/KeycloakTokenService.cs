using System.Net.Http.Json;
using System.Text.Json;
using Common.Settings.Extensions;

namespace WebQueries.BRP
{
    /// <summary>
    /// Service for obtaining and exchanging Keycloak access tokens using client credentials and token exchange flows.
    /// </summary>
    public class KeycloakTokenService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeycloakTokenService"/> class.
        /// </summary>
        /// <param name="http">The HTTP client used to communicate with Keycloak.</param>
        public KeycloakTokenService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Requests a service account access token from Keycloak using the client credentials flow.
        /// </summary>
        /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation, containing the access token.</returns>
        public async Task<string> GetServiceTokenAsync()
        {
            string url = $"{Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakAuthServerUrl)}/token";

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientId)!,
                ["client_secret"] = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientSecret)!,
                ["audience"] = "haalcentraal"
            };

            HttpResponseMessage response = await _http.PostAsync(url, new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();

            JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("access_token").GetString()!;
        }

        /// <summary>
        /// Exchanges an existing access token for a new token targeted at the BRP system using Keycloak's token exchange protocol.
        /// </summary>
        /// <param name="subjectToken">The original access token to exchange.</param>
        /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation, containing the exchanged access token.</returns>
        public async Task<string> ExchangeForBrpTokenAsync(string subjectToken)
        {
            string url = $"{Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakAuthServerUrl)!}/token";

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
                ["client_id"] = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakTokenExchangeClient)!,
                ["client_secret"] = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakClientSecret)!,
                ["subject_token"] = subjectToken,
                ["requested_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                ["audience"] = Environment.GetEnvironmentVariable(ConfigExtensions.KeyCloakTokenExchangeAudience)!
            };

            HttpResponseMessage response = await _http.PostAsync(url, new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();

            JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("access_token").GetString()!;
        }
    }
}
