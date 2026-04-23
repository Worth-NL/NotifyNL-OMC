// © 2023, Worth Systems.

using Common.Extensions;
using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using EventsHandler.Services.Responding.Enums;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using EventsHandler.Services.Validation.Interfaces;
using System.Reflection;
using ZhvModels.Enums;
using ZhvModels.Mapping.Helpers;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using EventAttributes = ZhvModels.Mapping.Models.POCOs.NotificatieApi.EventAttributes;

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
        /// Checks the result of deserialization of the <see cref="NotificationEvent"/> notification.
        /// </summary>
        HealthCheck IValidationService<NotificationEvent>.Validate(ref NotificationEvent notification)
        {
            BaseEnhancedDetails? details = null;

            if (notification.IsInvalidEvent(out int[] invalidPropertiesIndices))
            {
                details = ReportInvalidPropertiesNames(ref notification, invalidPropertiesIndices);
                notification.Details = details;
                return HealthCheck.ERROR_Invalid;
            }

            if (HasEmptyAttributes(ref notification, out HealthCheck healthCheck, ref details))
            {
                notification.Details = details!;
                return healthCheck;
            }

            if (ContainsAnyOrphans(ref notification, out healthCheck, ref details))
            {
                notification.Details = details!;
                return healthCheck;
            }

            notification.Details = InfoDetails.Empty;
            return HealthCheck.OK_Valid;
        }

        /// <summary>
        /// Gets the names of invalid <see cref="NotificationEvent"/> properties.
        /// </summary>
        private BaseEnhancedDetails ReportInvalidPropertiesNames(ref NotificationEvent notification, IEnumerable<int> invalidPropertiesIndices)
        {
            NotificationEvent currentNotification = notification;

            return this._detailsBuilder.Get<ErrorDetails>(
                Reasons.MissingProperties_Notification,
                JoinWithComma(invalidPropertiesIndices
                    .Select(index => currentNotification.Properties
                        .GetPropertyDutchName(currentNotification.Properties[index]))));
        }

        /// <summary>
        /// Determines whether specific <see cref="EventAttributes"/> properties are missing.
        /// </summary>
        private bool HasEmptyAttributes(
            ref NotificationEvent notification,
            out HealthCheck healthCheck,
            ref BaseEnhancedDetails? details)
        {
            List<string>? missingPropertiesNames = null;

            PropertiesMetadata specificProperties = notification.Attributes.Properties(notification.Channel);

            for (int index = 0; index < specificProperties.Count; index++)
            {
                PropertyInfo currentProperty = specificProperties[index];

                if (notification.Attributes.NotInitializedProperty(currentProperty))
                {
                    (missingPropertiesNames ??= new List<string>(specificProperties.Count))
                        .Add(specificProperties.GetPropertyDutchName(currentProperty));
                }
            }

            if (missingPropertiesNames.HasAny())
            {
                healthCheck = HealthCheck.ERROR_Invalid;
                details = this._detailsBuilder.Get<ErrorDetails>(
                    Reasons.MissingProperties_Attributes,
                    JoinWithComma(missingPropertiesNames!));

                return true;
            }

            healthCheck = HealthCheck.OK_Valid;
            details = InfoDetails.Empty;

            return false;
        }

        /// <summary>
        /// Determines whether there are any unknown JSON properties.
        /// </summary>
        private bool ContainsAnyOrphans(
            ref NotificationEvent notification,
            out HealthCheck healthCheck,
            ref BaseEnhancedDetails? details)
        {
            if (notification.Orphans.Count > 0)
            {
                healthCheck = HealthCheck.ERROR_Invalid;
                details = this._detailsBuilder.Get<InfoDetails>(
                    Reasons.UnexpectedProperties_Notification,
                    GetSeparatedKeys(notification.Orphans));

                return true;
            }

            if (notification.Attributes.Orphans.Count > 0)
            {
                healthCheck = HealthCheck.OK_Inconsistent;
                details = this._detailsBuilder.Get<InfoDetails>(
                    Reasons.UnexpectedProperties_Attributes,
                    GetSeparatedKeys(notification.Attributes.Orphans));

                return true;
            }

            healthCheck = HealthCheck.OK_Valid;
            details = InfoDetails.Empty;

            return false;
        }

        /// <summary>
        /// Gets only the keys from a dictionary.
        /// </summary>
        private static string GetSeparatedKeys(IDictionary<string, object> dictionary)
        {
            return JoinWithComma(dictionary.Select(x => x.Key));
        }

        /// <summary>
        /// Joins a collection into a comma-separated string.
        /// </summary>
        private static string JoinWithComma<T>(IEnumerable<T> collection)
        {
            const string separator = ", ";
            return string.Join(separator, collection);
        }
    }
}