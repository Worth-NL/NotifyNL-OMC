﻿// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Converters;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Enums.OpenKlant
{
    /// <summary>
    /// The distribution channel. The method how the recipient (a citizen) want to be notified.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<DistributionChannels>))]
    public enum DistributionChannels
    {
        /// <summary>
        /// Default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Communication method: None => "do not notify me".
        /// </summary>
        [JsonPropertyName("geen")]
        None = 1,

        /// <summary>
        /// Communication method: SMS.
        /// </summary>
        [JsonPropertyName("sms")]
        Sms = 2,

        /// <summary>
        /// Communication method: e-mail.
        /// </summary>
        [JsonPropertyName("email")]
        Email = 3,

        /// <summary>
        /// Communication method: SMS and e-mail.
        /// </summary>
        [JsonPropertyName("beiden")]
        Both = 4
    }
}