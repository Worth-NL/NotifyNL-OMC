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
        Sms = 2
    }
}