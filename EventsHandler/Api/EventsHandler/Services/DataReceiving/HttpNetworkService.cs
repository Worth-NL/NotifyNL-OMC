// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Properties;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataReceiving.Interfaces;
using Microsoft.IdentityModel.Tokens;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace EventsHandler.Services.DataReceiving
{
    /// <inheritdoc cref="IHttpNetworkService"/>
    internal sealed class HttpNetworkService : IHttpNetworkService
    {
        private readonly WebApiConfiguration _configuration;
        private readonly EncryptionContext _encryptionContext;
        private readonly IHttpClientFactory<HttpClient, (string, string)[]> _httpClientFactory;
        private readonly ConcurrentDictionary<HttpClientTypes, HttpClient> _httpClients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpNetworkService"/> class.
        /// </summary>
        public HttpNetworkService(
            WebApiConfiguration configuration,
            EncryptionContext encryptionContext,
            IHttpClientFactory<HttpClient, (string, string)[]> httpClientFactory)
        {
            this._configuration = configuration;
            this._encryptionContext = encryptionContext;
            this._httpClientFactory = httpClientFactory;
            
            InitializeAvailableHttpClients();
        }

        #region Internal methods
        /// <inheritdoc cref="IHttpNetworkService.GetAsync(HttpClientTypes, Uri)"/>
        async Task<(bool Success, string JsonResponse)> IHttpNetworkService.GetAsync(HttpClientTypes httpClientType, Uri uri)
        {
            return await ExecuteCallAsync(httpClientType, uri);
        }

        /// <inheritdoc cref="IHttpNetworkService.PostAsync(HttpClientTypes, Uri, HttpContent)"/>
        async Task<(bool Success, string JsonResponse)> IHttpNetworkService.PostAsync(HttpClientTypes httpClientType, Uri uri, HttpContent body)
        {
            return await ExecuteCallAsync(httpClientType, uri, body);
        }
        #endregion

        #region Helper methods
        private void InitializeAvailableHttpClients()
        {
            this._httpClients.TryAdd(HttpClientTypes.OpenZaak_v1, this._httpClientFactory
                .GetHttpClient(new[] { ("Accept-Crs", "EPSG:4326"), ("Content-Crs", "EPSG:4326") }));

            this._httpClients.TryAdd(HttpClientTypes.OpenKlant_v1, this._httpClientFactory
                .GetHttpClient(new[] { ("Accept-Crs", "EPSG:4326"), ("Content-Crs", "EPSG:4326") }));

            // TODO: Open Klant 2 using static token from Environment Variables
            this._httpClients.TryAdd(HttpClientTypes.OpenKlant_v2, this._httpClientFactory
                .GetHttpClient(new[] { ("Authorization", "Token ") }));

            this._httpClients.TryAdd(HttpClientTypes.Telemetry_ContactMomenten, this._httpClientFactory
                .GetHttpClient(new[] { ("X-NLX-Logrecord-ID", ""), ("X-Audit-Toelichting", "") }));  // TODO: Put the right value here
        }

        /// <summary>
        /// Executes the standard safety procedure before and after making the HTTP Request.
        /// </summary>
        private async Task<(bool Success, string JsonResponse)> ExecuteCallAsync(HttpClientTypes httpClientType, Uri uri, HttpContent? body = default)
        {
            try
            {
                // HTTPS protocol validation
                if (uri.Scheme != DefaultValues.Request.HttpsProtocol)
                {
                    return (false, Resources.HttpRequest_ERROR_HttpsProtocolExpected);
                }

                // Determine whether GET or POST call should be sent (depends on if body is required)
                HttpResponseMessage result = body is null
                    ? await AuthorizeWithJwt(ResolveClient(httpClientType)).GetAsync(uri)
                    : await AuthorizeWithJwt(ResolveClient(httpClientType)).PostAsync(uri, body);

                return (result.IsSuccessStatusCode, await result.Content.ReadAsStringAsync());  // Status + JSON response
            }
            catch (Exception exception)
            {
                return (false, exception.Message);
            }
        }

        /// <summary>
        /// Resolves a specific type of cached <see cref="HttpClient"/> or add a new one if it's not existing.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        private HttpClient ResolveClient(HttpClientTypes httpClientType)
        {
            return this._httpClients.GetValueOrDefault(httpClientType)!;
        }

        /// <summary>
        /// Adds JWT Token to "Authorize" header of the HTTP Request.
        /// </summary>
        private HttpClient AuthorizeWithJwt(HttpClient httpClient)
        {
            // TODO: To be considered, caching the token until the expiration time doesn't elapse yet
            SecurityKey securityKey = this._encryptionContext.GetSecurityKey(
                this._configuration.User.Authorization.JWT.Secret());

            // Preparing JWT token
            string jwtToken = this._encryptionContext.GetJwtToken(securityKey,
                this._configuration.User.Authorization.JWT.Issuer(),
                this._configuration.User.Authorization.JWT.Audience(),
                this._configuration.User.Authorization.JWT.ExpiresInMin(),
                this._configuration.User.Authorization.JWT.UserId(),
                this._configuration.User.Authorization.JWT.UserName());

            // Set Authorization header
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                DefaultValues.Authorization.OpenApiSecurityScheme.BearerSchema, jwtToken);

            return httpClient;
        }
        #endregion
    }
}