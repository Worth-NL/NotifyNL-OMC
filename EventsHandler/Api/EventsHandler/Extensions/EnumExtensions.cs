// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotifyNL;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.Responding.Enums.v2;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for existing variety of enums.
    /// </summary>
    internal static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<Enum, string> s_cachedEnumOptionNames = new();

        /// <summary>
        /// Gets the name of the <typeparamref name="TEnum"/> option.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enum">The enum option.</param>
        /// <returns>
        ///   The custom name of the given <typeparamref name="TEnum"/> option defined in attributes
        ///   or the default enum option name if the expected attributes are not existing.
        /// </returns>
        internal static string GetEnumName<TEnum>(this TEnum @enum)
            where TEnum : struct, Enum
        {
            return s_cachedEnumOptionNames.GetOrAdd(
                key: @enum,
                value: ExtractCustomEnumOptionName(@enum));

            static string ExtractCustomEnumOptionName(TEnum @enum)
            {
                string enumOptionName = @enum.ToString();

                string jsonPropertyName =
                    // Case #1: The name from JsonPropertyName attribute can be retrieved
                    typeof(TEnum).GetMember(enumOptionName).FirstOrDefault()?       // The enum option is defined in the given enum
                                 .GetCustomAttribute<JsonPropertyNameAttribute>()?  // The attribute JsonPropertyName is existing
                                 .Name
                    // Case #2: The name from JsonPropertyName attribute cannot be retrieved
                    ?? enumOptionName;

                return !string.IsNullOrWhiteSpace(jsonPropertyName)
                    ? jsonPropertyName
                    // Case #3: The name from JsonPropertyName attribute is empty or contains only white characters
                    : enumOptionName;
            }
        }

        /// <summary>
        /// Converts from <see cref="ProcessingStatus"/> enum to <see cref="LogLevel"/> enum.
        /// </summary>
        /// <param name="processingStatus">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        internal static LogLevel ConvertToLogLevel(this ProcessingStatus processingStatus)
        {
            return processingStatus switch
            {
                ProcessingStatus.Success => LogLevel.Information,

                ProcessingStatus.Skipped or
                ProcessingStatus.Aborted => LogLevel.Warning,

                ProcessingStatus.NotPossible or
                ProcessingStatus.Failure => LogLevel.Error,
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
