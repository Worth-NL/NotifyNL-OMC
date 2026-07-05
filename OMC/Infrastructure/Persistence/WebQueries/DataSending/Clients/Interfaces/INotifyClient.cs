// © 2023, Worth Systems.

using System.Net.Mail;
using WebQueries.DataSending.Models.Reponses;

namespace WebQueries.DataSending.Clients.Interfaces
{
    /// <summary>
    /// The common interface to be used either with business or test/mock notification client.
    /// </summary>
    public interface INotifyClient
    {
        /// <summary>
        /// Sends the e-mail asynchronously.
        /// </summary>
        /// <param name="emailAddress">The email address of the recipient.</param>
        /// <param name="templateId">The template identifier.</param>
        /// <param name="personalization">
        ///   The personalization of the template.
        ///   <para>
        ///     NOTE: The parameters in the personalization argument must match the placeholder fields in
        ///     the actual template. The API notification client ignores any extra fields in the method.
        ///   </para>
        /// </param>
        /// <param name="reference">
        ///   A unique identifier you can create if you need to. This reference
        ///   identifies a single unique notification or a batch of notifications.
        /// </param>
        public Task<NotifySendResponse> SendEmailAsync(string emailAddress, string templateId, Dictionary<string, object> personalization, string reference);

        /// <summary>
        /// Sends the text message (SMS) asynchronously.
        /// </summary>
        /// <param name="mobileNumber">The phone number of the recipient of the text message.</param>
        /// <param name="templateId">The template identifier.</param>
        /// <param name="personalization">
        ///   The personalization of the template.
        ///   <para>
        ///     NOTE: The parameters in the personalization argument must match the placeholder fields in
        ///     the actual template. The API notification client ignores any extra fields in the method.
        ///   </para>
        /// </param>
        /// <param name="reference">
        ///   A unique identifier you can create if you need to. This reference
        ///   identifies a single unique notification or a batch of notifications.
        /// </param>
        public Task<NotifySendResponse> SendSmsAsync(string mobileNumber, string templateId, Dictionary<string, object> personalization, string reference);

        /// <summary>
        /// Sends the text message (SMS) asynchronously.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        /// <param name="personalization">
        ///   The personalization of the template.
        ///   <para>
        ///     NOTE: The parameters in the personalization argument must match the placeholder fields in
        ///     the actual template. The API notification client ignores any extra fields in the method.
        ///   </para>
        /// </param>
        /// <param name="reference">
        ///   A unique identifier you can create if you need to. This reference
        ///   identifies a single unique notification or a batch of notifications.
        /// </param>
        public Task<NotifySendResponse> SendLetterAsync(string templateId, Dictionary<string, object> personalization, string reference);

        /// <summary>
        /// Sends a message to a user's digital message box (e.g., Berichtenbox) asynchronously.
        /// </summary>
        /// <param name="sender">
        ///   The identifier or name of the sender originating the message.
        /// </param>
        /// <param name="recipient">
        ///   The recipient's unique identifier (such as a BSN or other user key) to deliver the message to.
        /// </param>
        /// <param name="message">
        ///   The main content or body of the message.
        /// </param>
        /// <param name="subject">
        ///   The subject line or title of the message.
        /// </param>
        /// <param name="attachments">
        ///   The collection of attachments to include with the message.
        ///   <para>
        ///     TODO: The current Attachment type is incorrect – it reflects attachments as fetched from
        ///     the ZGW Berichten API, not the actual PDF/file payloads. This needs to be updated to the
        ///     correct file/PDF type.
        ///   </para>
        /// </param>
        /// <param name="reference">
        ///   A unique identifier you can create if you need to. This reference
        ///   identifies a single unique notification or a batch of notifications.
        /// </param>
        public Task<NotifySendResponse> SendMessageBoxNotificationAsync(
            string sender,
            string recipient,
            string message,
            string subject,
            IEnumerable<Attachment> attachments,
            string reference);

        /// <summary>
        /// Generates a preview version of a template.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        /// <param name="personalization">
        ///   The personalization of the template.
        ///   <para>
        ///     NOTE: The parameters in the personalization argument must match the placeholder fields in
        ///     the actual template. The API notification client ignores any extra fields in the method.
        ///   </para>
        /// </param>
        public Task<NotifyTemplateResponse> GenerateTemplatePreviewAsync(string templateId, Dictionary<string, object> personalization);

        /// <summary>
        /// Fetches the notification by id from NotifyNL.
        /// </summary>
        /// <param name="notificationId">The notification identifier.</param>
        public Task<NotificationData> GetNotificationDataAsync(Guid notificationId);
    }
}