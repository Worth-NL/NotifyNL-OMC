// © 2023, Worth Systems.

namespace EventsHandler.Services.DataSending.Clients.Interfaces
{
    /// <summary>
    /// The common interface to be used either with business or test/mock notification client.
    /// </summary>
    public interface INotifyClient
    {
        /// <summary>
        /// Sends the text message (SMS) asynchronously.
        /// </summary>
        /// <param name="mobileNumber">The mobile (phone) number.</param>
        /// <param name="templateId">The template identifier.</param>
        /// <param name="personalization">The personalization.</param>
        internal Task<bool> SendSmsAsync(string mobileNumber, string templateId, Dictionary<string, object> personalization);

        /// <summary>
        /// Sends the e-mail asynchronously.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="templateId">The template identifier.</param>
        /// <param name="personalization">The personalization.</param>
        internal Task<bool> SendEmailAsync(string emailAddress, string templateId, Dictionary<string, object> personalization);
    }
}