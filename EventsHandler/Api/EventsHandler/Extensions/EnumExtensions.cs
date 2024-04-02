using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for existing variety of enums.
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        /// Converts to <see cref="NotificationTypes"/> enum into <see cref="NotifyMethods"/> enum.
        /// </summary>
        /// <param name="notificationTypes">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        internal static NotifyMethods ConvertToNotifyMethod(this NotificationTypes notificationTypes)
        {
            // Get value of the enum A
            int notificationTypeValue = (int)notificationTypes;

            // Ensure if the conversion is possible
            return Enum.IsDefined(typeof(NotifyMethods), notificationTypeValue)
                // SUCCESS: Convert to enum B by direct value-to-enum casting
                ? (NotifyMethods)notificationTypeValue
                // FAILURE: Return fallback enum B
                : NotifyMethods.None;
        }
    }
}
