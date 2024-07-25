// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Helpers;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Results.Builder.Interface;
using EventsHandler.Behaviors.Responding.Results.Enums;
using EventsHandler.Extensions;
using EventsHandler.Services.Validation.Interfaces;
using System.Reflection;
using EventAttributes = EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi.EventAttributes;

namespace EventsHandler.Services.Validation
{
    /// <inheritdoc cref="IValidationService{TModel}"/>
    internal sealed class NotificationValidator : IValidationService<NotificationEvent>
    {
        private readonly IDetailsBuilder _detailsBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationValidator"/> class.
        /// </summary>
        public NotificationValidator(IDetailsBuilder detailsBuilder)
        {
            this._detailsBuilder = detailsBuilder;
        }

        /// <summary>
        /// Checks the result of deserialization of the <see cref="NotificationEvent"/> notification (and it's nested components).
        /// </summary>
        /// <param name="notification">The notification model to be implicitly validated.</param>
        HealthCheck IValidationService<NotificationEvent>.Validate(ref NotificationEvent notification)
        {
            // 1. Problem with deserialization of the event notification (common properties)
            if (notification.IsInvalidEvent(out int[] invalidPropertiesIndices))
            {
                return ReportInvalidPropertiesNames(ref notification, invalidPropertiesIndices);
            }

            // 2. Missing values of POCO notification (optional) properties
            if (HasEmptyAttributes(ref notification, out HealthCheck healthCheck))
            {
                return healthCheck;
            }

            // 3. Additional JSON properties not included in POCO models
            return ContainsAnyOrphans(ref notification, out healthCheck)
                ? healthCheck
                : HealthCheck.OK_Valid;
        }

        #region Helper methods
        /// <summary>
        /// Gets the names of invalid <see cref="NotificationEvent"/> properties.
        /// </summary>
        /// <param name="notification">The notification to which invalid properties belongs.</param>
        /// <param name="invalidPropertiesIndices">The collection of indices of invalid properties.</param>
        /// <returns>
        ///   The comma-separated names of invalid <see cref="NotificationEvent"/> properties.
        /// </returns>
        private HealthCheck ReportInvalidPropertiesNames(ref NotificationEvent notification, IEnumerable<int> invalidPropertiesIndices)
        {
            NotificationEvent currentNotification = notification;

            notification.Details = this._detailsBuilder.Get<ErrorDetails>(
                Reasons.MissingProperties_Notification,
                JoinWithComma(invalidPropertiesIndices
                    .Select(index => currentNotification.Properties  // NOTE: Initialization of notification metadata (or reusing an already initialized ones)
                        .GetPropertyDutchName(currentNotification.Properties[index]))));

            return HealthCheck.ERROR_Invalid;
        }

        /// <summary>
        /// Determines whether specific properties from <see cref="EventAttributes"/> POCO notification wasn't properly deserialized.
        /// <para>
        ///   DETAILS: <see cref="EventAttributes"/> sub-notification can potentially contain undefined, missing or unmatching
        ///   properties (because different Web APIs may define their own key-value pairs as "kenmerken" [attributes /
        ///   characteristics], and because different types of notifications contains different dynamic properties e.g.,
        ///   for the purpose of business processing: cases, objects, decisions, etc...).
        /// </para>
        /// </summary>
        /// <remarks>
        ///   NOTE: Only the properties related to the specific notification type (defined in <see cref="NotificationEvent"/>.<see cref="Channels"/>)
        ///   will be validated, to not produce any unnecessary validation noise (by checking properties which are not
        ///   essential anyway for this specific business case).
        /// </remarks>
        /// <param name="notification">The notification to be validated.</param>
        /// <param name="healthCheck">The final health check.</param>
        /// <returns>
        ///   <see langword="true"/> if value of any specific <see cref="EventAttributes"/> property is missing
        ///   (weren't mapped as expected); otherwise, <see langword="false"/>.
        /// </returns>
        private bool HasEmptyAttributes(ref NotificationEvent notification, out HealthCheck healthCheck)
        {
            List<string>? missingPropertiesNames = null;

            // Determine properties only for this specific notification type (e.g., cases, objects, decisions, etc.)
            PropertiesMetadata specificProperties = notification.Attributes.Properties(notification.Channel);

            for (int index = 0; index < specificProperties.Count; index++)
            {
                PropertyInfo currentProperty = specificProperties[index];

                // Check if the property is null
                if (notification.Attributes.NotInitializedProperty(currentProperty))
                {
                    // Stores Dutch names of missing properties
                    (missingPropertiesNames ??= new List<string>(specificProperties.Count))  // Lazy initialization
                        .Add(specificProperties.GetPropertyDutchName(currentProperty));
                }
            }

            // Failure: Missing specific EventAttributes data (necessary for this notification type)
            if (missingPropertiesNames.HasAny())
            {
                healthCheck = HealthCheck.ERROR_Invalid;
                notification.Details = this._detailsBuilder.Get<ErrorDetails>(
                    Reasons.MissingProperties_Attributes,
                    JoinWithComma(missingPropertiesNames!));

                return true;
            }

            // Success: Everything is good
            healthCheck = HealthCheck.OK_Valid;
            notification.Details = InfoDetails.Empty;

            return false;
        }

        /// <summary>
        /// Determines whether there are any JSON properties that couldn't be matched with
        /// <see cref="NotificationEvent"/> or <see cref="EventAttributes"/> models properties.
        /// <para>
        ///   Eventual mismatches should be reported back as errors to not risk loosing any important data in the process.
        /// </para>
        /// </summary>
        /// <param name="notification">The notification to be validated.</param>
        /// <param name="healthCheck">The final health check.</param>
        /// <returns>
        ///   <see langword="true"/> if any additional unmatching JSON property was found; otherwise, <see langword="false"/>.
        /// </returns>
        private bool ContainsAnyOrphans(ref NotificationEvent notification, out HealthCheck healthCheck)
        {
            // Failure: Additional JSON properties for NotificationEvent
            if (notification.Orphans.Count > 0)
            {
                healthCheck = HealthCheck.ERROR_Invalid;
                notification.Details = this._detailsBuilder.Get<InfoDetails>(
                    Reasons.UnexpectedProperties_Notification,
                    GetSeparatedKeys(notification.Orphans));

                return true;
            }

            // Inconsistency: Additional JSON properties for EventAttributes
            if (notification.Attributes.Orphans.Count > 0)
            {
                healthCheck = HealthCheck.OK_Inconsistent;
                notification.Details = this._detailsBuilder.Get<InfoDetails>(
                    Reasons.UnexpectedProperties_Attributes,
                    GetSeparatedKeys(notification.Attributes.Orphans));

                return true;
            }

            // Success: Everything is good
            healthCheck = HealthCheck.OK_Valid;
            notification.Details = InfoDetails.Empty;

            return false;
        }

        /// <summary>
        /// Gets only the keys from a given dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to be decomposed into keys.</param>
        /// <returns>
        ///   Comma-separated <see langword="string"/> representing JSON keys.
        /// </returns>
        private static string GetSeparatedKeys(IDictionary<string, object> dictionary)
        {
            return JoinWithComma(dictionary.Select(keyValuePair => keyValuePair.Key));
        }

        private static string JoinWithComma<T>(IEnumerable<T> collection)
        {
            const string separator = ", ";

            return string.Join(separator, collection);
        }
        #endregion
    }
}