// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
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
        /// Checks the result of deserialization of the <see cref="NotificationEvent"/> model (and it's nested components).
        /// </summary>
        /// <param name="model">The notification model to be implicitly validated.</param>
        HealthCheck IValidationService<NotificationEvent>.Validate(ref NotificationEvent model)
        {
            // 1. Problem with deserialization of the model
            if (NotificationEvent.IsDefault(model))
            {
                return HealthCheck.ERROR_Invalid;
            }

            // 2. Missing values of POCO model (optional) properties
            if (HasEmptyAttributes(ref model, out HealthCheck healthCheck))
            {
                return healthCheck;
            }

            // 3. Additional JSON properties not included in POCO models
            return ContainsAnyOrphans(ref model, out healthCheck)
                ? healthCheck
                : HealthCheck.OK_Valid;
        }

        #region Helper methods
        /// <summary>
        /// Determines whether any (optional) property from <see cref="EventAttributes"/> POCO model wasn't properly deserialized.
        /// <para>
        ///   <see cref="EventAttributes"/> sub-model can potentially contain undefined, missing or unmatching properties
        ///   (because different APIs may define their own key-value pairs as "kenmerken" [attributes / characteristics]).
        /// </para>
        /// </summary>
        /// <param name="notification">The model to be validated.</param>
        /// <param name="healthCheck">The final health check.</param>
        /// <returns>
        ///   <see langword="true"/> if value of any <see cref="EventAttributes"/> property is missing (weren't mapped as expected); otherwise, <see langword="false"/>.
        /// </returns>
        private bool HasEmptyAttributes(ref NotificationEvent notification, out HealthCheck healthCheck)
        {
            List<string>? missingPropertiesNames = null;

            for (int index = 0; index < notification.Attributes.Properties.Count; index++)
            {
                PropertyInfo currentProperty = notification.Attributes.Properties[index];

                if (notification.Attributes.NotInitializedProperty(currentProperty))
                {
                    // Stores Dutch names of missing properties
                    (missingPropertiesNames ??= new List<string>(notification.Attributes.Properties.Count))  // Lazy initialization
                        .Add(notification.Attributes.Properties.GetPropertyDutchName(currentProperty));
                }
            }

            // Failure: Missing dynamic EventAttributes data (check with third-party API if this is desired)
            if (missingPropertiesNames.HasAny())
            {
                healthCheck = HealthCheck.OK_Inconsistent;
                notification.Details = this._detailsBuilder.Get<InfoDetails>(
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
        /// <param name="notification">The model to be validated.</param>
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
            // Failure: Additional JSON properties for EventAttributes (check with third-party API if this is desired)

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