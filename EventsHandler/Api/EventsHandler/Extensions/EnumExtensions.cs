// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotifyNL;
using EventsHandler.Services.Responding.Enums.v2;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for existing variety of enums.
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        /// Converts from <see cref="ProcessingResult"/> enum to <see cref="LogLevel"/> enum.
        /// </summary>
        /// <param name="processingResult">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        internal static LogLevel ConvertToLogLevel(this ProcessingResult processingResult)  // TODO: Missing code coverage
        {
            return processingResult switch
            {
                ProcessingResult.Success => LogLevel.Information,

                ProcessingResult.Skipped or
                ProcessingResult.Aborted => LogLevel.Warning,

                ProcessingResult.NotPossible or
                ProcessingResult.Failure => LogLevel.Error,
                _                        => LogLevel.None
            };
        }

        /// <summary>
        /// Converts from <see cref="NotificationTypes"/> enum to <see cref="NotifyMethods"/> enum.
        /// </summary>
        /// <param name="notificationType">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        internal static NotifyMethods ConvertToNotifyMethod(this NotificationTypes notificationType)
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
                !Enum.IsDefined(typeof(DeliveryStatuses), deliveryStatus))  // Can't determine based on the value
            {
                // FAILURE: Return fallback enum B
                return FeedbackTypes.Unknown;
            }

            // SUCCESS: Convert to enum B
            return deliveryStatus switch
            {
                DeliveryStatuses.Delivered => FeedbackTypes.Success,

                DeliveryStatuses.PermanentFailure or
                DeliveryStatuses.TemporaryFailure or
                DeliveryStatuses.TechnicalFailure => FeedbackTypes.Failure,

                // FAILURE: Return fallback enum B
                _ => FeedbackTypes.Info
            };
        }
    }
}
