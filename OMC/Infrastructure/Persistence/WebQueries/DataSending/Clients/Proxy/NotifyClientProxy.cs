// © 2023, Worth Systems.

using Notify.Client;
using Notify.Exceptions;
using Notify.Models;
using Notify.Models.Responses;
using System.Diagnostics.CodeAnalysis;

using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Models.Reponses;

namespace WebQueries.DataSending.Clients.Proxy
{
    /// <inheritdoc cref="INotifyClient"/>
    [ExcludeFromCodeCoverage(Justification = "The real implementation of NotificationClient from Notify.Client should not be tested.")]
    internal sealed class NotifyClientProxy : INotifyClient
    {
        private readonly NotificationClient _notificationClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyClientProxy"/> class.
        /// </summary>
        internal NotifyClientProxy(NotificationClient notificationClient)
        {
            _notificationClient = notificationClient;
        }

        /// <inheritdoc cref="INotifyClient.SendEmailAsync(string, string, Dictionary{string, object}, string)"/>
        async Task<NotifySendResponse> INotifyClient.SendEmailAsync(string emailAddress, string templateId, Dictionary<string, object> personalization, string reference)
        {
            try
            {
                _ = await _notificationClient.SendEmailAsync(emailAddress, templateId, personalization, reference);

                return NotifySendResponse.Success();
            }
            catch (NotifyClientException exception)  // On failure this method is throwing exception
            {
                return NotifySendResponse.Failure(exception.Message);
            }
        }

        /// <inheritdoc cref="INotifyClient.SendSmsAsync(string, string, Dictionary{string, object}, string)"/>
        async Task<NotifySendResponse> INotifyClient.SendSmsAsync(string mobileNumber, string templateId, Dictionary<string, object> personalization, string reference)
        {
            try
            {
                _ = await _notificationClient.SendSmsAsync(mobileNumber, templateId, personalization, reference);

                return NotifySendResponse.Success();
            }
            catch (NotifyClientException exception)  // On failure this method is throwing exception
            {
                return NotifySendResponse.Failure(exception.Message);
            }
        }

        /// <inheritdoc cref="INotifyClient.SendLetterAsync(string, Dictionary{string, object}, string, IEnumerable{string}?)"/>
        async Task<NotifySendResponse> INotifyClient.SendLetterAsync(string templateId, Dictionary<string, object> personalization, string reference, IEnumerable<string>? attachments)
        {
            try
            {
                _ = await _notificationClient.SendLetterAsync(templateId, personalization, reference, attachments: attachments);

                return NotifySendResponse.Success();
            }
            catch (NotifyClientException exception)  // On failure this method is throwing exception
            {
                return NotifySendResponse.Failure(exception.Message);
            }
        }

        /// <inheritdoc cref="INotifyClient.SendPrecompiledLetterAsync(string, byte[], string?)"/>
        async Task<NotifySendResponse> INotifyClient.SendPrecompiledLetterAsync(string reference, byte[] pdfContents, string? postage)
        {
            try
            {
                // NOTE: The client's own "postage" parameter is nullable-oblivious (it predates NRTs) and
                // treats null as "use the service default", so passing it straight through is correct.
                _ = await _notificationClient.SendPrecompiledLetterAsync(reference, pdfContents, postage!);

                return NotifySendResponse.Success();
            }
            catch (NotifyClientException exception)  // On failure this method is throwing exception
            {
                return NotifySendResponse.Failure(exception.Message);
            }
        }

        /// <inheritdoc cref="INotifyClient.SendMessageBoxNotificationAsync"/>
        async Task<NotifySendResponse> INotifyClient.SendMessageBoxNotificationAsync(
            string recipient,
            string message,
            string messageType,
            string subject,
            IEnumerable<Attachment> attachments,
            string reference)
        {
            try
            {
                _ = await _notificationClient.SendMessageBoxNotificationAsync(
                    recipient, message, messageType, subject, attachments, reference);

                return NotifySendResponse.Success();
            }
            catch (NotifyClientException exception)
            {
                return NotifySendResponse.Failure(exception.Message);
            }
            // TODO (flagged, not fixed): the underlying GovukNotify client's own argument validation
            // (recipient/message/messageType/subject/attachment length+count checks) throws a plain
            // ArgumentException, not NotifyClientException, so an invalid messageType/oversized field
            // would propagate as an unhandled exception here rather than a graceful NotifySendResponse.Failure.
            // Pre-existing gap, not introduced by this change - see WebQueries.MOBB.MessageBoxScenarioImplementation.
        }

        /// <inheritdoc cref="INotifyClient.GenerateTemplatePreviewAsync(string, Dictionary{string, object})"/>
        async Task<NotifyTemplateResponse> INotifyClient.GenerateTemplatePreviewAsync(string templateId, Dictionary<string, object> personalization)
        {
            try
            {
                TemplatePreviewResponse templatePreviewResponse =
                    await _notificationClient.GenerateTemplatePreviewAsync(templateId, personalization);

                return NotifyTemplateResponse.Success(templatePreviewResponse.subject, templatePreviewResponse.body);
            }
            catch (NotifyClientException exception)  // On failure this method is throwing exception
            {
                return NotifyTemplateResponse.Failure(exception.Message);
            }
        }

        /// <inheritdoc cref="INotifyClient.GetNotificationDataAsync(Guid)"/>
        async Task<NotificationData> INotifyClient.GetNotificationDataAsync(Guid notificationId)
        {
            try
            {
                Notification notification =
                    await _notificationClient.GetNotificationByIdAsync(notificationId.ToString());

                return NotificationData.Success(notification);
            }
            catch (NotifyClientException exception)  // On failure this method is throwing exception
            {
                return NotificationData.Failure(exception.Message);
            }
        }
    }
}