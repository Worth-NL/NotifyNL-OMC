﻿// © 2023, Worth Systems.

using Common.Settings.Configuration;
using Notify.Client;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Clients.Proxy;

namespace WebQueries.DataSending.Clients.Factories
{
    /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameter}"/>
    /// <remarks>
    ///   Customized to create "Notify NL" Web API service <see cref="NotificationClient"/>s.
    /// </remarks>
    public sealed class NotificationClientFactory : IHttpClientFactory<INotifyClient, string>
    {
        private readonly WebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationClientFactory"/> class.
        /// </summary>
        public NotificationClientFactory(WebApiConfiguration configuration)  // Dependency Injection (DI)
        {
            _configuration = configuration;
        }

        /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameters}.GetHttpClient(TParameters)"/>
        INotifyClient IHttpClientFactory<INotifyClient, string>.GetHttpClient(string _)  // NOTE: This parameter is not used anymore
        {
            return new NotifyClientProxy(
                new NotificationClient(
                    baseUrl: _configuration.Notify.API.BaseUrl().AbsoluteUri,  // The base URL to "Notify NL" API Service
                    apiKey: _configuration.Notify.API.Key()));                // 3rd party-specific "Notify NL" API Key
        }
    }
}