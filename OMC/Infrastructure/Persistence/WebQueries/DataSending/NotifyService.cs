﻿// © 2023, Worth Systems.

using Common.Extensions;
using WebQueries.DataSending.Clients.Factories;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Serialization.Interfaces;

namespace WebQueries.DataSending
{
    /// <inheritdoc cref="INotifyService{TPackage}"/>
    public sealed class NotifyService : INotifyService<NotifyData>
    {
        #region Cached HttpClient
        private static readonly object s_padlock = new();

        private static INotifyClient? s_httpClient;
        #endregion

        /// <inheritdoc cref="NotificationClientFactory"/>
        private readonly IHttpClientFactory<INotifyClient, string> _notifyClientFactory;
        private readonly ISerializationService _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyService"/> class.
        /// </summary>
        public NotifyService(IHttpClientFactory<INotifyClient, string> notifyClientFactory, ISerializationService serializer)  // Dependency Injection (DI)
        {
            this._notifyClientFactory = notifyClientFactory;
            this._serializer = serializer;
        }

        /// <inheritdoc cref="INotifyService{TPackage}.SendEmailAsync(TPackage)"/>
        async Task<NotifySendResponse> INotifyService<NotifyData>.SendEmailAsync(NotifyData package)
        {
            return await ResolveNotifyClient(package.Reference.Notification)
                .SendEmailAsync(emailAddress:    package.ContactDetails,
                                templateId:      package.TemplateId.ToString(),
                                personalization: package.Personalization,
                                reference:       await PrepareReferenceAsync(package.Reference));
        }

        /// <inheritdoc cref="INotifyService{TPackage}.SendSmsAsync(TPackage)"/>
        async Task<NotifySendResponse> INotifyService<NotifyData>.SendSmsAsync(NotifyData package)
        {
            return await ResolveNotifyClient(package.Reference.Notification)
                .SendSmsAsync(mobileNumber:    GetDutchFallbackNumber(package.ContactDetails),
                              templateId:      package.TemplateId.ToString(),
                              personalization: package.Personalization,
                              reference:       await PrepareReferenceAsync(package.Reference));
        }

        /// <inheritdoc cref="INotifyService{TPackage}.GenerateTemplatePreviewAsync(TPackage)"/>
        async Task<NotifyTemplateResponse> INotifyService<NotifyData>.GenerateTemplatePreviewAsync(NotifyData package)
        {
            return await ResolveNotifyClient(package.Reference.Notification)
                .GenerateTemplatePreviewAsync(templateId:      package.TemplateId.ToString(),
                                              personalization: package.Personalization);
        }

        /// <inheritdoc cref="INotifyService{TPackage}.GetNotificationDataAsync(TPackage, Guid)"/>
        async Task<NotificationData> INotifyService<NotifyData>.GetNotificationDataAsync(NotifyData package, Guid notificationId)
        {
            return await ResolveNotifyClient(package.Reference.Notification)
                .GetNotificationDataAsync(notificationId: notificationId);
        }

        #region IDisposable
        /// <inheritdoc cref="IDisposable.Dispose"/>>
        void IDisposable.Dispose()
        {
            // Clears the cached client
            s_httpClient = null;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the cached <see cref="INotifyClient"/> or create a new one if not yet existing.
        /// </summary>
        private INotifyClient ResolveNotifyClient(NotificationEvent notification)
        {
            if (s_httpClient == null)
            {
                lock (s_padlock)
                {
                    s_httpClient ??= this._notifyClientFactory.GetHttpClient(
                        notification.GetOrganizationId());  // NOTE: Used for logging purposes only
                }
            }

            return s_httpClient;
        }

        private static string GetDutchFallbackNumber(string initialMobileNumber)
        {
            const string dutchCountryCode = "+31";
            const char missingCountryCode = '0';

            return initialMobileNumber.Length > 0 &&
                   initialMobileNumber[0] == missingCountryCode
                // If the country code is missing adds the Dutch one as a default option
                ? string.Concat(dutchCountryCode, initialMobileNumber[1..].AsSpan())
                : initialMobileNumber;
        }

        /// <summary>
        /// Gets encoded version of the serialized notification to make it more compact and harder to read.
        /// </summary>
        private async Task<string> PrepareReferenceAsync(NotifyReference reference)
        {
            // Serialize object to string
            string serializedNotification = this._serializer.Serialize(reference);

            // Encode & compress the string
            return await serializedNotification.CompressGZipAsync(CancellationToken.None);
        }
        #endregion
    }
}