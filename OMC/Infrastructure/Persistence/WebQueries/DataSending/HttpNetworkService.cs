// © 2023, Worth Systems.

using Common.Constants;
using Common.Settings.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using WebQueries.Constants;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Clients.Factories;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.Properties;

namespace WebQueries.DataSending
{
    /// <inheritdoc cref="IHttpNetworkService"/>
    public sealed class HttpNetworkService : IHttpNetworkService
    {
        private static readonly object s_padlock = new();

        private readonly OmcConfiguration _configuration;
        private readonly EncryptionContext _encryptionContext;
        private readonly SemaphoreSlim _semaphore;

        /// <inheritdoc cref="RegularHttpClientFactory"/>
        private readonly IHttpClientFactory<HttpClient, (string /* Header key */, string /* Header value */)[]> _httpClientFactory;

        /// <summary>
        /// Cached reusable HTTP Clients with preconfigured settings (etc., "Authorization" or "Headers").
        /// </summary>
        private readonly ConcurrentDictionary<HttpClientTypes, HttpClient> _httpClients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpNetworkService"/> class.
        /// </summary>
        public HttpNetworkService(OmcConfiguration configuration, EncryptionContext encryptionContext,
                                  IHttpClientFactory<HttpClient, (string, string)[]> httpClientFactory)  // Dependency Injection (DI)
        {
            this._configuration = configuration;
            this._encryptionContext = encryptionContext;
            this._httpClientFactory = httpClientFactory;

            // NOTE: Prevents "TaskCanceledException" in case there is a lot of simultaneous HTTP Requests being called
            this._semaphore = new SemaphoreSlim(
                this._configuration.AppSettings.Network.HttpRequestsSimultaneousNumber());

            // NOTE: This method is working like IHttpClientFactory: builder.Services.AddHttpClient("type_1", client => { });
            InitializeAvailableHttpClients();
        }

        #region Internal methods
        /// <inheritdoc cref="IHttpNetworkService.GetAsync(HttpClientTypes, Uri)"/>
        async Task<HttpRequestResponse> IHttpNetworkService.GetAsync(HttpClientTypes httpClientType, Uri uri)
        {
            return await ExecuteCallAsync(httpClientType, uri, httpMethod: HttpMethod.Get);
        }

        /// <inheritdoc cref="IHttpNetworkService.PostAsync(HttpClientTypes, Uri, string)"/>
        async Task<HttpRequestResponse> IHttpNetworkService.PostAsync(HttpClientTypes httpClientType, Uri uri, string jsonBody)
        {
            // Prepare HTTP Request Body
            StringContent requestBody = new(jsonBody, Encoding.UTF8, QueryValues.Default.Network.ContentType); 

            return await ExecuteCallAsync(httpClientType, uri, httpMethod: HttpMethod.Post, body: requestBody);
        }

        /// <inheritdoc cref="IHttpNetworkService.DeleteAsync(HttpClientTypes, Uri)"/>
        async Task<HttpRequestResponse> IHttpNetworkService.DeleteAsync(HttpClientTypes httpClientType, Uri uri)
        {
            return await ExecuteCallAsync(httpClientType, uri, httpMethod: HttpMethod.Delete);
        }
        #endregion

