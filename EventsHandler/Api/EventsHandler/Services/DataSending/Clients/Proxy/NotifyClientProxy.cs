// © 2023, Worth Systems.

using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using Notify.Client;
using Notify.Models.Responses;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Services.DataSending.Clients.Proxy
{
    /// <inheritdoc cref="INotifyClient"/>
    [ExcludeFromCodeCoverage(Justification = "The real implementation of NotificationClient from Notify.Client should not be tested.")]
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

        /// <inheritdoc cref="INotifyClient.SendEmailAsync(string, string, Dictionary{string, object}, string)"/>
        async Task<NotifySendResponse> INotifyClient.SendEmailAsync(string emailAddress, string templateId, Dictionary<string, object> personalization, string reference)
        {
            try
            {
                EmailNotificationResponse emailNotificationResponse = await this._notificationClient.SendEmailAsync(emailAddress, templateId, personalization, reference);

                return emailNotificationResponse != null
                    ? NotifySendResponse.Success(emailNotificationResponse.content.body)
                    : NotifySendResponse.Failure();
            }
            catch (Exception exception)
            {
                return NotifySendResponse.Failure(exception.Message);
            }
        }

        /// <inheritdoc cref="INotifyClient.SendSmsAsync(string, string, Dictionary{string, object}, string)"/>
        async Task<NotifySendResponse> INotifyClient.SendSmsAsync(string mobileNumber, string templateId, Dictionary<string, object> personalization, string reference)
        {
            try
            {
                SmsNotificationResponse smsNotificationResponse = await this._notificationClient.SendSmsAsync(mobileNumber, templateId, personalization, reference);

                return smsNotificationResponse != null
                    ? NotifySendResponse.Success(smsNotificationResponse.content.body)
                    : NotifySendResponse.Failure();
            }
            catch (Exception exception)
            {
                return NotifySendResponse.Failure(exception.Message);
            }
        }

        /// <inheritdoc cref="INotifyClient.GenerateTemplatePreviewAsync(string, Dictionary{string, object})"/>
        async Task<NotifyTemplateResponse> INotifyClient.GenerateTemplatePreviewAsync(string templateId, Dictionary<string, object> personalization)
        {
            try
            {
                TemplatePreviewResponse templatePreviewResponse =
                    await this._notificationClient.GenerateTemplatePreviewAsync(templateId, personalization);

                return templatePreviewResponse != null
                    ? NotifyTemplateResponse.Success(templatePreviewResponse.subject, templatePreviewResponse.body)
                    : NotifyTemplateResponse.Failure();
            }
            catch (Exception exception)
            {
                return NotifyTemplateResponse.Failure(exception.Message);
            }
        }
    }
}