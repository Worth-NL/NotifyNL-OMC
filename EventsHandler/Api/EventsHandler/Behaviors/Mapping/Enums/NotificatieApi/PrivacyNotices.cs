﻿// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Converters;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Enums.NotificatieApi
{
    /// <summary>
    /// Gets the confidentiality notice of the current notification.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<PrivacyNotices>))]
    public enum PrivacyNotices
    {
        /// <summary>
        /// Default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Have to be confidential.
        /// </summary>
        [JsonPropertyName("vertrouwelijk")]
        Confidential = 1,

        /// <summary>
        /// Doesn't have to be confidential.
        /// </summary>
        [JsonPropertyName("openbaar")]
        NonConfidential = 2
    }
}