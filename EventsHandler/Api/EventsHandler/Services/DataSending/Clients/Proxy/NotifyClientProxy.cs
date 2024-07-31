// © 2023, Worth Systems.

using EventsHandler.Services.DataSending.Clients.Interfaces;
using Notify.Client;
using Notify.Exceptions;

namespace EventsHandler.Services.DataSending.Clients.Proxy
{
    /// <inheritdoc cref="INotifyClient"/>
    internal sealed class NotifyClientProxy : INotifyClient
    {
        private readonly NotificationClient _notificationClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyClientProxy"/> class.
        /// </summary>
        public NotifyClientProxy(NotificationClient notificationClient)
        {
            this._notificationClient = notificationClient;
        }

        /// <inheritdoc cref="INotifyClient.SendSmsAsync(string, string, Dictionary{string, object}, string)"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="NotifyClientException"/>
        async Task<bool> INotifyClient.SendSmsAsync(string mobileNumber, string templateId, Dictionary<string, object> personalization, string reference)
        {
            return await this._notificationClient.SendSmsAsync(mobileNumber, templateId, personalization, reference) != null;
        }

        /// <inheritdoc cref="INotifyClient.SendEmailAsync(string, string, Dictionary{string, object}, string)"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="NotifyClientException"/>
        async Task<bool> INotifyClient.SendEmailAsync(string emailAddress, string templateId, Dictionary<string, object> personalization, string reference)
        {
            return await this._notificationClient.SendEmailAsync(emailAddress, templateId, personalization, reference) != null;
        }
    }
}