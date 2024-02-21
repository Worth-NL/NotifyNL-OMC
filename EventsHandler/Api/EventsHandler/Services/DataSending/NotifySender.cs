﻿// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Extensions;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
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

        private readonly IHttpClientFactory<INotifyClient, string> _notifyClientFactory;
        private readonly IFeedbackTelemetryService _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifySender"/> class.
        /// </summary>
        public NotifySender(
            IHttpClientFactory<INotifyClient, string> notifyClientFactory,
            IFeedbackTelemetryService telemetry)
        {
            this._notifyClientFactory = notifyClientFactory;
            this._telemetry = telemetry;
        }

        /// <inheritdoc cref="ISendingService{TModel, TPackage}.SendSmsAsync(TModel, TPackage)"/>
        async Task ISendingService<NotificationEvent, NotifyData>.SendSmsAsync(NotificationEvent notification, NotifyData package)
        {
            _ = await ResolveNotifyClient(notification).SendSmsAsync(mobileNumber:    package.ContactDetails,
                                                                     templateId:      package.TemplateId,
                                                                     personalisation: package.Personalization);
            // TODO: Extract some data from Notify NL here
            _ = await this._telemetry.ReportCompletionAsync(notification, package.NotificationMethod);
        }

        /// <inheritdoc cref="ISendingService{TModel, TPackage}.SendEmailAsync(TModel, TPackage)"/>
        async Task ISendingService<NotificationEvent, NotifyData>.SendEmailAsync(NotificationEvent notification, NotifyData package)
        {
            _ = await ResolveNotifyClient(notification).SendEmailAsync(emailAddress:    package.ContactDetails,
                                                                       templateId:      package.TemplateId,
                                                                       personalisation: package.Personalization);
            // TODO: Extract some data from Notify NL here
            _ = await this._telemetry.ReportCompletionAsync(notification, package.NotificationMethod);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>>
        void IDisposable.Dispose()
        {
            s_httpClient = null;
        }

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
                    s_httpClient ??= this._notifyClientFactory.GetHttpClient(notification.GetOrganizationId());
                }
            }

            return s_httpClient;
        }
        #endregion
    }
}