// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Converters;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Enums.NotifyNL
{
    /// <summary>
    /// The notification types returned by "Notify NL" Web API service.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<NotificationTypes>))]
    internal enum NotificationTypes
    {
        /// <summary>
        /// Default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,
        
        /// <summary>
        /// Notification type: e-mail.
        /// </summary>
        [JsonPropertyName("email")]
        Email = NotifyMethods.Email,  // 2
        
        /// <summary>
        /// Notification type: SMS.
        /// </summary>
        [JsonPropertyName("sms")]
        Sms = NotifyMethods.Sms  // 3
    }
}