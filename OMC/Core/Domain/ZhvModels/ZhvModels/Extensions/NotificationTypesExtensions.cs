// © 2024, Worth Systems.

using Common.Enums.Processing;
using ZhvModels.Mapping.Enums.NotifyNL;

namespace ZhvModels.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="NotificationTypes"/>.
    /// </summary>
    public static class NotificationTypesExtensions
    {
        /// <summary>
        /// Converts from <see cref="NotificationTypes"/> enum to <see cref="NotifyMethods"/> enum.
        /// </summary>
        /// <param name="notificationType">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        public static NotifyMethods ConvertToNotifyMethod(this NotificationTypes notificationType)
        {
            // SUCCESS: Convert to enum B
            return notificationType switch
            {
                NotificationTypes.Email => NotifyMethods.Email,

                NotificationTypes.Sms => NotifyMethods.Sms,

                // FAILURE: Return fallback enum B
                _ => NotifyMethods.None
            };
        }
    }
}