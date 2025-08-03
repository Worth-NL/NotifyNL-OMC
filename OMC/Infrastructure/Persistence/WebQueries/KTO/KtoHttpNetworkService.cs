// © 2025, Worth Systems.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.Settings.Configuration;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.KTO.Interfaces;
using WebQueries.KTO.Models;

namespace WebQueries.KTO
{
    /// <summary>
    /// Handles HTTP requests with OAuth2 authentication for KTO.
    /// </summary>
    public sealed class KtoHttpNetworkService : IHttpNetworkServiceKto
    {
        private static readonly SemaphoreSlim s_tokenSemaphore = new(1, 1);
        private readonly OmcConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string? _cachedKtoToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="KtoHttpNetworkService"/> class.
        /// </summary>
        public KtoHttpNetworkService(OmcConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient(); // Create an instance from the factory
        }

        /// <summary>
        /// Retrieves an OAuth2 access token for KTO authentication.
        /// </summary>
        private async Task<string?> GetKtoAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedKtoToken) && DateTime.UtcNow < _tokenExpiration)
            {
                return _cachedKtoToken;
            }

            await s_tokenSemaphore.WaitAsync();
            try
            {
                using var httpClient = new HttpClient();
                var requestBody = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _configuration.KTO.Auth.JWT.ClientId() },
                    { "client_secret", _configuration.KTO.Auth.JWT.Secret() },
                    { "scope", _configuration.KTO.Auth.JWT.Scope() }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, _configuration.KTO.Auth.JWT.Issuer());
                request.Content = new FormUrlEncodedContent(requestBody);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseJson = await response.Content.ReadAsStringAsync();
                OAuthTokenResponse? tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse?>(responseJson);

                _cachedKtoToken = tokenResponse?.AccessToken ?? throw new Exception("Access token missing in response.");
                _tokenExpiration = DateTime.UtcNow.AddMinutes(55);

                return _cachedKtoToken;
            }
            finally
            {
                s_tokenSemaphore.Release();
            }
        }

        /// <summary>
        /// Sends a GET request with authentication.
        /// </summary>
        async Task<HttpRequestResponse> IHttpNetworkServiceKto.GetAsync(Uri uri)
        {
            string? token = await GetKtoAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            return response.IsSuccessStatusCode
                ? HttpRequestResponse.Success(await response.Content.ReadAsStringAsync())
                : HttpRequestResponse.Failure(await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Sends a POST request with authentication.
        /// </summary>
        async Task<HttpRequestResponse> IHttpNetworkServiceKto.PostAsync(string jsonBody)
        {
            string? token = await GetKtoAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(_configuration.KTO.Url(), content);
            return response.IsSuccessStatusCode
                ? HttpRequestResponse.Success(await response.Content.ReadAsStringAsync())
                : HttpRequestResponse.Failure(await response.Content.ReadAsStringAsync());
        }
    }
}
