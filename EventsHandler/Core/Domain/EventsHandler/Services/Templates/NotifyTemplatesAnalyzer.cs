// © 2023, Worth Systems.

using Common.Extensions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.Templates.Interfaces;
using Notify.Models.Responses;
using System.Text.RegularExpressions;

namespace EventsHandler.Services.Templates
{
    /// <inheritdoc cref="ITemplatesService{TTemplate, TModel}"/>
    internal sealed partial class NotifyTemplatesAnalyzer : ITemplatesService<TemplateResponse, NotificationEvent>  // NOTE: "partial" is introduced by the new RegEx generation approach
    {
        #region Regular Expressions
        // Group names
        private const string PlaceholderGroupName = "Placeholder";

        // RegEx patterns
        private static readonly Regex s_templatePlaceholdersRegex = GenerateTemplatePlaceholdersRegex();

        // RegEx generation (.NET 7 feature)
        [GeneratedRegex($"\\(\\((?<{PlaceholderGroupName}>\\w+)\\)\\)", RegexOptions.Compiled)]
        private static partial Regex GenerateTemplatePlaceholdersRegex();
        #endregion

        internal const string ValueNotAvailable = "Niet beschikbaar";

        private static readonly Dictionary<string, dynamic> s_emptyDictionary = [];

        /// <inheritdoc cref="ITemplatesService{TTemplate, TModel}.GetPlaceholders(TTemplate)"/>
        string[] ITemplatesService<TemplateResponse, NotificationEvent>.GetPlaceholders(TemplateResponse template)
        {
            // Invalid template
            if (string.IsNullOrEmpty(template.subject) ||
                string.IsNullOrEmpty(template.body))
            {
                return [];
            }

            MatchCollection matches = s_templatePlaceholdersRegex.Matches(template.subject + template.body);

            // No placeholders found
            if (!matches.HasAny())
            {
                return [];
            }

            // Return placeholders
            return matches.Select(match => match.Groups[PlaceholderGroupName].Value)  // Skip brackets surrounding ((placeholders))
                          .Distinct()                                                 // Get only unique values
                          .ToArray();
        }

        /// <inheritdoc cref="ITemplatesService{TTemplate, TModel}.MapPersonalization(string[], TModel)"/>
        Dictionary<string, dynamic> ITemplatesService<TemplateResponse, NotificationEvent>.MapPersonalization(string[] placeholders, NotificationEvent notification)
        {
            if (!placeholders.HasAny())
            {
                return s_emptyDictionary;
            }

            Dictionary<string, dynamic> mappedPlaceholders = [];

            for (int index = 0; index < placeholders.Length; index++)
            {
                string currentPlaceholder = placeholders[index];

                mappedPlaceholders.Add(currentPlaceholder, GetValue(notification, currentPlaceholder));
            }

            return mappedPlaceholders;
        }

        /// <summary>
        /// Gets the value of POCO model property corresponding to the received placeholder (key).
        /// </summary>
        /// <param name="notification">The notification from which corresponding value should be extracted.</param>
        /// <param name="placeholder">The placeholder (from <see cref="TemplateResponse"/>) used as a "key" (in Dutch).</param>
        /// <returns>
        ///   The value matching to the given placeholder (key); default value if nothing was found.
        /// </returns>
        private static object GetValue(NotificationEvent notification, string placeholder)
        {
            // FASTER + MORE PROBABLE: Check the Orphans from the nested EventAttributes POCO model
            if (notification.Attributes.Orphans.TryGetValue(placeholder, out object? propertyValue))
            {
                return propertyValue;
            }

            // FASTER + LESS PROBABLE: Check the Orphans from the root NotificationEvent POCO model
            if (notification.Orphans.TryGetValue(placeholder, out propertyValue))
            {
                return propertyValue;
            }

            // BIT SLOWER + MORE PROBABLE: Check the instance properties of EventAttributes POCO model
            if (notification.Attributes.Properties(Channels.Cases)
                    .TryGetPropertyValueByDutchName(placeholder, notification.Attributes, out propertyValue) ||

                notification.Attributes.Properties(Channels.Objects)
                    .TryGetPropertyValueByDutchName(placeholder, notification.Attributes, out propertyValue) ||

                notification.Attributes.Properties(Channels.Decisions)
                    .TryGetPropertyValueByDutchName(placeholder, notification.Attributes, out propertyValue))
            {
                return propertyValue;
            }

            // BIT SLOWER + LESS PROBABLE: Check the instance properties of NotificationEvent POCO model
            if (notification.Properties.TryGetPropertyValueByDutchName(placeholder, notification, out propertyValue))
            {
                return propertyValue;
            }

            // Fallback: Always map some value into expected placeholder (to avoid "NotifyClientException")
            return ValueNotAvailable;  // Default value
        }
    }
}