        #region HTTP Clients
        private void InitializeAvailableHttpClients()
        {
            // Headers
            const string acceptCrsHeader = "Accept-Crs";
            const string contentCrsHeader = "Content-Crs";
            const string authorizeHeader = "Authorization";

            // Values
            const string crsValue = "EPSG:4326";

            // Key-value pairs
            (string, string) acceptCrs = (acceptCrsHeader, crsValue);
            (string, string) contentCrs = (contentCrsHeader, crsValue);

            // Registration of clients => an equivalent of IHttpClientFactory "services.AddHttpClient()"
            this._httpClients.TryAdd(HttpClientTypes.OpenZaak_v1, this._httpClientFactory
                .GetHttpClient([acceptCrs, contentCrs]));  // JWT Token

            this._httpClients.TryAdd(HttpClientTypes.OpenKlant_v1, this._httpClientFactory
                .GetHttpClient([acceptCrs, contentCrs]));  // JWT Token

            this._httpClients.TryAdd(HttpClientTypes.OpenKlant_v2, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.OpenKlant_v2))]));  // API Key

            this._httpClients.TryAdd(HttpClientTypes.Objecten, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.Objecten)), contentCrs]));  // API Key

            this._httpClients.TryAdd(HttpClientTypes.ObjectTypen, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.ObjectTypen)), contentCrs]));  // API Key

            this._httpClients.TryAdd(HttpClientTypes.Telemetry_Contactmomenten, this._httpClientFactory
                .GetHttpClient([("X-NLX-Logrecord-ID", string.Empty), ("X-Audit-Toelichting", string.Empty)]));  // JWT Token

            this._httpClients.TryAdd(HttpClientTypes.Telemetry_Klantinteracties, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.Telemetry_Klantinteracties))]));  // API Key
        }

        /// <summary>
        /// Resolves a specific type of cached <see cref="HttpClient"/> or add a new one if it's not existing.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        private HttpClient ResolveClient(HttpClientTypes httpClientType)
        {
            return httpClientType switch
            {
                // Clients requiring JWT token to be refreshed
                HttpClientTypes.OpenZaak_v1 or
                HttpClientTypes.OpenKlant_v1 or
                HttpClientTypes.Telemetry_Contactmomenten
                    => AuthorizeWithGeneratedJwt(this._httpClients[httpClientType]),

                // Clients using static API keys from configuration
                HttpClientTypes.OpenKlant_v2 or
                HttpClientTypes.Objecten or
                HttpClientTypes.ObjectTypen or
                HttpClientTypes.Telemetry_Klantinteracties
                    => this._httpClients[httpClientType],

                _ => throw new ArgumentException(
                    $"{QueryResources.Authorization_ERROR_HttpClientTypeNotSuported} {httpClientType}"),
            };
        }
        #endregion

        #region Authorization
        /// <summary>
        /// Adds generated JSON Web Token to "Authorization" part of the HTTP Request.
        /// </summary>
        /// <remarks>
        /// The token will be initialized for the first time and then refreshed if it's time expired.
        /// </remarks>
        /// <returns>
        /// The source <see cref="HttpClient"/> with updated "Authorization" header.
        /// </returns>
        private HttpClient AuthorizeWithGeneratedJwt(HttpClient httpClient)
        {
            lock (s_padlock)  // NOTE: Prevents multiple threads to update authorization token of an already used HttpClient
            {
                // TODO: Caching the token until the expiration time doesn't elapse yet
                SecurityKey securityKey = this._encryptionContext.GetSecurityKey(
                    this._configuration.ZGW.Auth.JWT.Secret());

                // Preparing JWT token
                string jwtToken = this._encryptionContext.GetJwtToken(securityKey,
                    this._configuration.ZGW.Auth.JWT.Issuer(),
                    this._configuration.ZGW.Auth.JWT.Audience(),
                    this._configuration.ZGW.Auth.JWT.ExpiresInMin(),
                    this._configuration.ZGW.Auth.JWT.UserId(),
                    this._configuration.ZGW.Auth.JWT.UserName());

                // Set Authorization header
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    CommonValues.Default.Authorization.OpenApi.SecurityScheme.BearerSchema, jwtToken);

                return httpClient;
            }
        }

        /// <summary>
        /// Gets static token for "Headers" part of the HTTP Request.
        /// </summary>
        /// <remarks>
        /// The token will be got from a specific setting defined (per API service) in <see cref="OmcConfiguration"/>.
        /// </remarks>
        /// <returns>
        /// The Key and Value of the "Header" used for authorization purpose.
        /// </returns>
        private string AuthorizeWithStaticApiKey(HttpClientTypes httpClientType)
        {
            return httpClientType switch
            {
                HttpClientTypes.OpenKlant_v2 or
                HttpClientTypes.Telemetry_Klantinteracties
                    => $"{CommonValues.Default.Authorization.Token} {this._configuration.ZGW.Auth.Key.OpenKlant()}",

                HttpClientTypes.Objecten
                    => $"{CommonValues.Default.Authorization.Token} {this._configuration.ZGW.Auth.Key.Objecten()}",

                HttpClientTypes.ObjectTypen
                    => $"{CommonValues.Default.Authorization.Token} {this._configuration.ZGW.Auth.Key.ObjectTypen()}",

                _ => throw new ArgumentException(
                    $"{QueryResources.Authorization_ERROR_HttpClientTypeNotSuported} {httpClientType}")
            };
        }
        #endregion

        #region HTTP Requests
        /// <summary>
        /// Executes the standard safety procedure before and after making the HTTP Request.
        /// </summary>
        private async Task<HttpRequestResponse> ExecuteCallAsync(
            HttpClientTypes httpClientType, Uri uri, HttpMethod httpMethod, HttpContent? body = default)
        {
            try
            {
                await _semaphore.WaitAsync();
                HttpClient client = ResolveClient(httpClientType);

                HttpResponseMessage result = httpMethod switch
                {
                    { } m when m == HttpMethod.Get
                        => await client.GetAsync(uri),

                    { } m when m == HttpMethod.Post
                        => await client.PostAsync(uri, body ?? new StringContent(string.Empty)),

                    { } m when m == HttpMethod.Delete
                        => await client.DeleteAsync(uri),

                    _ => throw new NotSupportedException($"HTTP method '{httpMethod}' is not supported.")
                };

                _semaphore.Release();

                string responseContent = await result.Content.ReadAsStringAsync();
                return result.IsSuccessStatusCode
                    ? HttpRequestResponse.Success(responseContent)
                    : HttpRequestResponse.Failure(responseContent);
            }
            catch (Exception exception)
            {
                return HttpRequestResponse.Failure(exception.Message);
            }
        }
        #endregion
    }
}