// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;

namespace EventsHandler.Behaviors.Mapping.Enums.NotifyNL
{
    /// <summary>
    /// Notification types returned by "NotifyNL" API web service.
    /// </summary>
    internal enum NotificationTypes
    {
        /// <inheritdoc cref="DistributionChannels.Email"/>
        [JsonPropertyName("email")]
        Email = DistributionChannels.Email,  // 2
        
        /// <inheritdoc cref="DistributionChannels.Sms"/>
        [JsonPropertyName("sms")]
        Sms = DistributionChannels.Sms  // 3
    }
}
