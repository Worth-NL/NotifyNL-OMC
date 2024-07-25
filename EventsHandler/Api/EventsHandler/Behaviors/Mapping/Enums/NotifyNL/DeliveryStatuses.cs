// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Converters;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Enums.NotifyNL
{
    /// <summary>
    /// The delivery statuses of notifications (e-mail, SMS) returned by "Notify NL" Web API service.
    /// <para>
    ///   Source:
    /// 
    ///   <code>
    ///     https://[DOMAIN*]/using-notify/message-status/email
    ///   </code>
    /// 
    ///   * Domain where "Notify NL" Admin portal is deployed (e.g., "admin.notify.nl").
    /// </para>
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<DeliveryStatuses>))]
    public enum DeliveryStatuses
    {
        /// <summary>
        /// Default value.
        /// <para>
        ///   It might occur in case of problems with receiving notification delivery statuses from "Notify NL"
        ///   (for example if the API was recently changed).
        /// </para>
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        #region Email statuses (common with SMS as well)
        /// <summary>
        /// Notify has placed the message in a queue, ready to be sent to the provider.
        /// It should only remain in this state for a few seconds.
        /// </summary>
        [JsonPropertyName("created")]
        Created = 1,

        /// <summary>
        /// Notify has sent the message to the provider. The provider will try to deliver the
        /// message to the recipient for up to 72 hours. Notify is waiting for delivery information.
        /// </summary>
        [JsonPropertyName("sending")]
        Sending = 2,

        /// <summary>
        /// Email: The message was successfully delivered. Notify will not tell you if a user has opened or read a message.
        /// 
        /// <para>
        ///   SMS: The message was successfully delivered. If a recipient blocks your sender name or mobile number, your
        ///   message will still show as delivered.
        /// </para>
        /// </summary>
        [JsonPropertyName("delivered")]
        Delivered = 3,
        #endregion

        #region SMS statuses (exclusive)
        /// <summary>
        /// SMS: Notify is waiting for more delivery information. Notify received a callback from the provider but the
        /// recipient’s device has not yet responded. Another callback from the provider determines the final status of
        /// the text message.
        /// </summary>
        [JsonPropertyName("pending")]
        Pending = 4,

        /// <summary>
        /// SMS: The message was sent to an international number. The mobile networks in some countries do not provide
        /// any more delivery information. The Notify website displays this status as ‘Sent to an international number’.
        /// </summary>
        [JsonPropertyName("sent")]
        Sent = 5,
        #endregion

        #region Mail statuses (exclusive)
        /// <summary>
        /// Mail: Notify has sent the letter to the provider to be printed.
        /// </summary>
        [JsonPropertyName("accepted")]
        Accepted = 6,

        /// <summary>
        /// Mail: The provider has printed and dispatched the letter.
        /// </summary>
        [JsonPropertyName("received")]
        Received = 7,

        /// <summary>
        /// Mail: Sending cancelled. The letter will not be printed or dispatched.
        /// </summary>
        [JsonPropertyName("cancelled")]
        Cancelled = 8,
        #endregion

        #region Failures (common for Email, SMS, and Mail)
        /// <summary>
        /// Email: The provider could not deliver the message because the email address was wrong.
        /// You should remove these email addresses from your database.
        ///
        /// <para>
        ///   SMS: The provider could not deliver the message. This can happen if the phone number was wrong or if the network
        ///   operator rejects the message. If you’re sure that these phone numbers are correct, you should contact Notify support.
        ///   If not, you should remove them from your database. You’ll still be charged for text messages that cannot be delivered.
        /// </para>
        ///
        /// <para>
        ///   Mail: The provider cannot print the letter. Your letter will not be dispatched.
        /// </para>
        /// </summary>
        [JsonPropertyName("permanent-failure")]
        PermanentFailure = 9,

        /// <summary>
        /// Email: The provider could not deliver the message. This can happen when the recipient’s inbox
        /// is full or their anti-spam filter rejects your email. Check your content does not look like spam
        /// before you try to send the message again.
        /// <para>
        ///   Source:
        ///   <code>
        ///     https://www.gov.uk/service-manual/design/sending-emails-and-text-messages#protect-your-users-from-spam-and-phishing
        ///   </code>
        /// </para>
        ///
        /// <para>
        ///   SMS: The provider could not deliver the message. This can happen when the recipient’s phone is off, has no signal,
        ///   or their text message inbox is full. You can try to send the message again. You’ll still be charged for text messages
        ///   to phones that are not accepting messages.
        /// </para>
        /// </summary>
        [JsonPropertyName("temporary-failure")]
        TemporaryFailure = 10,

        /// <summary>
        /// Email: Your message was not sent because there was a problem between Notify and the provider.
        /// You’ll have to try sending your messages again.
        ///
        /// <para>
        ///   SMS: Your message was not sent because there was a problem between Notify and the provider.
        ///   You’ll have to try sending your messages again. You will not be charged for text messages that
        ///   are affected by a technical failure.
        /// </para>
        ///
        /// <para>
        ///   Mail: Notify had an unexpected error while sending the letter to our printing provider.
        /// </para>
        /// </summary>
        [JsonPropertyName("technical-failure")]
        TechnicalFailure = 11
        #endregion
    }
}