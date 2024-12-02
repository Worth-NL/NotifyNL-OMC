// © 2023, Worth Systems.

using Common.Constants;
using Common.Settings.Configuration;
using EventsHandler.Properties;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Clients.Factories;
using EventsHandler.Services.DataSending.Clients.Factories.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using Microsoft.IdentityModel.Tokens;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;

namespace EventsHandler.Services.DataSending
{
    /// <inheritdoc cref="IHttpNetworkService"/>
    internal sealed class HttpNetworkService : IHttpNetworkService
    {
        private static readonly object s_padlock = new();

        private readonly WebApiConfiguration _configuration;
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
        public HttpNetworkService(
            WebApiConfiguration configuration,
            EncryptionContext encryptionContext,
            IHttpClientFactory<HttpClient, (string, string)[]> httpClientFactory)
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
        async Task<RequestResponse> IHttpNetworkService.GetAsync(HttpClientTypes httpClientType, Uri uri)
        {
            return await ExecuteCallAsync(httpClientType, uri);
        }

        /// <inheritdoc cref="IHttpNetworkService.PostAsync(HttpClientTypes, Uri, string)"/>
        async Task<RequestResponse> IHttpNetworkService.PostAsync(HttpClientTypes httpClientType, Uri uri, string jsonBody)
        {
            // Prepare HTTP Request Body
            StringContent requestBody = new(jsonBody, Encoding.UTF8, CommonValues.Default.Request.ContentType);

            return await ExecuteCallAsync(httpClientType, uri, requestBody);
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
            (string, string) acceptCrs  = (acceptCrsHeader,  crsValue);
            (string, string) contentCrs = (contentCrsHeader, crsValue);
            
            // Registration of clients => an equivalent of IHttpClientFactory "services.AddHttpClient()"
            this._httpClients.TryAdd(HttpClientTypes.OpenZaak_v1, this._httpClientFactory
                .GetHttpClient([acceptCrs, contentCrs]));

            this._httpClients.TryAdd(HttpClientTypes.OpenKlant_v1, this._httpClientFactory
                .GetHttpClient([acceptCrs, contentCrs]));

            this._httpClients.TryAdd(HttpClientTypes.OpenKlant_v2, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.OpenKlant_v2))]));

            this._httpClients.TryAdd(HttpClientTypes.Objecten, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.Objecten)), contentCrs]));

            this._httpClients.TryAdd(HttpClientTypes.ObjectTypen, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.ObjectTypen)), contentCrs]));

            this._httpClients.TryAdd(HttpClientTypes.Telemetry_Contactmomenten, this._httpClientFactory
                .GetHttpClient([("X-NLX-Logrecord-ID", string.Empty), ("X-Audit-Toelichting", string.Empty)]));

            this._httpClients.TryAdd(HttpClientTypes.Telemetry_Klantinteracties, this._httpClientFactory
                .GetHttpClient([(authorizeHeader, AuthorizeWithStaticApiKey(HttpClientTypes.Telemetry_Klantinteracties))]));
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
                HttpClientTypes.OpenZaak_v1  or 
                HttpClientTypes.OpenKlant_v1 or
                HttpClientTypes.Telemetry_Contactmomenten
                    => AuthorizeWithGeneratedJwt(this._httpClients[httpClientType]),

                // Clients using static API keys from configuration
                HttpClientTypes.OpenKlant_v2 or
                HttpClientTypes.Objecten     or
                HttpClientTypes.ObjectTypen  or
                HttpClientTypes.Telemetry_Klantinteracties
                    => this._httpClients[httpClientType],

                _ => throw new ArgumentException(
                    $"{ApiResources.Authorization_ERROR_HttpClientTypeNotSuported} {httpClientType}"),
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
                    CommonValues.Default.Authorization.OpenApiSecurityScheme.BearerSchema, jwtToken);

                return httpClient;
            }
        }

        /// <summary>
        /// Gets static token for "Headers" part of the HTTP Request.
        /// </summary>
        /// <remarks>
        /// The token will be got from a specific setting defined (per API service) in <see cref="WebApiConfiguration"/>.
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
                    $"{ApiResources.Authorization_ERROR_HttpClientTypeNotSuported} {httpClientType}")
            };
        }
        #endregion

        #region HTTP Requests
        /// <summary>
        /// Executes the standard safety procedure before and after making the HTTP Request.
        /// </summary>
        private async Task<RequestResponse> ExecuteCallAsync(HttpClientTypes httpClientType, Uri uri, HttpContent? body = default)
        {
            try
            {
                // HTTPS protocol validation
                if (uri.Scheme != CommonValues.Default.Request.HttpsProtocol)
                {
                    return RequestResponse.Failure(ApiResources.HttpRequest_ERROR_HttpsProtocolExpected);
                }

                // Determine whether GET or POST call should be sent (depends on if HTTP body is required)
                await this._semaphore.WaitAsync();
                HttpResponseMessage result = body is null
                    // NOTE: This method is working as IHttpClientFactory: _httpClientFactory.CreateClient("type_1");
                    ? await ResolveClient(httpClientType).GetAsync(uri)
                    : await ResolveClient(httpClientType).PostAsync(uri, body);
                this._semaphore.Release();

                return result.IsSuccessStatusCode
                    ? RequestResponse.Success(await result.Content.ReadAsStringAsync())
                    : RequestResponse.Failure(await result.Content.ReadAsStringAsync());
            }
            catch (Exception exception)
            {
                return RequestResponse.Failure(exception.Message);
            }
        }
        #endregion
    }
}