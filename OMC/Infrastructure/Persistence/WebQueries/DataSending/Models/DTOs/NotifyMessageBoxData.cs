
namespace WebQueries.DataSending.Models.DTOs
{
    /// <summary>
    /// The set of data which "Notify NL" will understand and use for a specific communication strategy.
    /// </summary>
    public struct NotifyMessageBoxData
    {
        /// <summary>
        /// The SMS or e-mail details where the notification should be sent.
        /// </summary>
        public string Sender { get; set; }
    }
}