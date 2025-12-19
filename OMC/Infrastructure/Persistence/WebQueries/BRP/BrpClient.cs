using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.Settings.Extensions;

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

        /// <summary>
        /// Initializes a new instance of <see cref="BrpClient"/>.
        /// </summary>
        /// <param name="http">The <see cref="HttpClient"/> used for BRP requests.</param>
        /// <param name="tokens">The <see cref="KeycloakTokenService"/> used to obtain and exchange tokens.</param>
        public BrpClient(HttpClient http, KeycloakTokenService tokens)
        {
            _http = http;
            _tokens = tokens;
        }

        /// <summary>
        /// Queries the BRP Personen API (v2.0) for a person's data using their BSN.
        /// </summary>
        /// <param name="bsn">The BSN (Burger Service Nummer) of the person to query.</param>
        public async Task<string> QueryPersonAsync(string bsn)
        {
            // Step 1: obtain service token from Keycloak
            string serviceToken = await _tokens.GetServiceTokenAsync();

            // Step 2: exchange token for Haal Centraal / BRP audience
            string brpToken = await _tokens.ExchangeForBrpTokenAsync(serviceToken);

            // Step 3: call BRP v2.0 via WS Gateway (POST with JSON body)
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

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            string url =
                $"{Environment.GetEnvironmentVariable(ConfigExtensions.BrpBaseUrl)}/brp/personen";

            HttpResponseMessage response = await _http.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }

}
