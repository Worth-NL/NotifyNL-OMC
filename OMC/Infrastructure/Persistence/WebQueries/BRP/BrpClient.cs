using System.Net.Http.Headers;
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
        /// Queries the BRP service for a person's data using their BSN (Burger Service Number).
        /// </summary>
        /// <param name="bsn">The BSN of the person to query.</param>
        /// <returns>A <see cref="Task{String}"/> containing the raw JSON response from BRP.</returns>
        public async Task<string> QueryPersonAsync(string bsn)
        {
            // 1. service token → 2. exchange for BRP token
            string serviceToken = await _tokens.GetServiceTokenAsync();
            string brpToken = await _tokens.ExchangeForBrpTokenAsync(serviceToken);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", brpToken);

            string url = $"{Environment.GetEnvironmentVariable(ConfigExtensions.BrpBaseUrl)!}/{bsn}";
            HttpResponseMessage response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }

}
