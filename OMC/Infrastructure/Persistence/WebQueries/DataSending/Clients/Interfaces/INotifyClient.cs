// © 2023, Worth Systems.

using Notify.Models;
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
        /// <param name="attachments">
        ///   The base64-encoded contents of 1 to 2 attachments to include with the letter, or <see langword="null"/>
        ///   for no attachments.
        /// </param>
        public Task<NotifySendResponse> SendLetterAsync(string templateId, Dictionary<string, object> personalization, string reference, IEnumerable<string>? attachments = null);

        /// <summary>
        /// Sends an already fully composed letter (a "precompiled" letter) asynchronously.
        /// </summary>
        /// <remarks>
        ///   Unlike <see cref="SendLetterAsync"/>, nothing is composed from a template here - the supplied
        ///   PDF <em>is</em> the letter, printed verbatim, which is what the "zonder oplegger" (no cover
        ///   sheet) requirement demands. Consequently the recipient's address is not passed separately: it
        ///   has to already sit in the PDF's own address window, and "Notify NL" reads it from there.
        /// </remarks>
        /// <param name="reference">
        ///   A unique identifier you can create if you need to. This reference
        ///   identifies a single unique notification or a batch of notifications.
        /// </param>
        /// <param name="pdfContents">The raw bytes of the PDF to be printed and posted.</param>
        /// <param name="postage">
        ///   The postage class - "first", "second" or "economy", or <see langword="null"/> to let
        ///   "Notify NL" apply its own default ("second").
        /// </param>
        public Task<NotifySendResponse> SendPrecompiledLetterAsync(string reference, byte[] pdfContents, string? postage = null);

        /// <summary>
        /// Sends a message to a user's digital message box (e.g., Berichtenbox) asynchronously.
        /// </summary>
        /// <param name="recipient">
        ///   The recipient's unique identifier (such as a BSN or other user key) to deliver the message to.
        /// </param>
        /// <param name="message">
        ///   The main content or body of the message.
        /// </param>
        /// <param name="messageType">
        ///   The type of the message, as reported by OpenVTB.
        /// </param>
        /// <param name="subject">
        ///   The subject line or title of the message.
        /// </param>
        /// <param name="attachments">
        ///   The collection of attachments (base64-encoded file content and filename) to include with the
        ///   message. 0 to 2 attachments are allowed.
        /// </param>
        /// <param name="reference">
        ///   A unique identifier you can create if you need to. This reference
        ///   identifies a single unique notification or a batch of notifications.
        /// </param>
        public Task<NotifySendResponse> SendMessageBoxNotificationAsync(
            string recipient,
            string message,
            string messageType,
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