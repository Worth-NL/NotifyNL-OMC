// © 2023, Worth Systems.

using Notify.Models.Responses;
using ZhvModels.Enums;

namespace EventsHandler.Models.DTOs.Processing
{
    /// <summary>
    /// The set of data which "Notify NL" will understand and use for a specific communication strategy.
    /// </summary>
    internal readonly struct NotifyData
    {
        /// <inheritdoc cref="NotifyMethods"/>
        internal NotifyMethods NotificationMethod { get; }

        /// <summary>
        /// The SMS or e-mail details where the notification should be sent.
        /// </summary>
        internal string ContactDetails { get; } = string.Empty;

        /// <summary>
        /// The template ID (existing on the "Notify NL" side) which should be used for notification.
        /// <para>
        ///   WARNING: The template ID should match selected notification method (SMS or e-mail).
        /// </para>
        /// </summary>
        internal Guid TemplateId { get; } = Guid.Empty;

        /// <summary>
        /// The key-value pairs of notification data (values) to be fit into placeholders (keys)
        /// from the <see cref="TemplateResponse"/>.
        /// </summary>
        internal Dictionary<string, object> Personalization { get; } = [];

        /// <inheritdoc cref="NotifyReference"/>
        internal NotifyReference Reference { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyData"/> struct.
        /// </summary>
        internal NotifyData(NotifyMethods notificationMethod, string contactDetails, Guid templateId,
            Dictionary<string, object> personalization, NotifyReference reference)
            : this(notificationMethod)
        {
            ContactDetails = contactDetails;
            TemplateId = templateId;
            Personalization = personalization;
            Reference = reference;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyData"/> struct.
        /// </summary>
        internal NotifyData(NotifyMethods notificationMethod)
        {
            NotificationMethod = notificationMethod;
        }
    }
}