// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.OpenKlant;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.DataProcessing.Enums
{
    /// <summary>
    /// The notification method used by "Notify NL" API Client to communicate with a citizen.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]  // NOTE: Simple JSON converter to display enum options in Swagger UI
    public enum NotifyMethods
    {
        /// <inheritdoc cref="DistributionChannels.None"/>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        None = DistributionChannels.None,  // 1

        /// <inheritdoc cref="DistributionChannels.Email"/>
        [JsonPropertyName("email")]
        Email = DistributionChannels.Email,  // 2

        /// <inheritdoc cref="DistributionChannels.Sms"/>
        [JsonPropertyName("sms")]
        Sms = DistributionChannels.Sms  // 3
    }
}