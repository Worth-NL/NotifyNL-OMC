// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataSending.Clients.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;

namespace EventsHandler.Services.DataSending
{
    /// <inheritdoc cref="INotifyService{TModel, TPackage}"/>
    internal sealed class NotifyService : INotifyService<NotificationEvent, NotifyData>
    {
        #region Cached HttpClient
        private static readonly object s_padlock = new();

        private static INotifyClient? s_httpClient;
        #endregion

        private readonly IHttpClientFactory<INotifyClient, string> _clientFactory;
        private readonly ISerializationService _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyService"/> class.
        /// </summary>
        public NotifyService(
            IHttpClientFactory<INotifyClient, string> clientFactory,
            ISerializationService serializer)
        {
            this._clientFactory = clientFactory;
            this._serializer = serializer;
        }

        /// <inheritdoc cref="INotifyService{TModel, TPackage}.SendSmsAsync(TModel, TPackage)"/>
        async Task INotifyService<NotificationEvent, NotifyData>.SendSmsAsync(NotificationEvent notification, NotifyData package)
        {
            string serializedNotification = this._serializer.Serialize(notification);
            string encodedNotification = serializedNotification.Base64Encode();

            _ = await ResolveNotifyClient(notification).SendSmsAsync(mobileNumber:    package.ContactDetails,
                                                                     templateId:      package.TemplateId.ToString(),
                                                                     personalization: package.Personalization,
                                                                     reference:       encodedNotification);
        }

        /// <inheritdoc cref="INotifyService{TModel, TPackage}.SendEmailAsync(TModel, TPackage)"/>
        async Task INotifyService<NotificationEvent, NotifyData>.SendEmailAsync(NotificationEvent notification, NotifyData package)
        {
            string serializedNotification = this._serializer.Serialize(notification);
            string encodedNotification = serializedNotification.Base64Encode();

            _ = await ResolveNotifyClient(notification).SendEmailAsync(emailAddress:    package.ContactDetails,
                                                                       templateId:      package.TemplateId.ToString(),
                                                                       personalization: package.Personalization,
                                                                       reference:       encodedNotification);
        }

        #region IDisposable
        /// <inheritdoc cref="IDisposable.Dispose"/>>
        void IDisposable.Dispose()
        {
            s_httpClient = null;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the cached <see cref="INotifyClient"/> or create a new one if not yet existing.
        /// <para>
        ///   The organization identifier (since it's unique) will be used as a key for <see cref="INotifyClient"/>.
        /// </para>
        /// </summary>
        private INotifyClient ResolveNotifyClient(NotificationEvent notification)
        {
            if (s_httpClient == null)
            {
                lock (s_padlock)
                {
                    s_httpClient ??= this._clientFactory.GetHttpClient(notification.GetOrganizationId());
                }
            }

            return s_httpClient;
        }
        #endregion
    }
}