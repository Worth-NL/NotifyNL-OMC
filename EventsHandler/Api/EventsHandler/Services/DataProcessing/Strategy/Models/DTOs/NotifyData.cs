// © 2023, Worth Systems.

using EventsHandler.Services.DataProcessing.Enums;
using Notify.Models.Responses;

namespace EventsHandler.Services.DataProcessing.Strategy.Models.DTOs
{
    /// <summary>
    /// The set of data which "Notify NL" will understand and use for a specific communication strategy (SMS or e-mail).
    /// </summary>
    internal readonly struct NotifyData
    {
        /// <inheritdoc cref="NotifyMethods"/>
        internal NotifyMethods NotificationMethod { get; }

        /// <summary>
        /// The SMS or e-mail details where the notification should be sent.
        /// </summary>
        internal string ContactDetails { get; }

        /// <summary>
        /// The template ID (existing on the "Notify NL" side) which should be used for notification.
        /// <para>
        ///   WARNING: The template ID should match selected notification method (SMS or e-mail).
        /// </para>
        /// </summary>
        internal string TemplateId { get; }

        /// <summary>
        /// The key-value pairs of notification data (values) to be fit into placeholders (keys)
        /// from the <see cref="TemplateResponse"/>.
        /// </summary>
        internal Dictionary<string, object> Personalization { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyData"/> struct.
        /// </summary>
        internal NotifyData(NotifyMethods notificationMethod, string contactDetails, string templateId, Dictionary<string, object> personalization)
        {
            this.NotificationMethod = notificationMethod;
            this.ContactDetails = contactDetails;
            this.TemplateId = templateId;
            this.Personalization = personalization;
        }
    }
}