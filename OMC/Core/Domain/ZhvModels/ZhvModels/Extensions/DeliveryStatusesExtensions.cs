// // © 2024, Worth Systems.

using Common.Enums.Responding;
using ZhvModels.Mapping.Enums.NotifyNL;

namespace ZhvModels.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DeliveryStatuses"/>.
    /// </summary>
    public static class DeliveryStatusesExtensions
    {
        /// <summary>
        /// Converts from <see cref="FeedbackTypes"/> enum to <see cref="deliveryStatus"/> enum.
        /// </summary>
        /// <param name="deliveryStatus">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        public static FeedbackTypes ConvertToFeedbackStatus(this DeliveryStatuses deliveryStatus)
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