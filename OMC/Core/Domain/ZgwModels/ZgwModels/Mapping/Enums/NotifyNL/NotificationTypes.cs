// © 2024, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Converters;

namespace ZgwModels.Mapping.Enums.NotifyNL
{
    /// <summary>
    /// The notification types returned by "Notify NL" Web API service.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<NotificationTypes>))]
    public enum NotificationTypes
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Notification type: e-mail.
        /// </summary>
        [JsonPropertyName("email")]
        Email = 1,

        /// <summary>
        /// Notification type: SMS.
        /// </summary>
        [JsonPropertyName("sms")]
        Sms = 2,

        /// <summary>
        /// Notification type: MOBB / Berichtenbox message.
        /// </summary>
        /// <remarks>
        ///   TODO (first-version, unconfirmed): "messagebox" is a temporary stand-in for whatever delivery-receipt
        ///   `type` value NotifyNL actually returns for a MOBB send - not yet confirmed. Update once known;
        ///   the "safe" JSON converter on this enum means a wrong guess just falls back to <see cref="Unknown"/>
        ///   rather than throwing, so this is low-risk to leave as a placeholder for now.
        /// </remarks>
        [JsonPropertyName("messagebox")]
        Mobb = 3
    }
}