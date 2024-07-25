// © 2023, Worth Systems.

using EventsHandler.Controllers.Base;
using EventsHandler.Properties;
using EventsHandler.Services.DataSending.Clients.Decorator;
using EventsHandler.Services.DataSending.Clients.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using Notify.Client;

namespace EventsHandler.Services.DataSending.Clients.Factories
{
    /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameter}"/>
    internal sealed class NotificationClientFactory : IHttpClientFactory<INotifyClient, string>
    {
        private readonly WebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationClientFactory"/> class.
        /// </summary>
        public NotificationClientFactory(WebApiConfiguration configuration)
        {
            this._configuration = configuration;
        }

        /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameters}.GetHttpClient(TParameters)"/>
        INotifyClient IHttpClientFactory<INotifyClient, string>.GetHttpClient(string organizationId)
        {
            OmcController.LogMessage(LogLevel.Trace, $"{Resources.Events_NotifyClientInitialized} {organizationId}.");

            return new NotifyClientDecorator(
                new NotificationClient(
                    baseUrl: this._configuration.OMC.API.BaseUrl.NotifyNL(),  // The base URL to "Notify NL" API Service
                    apiKey: this._configuration.User.API.Key.NotifyNL()));    // 3rd party-specific "Notify NL" API Key
        }
    }
}