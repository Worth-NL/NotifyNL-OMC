// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Enums.v2;
using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for existing variety of enums.
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        /// Converts from <see cref="NotificationTypes"/> enum to <see cref="NotifyMethods"/> enum.
        /// </summary>
        /// <param name="notificationType">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        internal static NotifyMethods ConvertToNotifyMethod(this NotificationTypes notificationType)
        {
            // Get value of the enum A
            int notificationTypeValue = (int)notificationType;

            // Ensure if the conversion is possible
            return Enum.IsDefined(typeof(NotifyMethods), notificationTypeValue)
                // SUCCESS: Convert to enum B by direct value-to-enum casting
                ? (NotifyMethods)notificationTypeValue
                // FAILURE: Return fallback enum B
                : NotifyMethods.None;
        }
        
        /// <summary>
        /// Converts from <see cref="DeliveryStatuses"/> enum to <see cref="FeedbackTypes"/> enum.
        /// </summary>
        /// <param name="deliveryStatus">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        internal static FeedbackTypes ConvertToFeedbackStatus(this DeliveryStatuses deliveryStatus)
        {
            if (deliveryStatus == DeliveryStatuses.Unknown ||
                !Enum.IsDefined(typeof(DeliveryStatuses), (int)deliveryStatus))  // Can't determine based on the value
            {
                return FeedbackTypes.Unknown;
            }

            return deliveryStatus switch
            {
                DeliveryStatuses.Delivered
                    => FeedbackTypes.Success,
                
                DeliveryStatuses.PermanentFailure or DeliveryStatuses.TemporaryFailure or DeliveryStatuses.TechnicalFailure
                    => FeedbackTypes.Failure,
                
                _ => FeedbackTypes.Info
            };
        }
    }
}
