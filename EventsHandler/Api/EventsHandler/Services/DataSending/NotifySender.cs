// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;

namespace EventsHandler.Services.DataSending
{
    /// <inheritdoc cref="ISendingService{TModel, TPackage}"/>
    internal sealed class NotifySender : ISendingService<NotificationEvent, NotifyData>
    {
        #region Cached HttpClient
        private static INotifyClient? s_httpClient;

        private static readonly object s_padlock = new();
        #endregion

        private readonly IHttpClientFactory<INotifyClient, string> _clientFactory;
        private readonly ISerializationService _serializer;
        private readonly ITelemetryService _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifySender"/> class.
        /// </summary>
        public NotifySender(
            IHttpClientFactory<INotifyClient, string> clientFactory,
            ISerializationService serializer,
            ITelemetryService telemetry)
        {
            this._clientFactory = clientFactory;
            this._serializer = serializer;
            this._telemetry = telemetry;
        }

        /// <inheritdoc cref="ISendingService{TModel, TPackage}.SendSmsAsync(TModel, TPackage)"/>
        async Task ISendingService<NotificationEvent, NotifyData>.SendSmsAsync(NotificationEvent notification, NotifyData package)
        {
            string serializedNotification = this._serializer.Serialize(notification);
            string encodedNotification = serializedNotification.Base64Encode();

            _ = await ResolveNotifyClient(notification).SendSmsAsync(mobileNumber:    package.ContactDetails,
                                                                     templateId:      package.TemplateId,
                                                                     personalization: package.Personalization,
                                                                     reference:       encodedNotification);

            _ = await this._telemetry.ReportCompletionAsync(notification, package.NotificationMethod,
                $"SMS: {Resources.Register_NotifyNL_SUCCESS_NotificationSent}");
        }

        /// <inheritdoc cref="ISendingService{TModel, TPackage}.SendEmailAsync(TModel, TPackage)"/>
        async Task ISendingService<NotificationEvent, NotifyData>.SendEmailAsync(NotificationEvent notification, NotifyData package)
        {
            string serializedNotification = this._serializer.Serialize(notification);
            string encodedNotification = serializedNotification.Base64Encode();
            
            _ = await ResolveNotifyClient(notification).SendEmailAsync(emailAddress:    package.ContactDetails,
                                                                       templateId:      package.TemplateId,
                                                                       personalization: package.Personalization,
                                                                       reference:       encodedNotification);

            _ = await this._telemetry.ReportCompletionAsync(notification, package.NotificationMethod,
                $"Email: {Resources.Register_NotifyNL_SUCCESS_NotificationSent}");
        }

        #region IDisposable
        /// <inheritdoc cref="IDisposable.Dispose"/>>
        void IDisposable.Dispose()
        {
            s_httpClient = null;
        }
        #endregion

        #region Helper methods
        // TODO: HttpClient can be cached on service level

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