﻿// © 2023, Worth Systems.

using EventsHandler.Services.DataSending.Clients.Interfaces;
using Notify.Client;

namespace EventsHandler.Services.DataSending.Clients.Decorator
{
    /// <inheritdoc cref="INotifyClient"/>
    internal sealed class NotifyClientDecorator : INotifyClient
    {
        private readonly NotificationClient _notificationClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyClientDecorator"/> class.
        /// </summary>
        public NotifyClientDecorator(NotificationClient notificationClient)
        {
            this._notificationClient = notificationClient;
        }

        /// <inheritdoc cref="INotifyClient.SendSmsAsync(string, string, Dictionary{string, object})"/>
        async Task<bool> INotifyClient.SendSmsAsync(string mobileNumber, string templateId, Dictionary<string, object> personalisation)
        {
            return await this._notificationClient.SendSmsAsync(mobileNumber, templateId, personalisation) != null;
        }

        /// <inheritdoc cref="INotifyClient.SendEmailAsync(string, string, Dictionary{string, object})"/>
        async Task<bool> INotifyClient.SendEmailAsync(string emailAddress, string templateId, Dictionary<string, object> personalisation)
        {
            return await this._notificationClient.SendEmailAsync(emailAddress, templateId, personalisation) != null;
        }
    }
}