// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Properties;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Decorator;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using Notify.Client;

namespace EventsHandler.Services.DataReceiving.Factories
{
    /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameter}"/>
    internal sealed class NotificationClientFactory : IHttpClientFactory<INotifyClient, string>
    {
        private readonly WebApiConfiguration _configuration;
        private readonly ILogger<NotificationClientFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationClientFactory"/> class.
        /// </summary>
        public NotificationClientFactory(WebApiConfiguration configuration, ILogger<NotificationClientFactory> logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameters}.GetHttpClient(TParameters)"/>
        INotifyClient IHttpClientFactory<INotifyClient, string>.GetHttpClient(string organizationId)
        {
            this._logger.LogInformation($"{Resources.Logging_Client_Initialized} {organizationId}");

            return new NotifyClientDecorator(
                new NotificationClient(
                    baseUrl: this._configuration.OMC.API.BaseUrl(),      // The base URL to "Notify NL" API Service
                    apiKey: this._configuration.User.API.Key.NotifyNL()));  // 3rd party-specific "Notify NL" API Key
        }
    }
}