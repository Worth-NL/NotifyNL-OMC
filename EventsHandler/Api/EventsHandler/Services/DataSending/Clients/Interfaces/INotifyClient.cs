// © 2023, Worth Systems.

using EventsHandler.Services.DataSending.Responses;

namespace EventsHandler.Services.DataSending.Clients.Interfaces
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
        /// <remarks>
        /// NOTE: Might throw exceptions from the external libraries.
        /// </remarks>
        internal Task<NotifySendResponse> SendEmailAsync(string emailAddress, string templateId, Dictionary<string, object> personalization, string reference);

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
        /// <remarks>
        /// NOTE: Might throw exceptions from the external libraries.
        /// </remarks>
        internal Task<NotifySendResponse> SendSmsAsync(string mobileNumber, string templateId, Dictionary<string, object> personalization, string reference);
        
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
        /// <remarks>
        /// NOTE: Might throw exceptions from the external libraries.
        /// </remarks>
        internal Task<NotifyTemplateResponse> GenerateTemplatePreviewAsync(string templateId, Dictionary<string, object> personalization);
    }
}