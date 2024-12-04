// © 2023, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using OpenKlant;
using OpenKlant;
using OpenKlant;

namespace EventsHandler.Services.DataProcessing.Enums
{
    /// <summary>
    /// The notification method used by "Notify NL" API Client to communicate with a citizen.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]  // NOTE: Simple JSON converter to display enum options in Swagger UI
    public enum NotifyMethods
    {
        /// <inheritdoc cref="ZhvModels.Mapping.Enums.OpenKlant.DistributionChannels.None"/>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
        None = DistributionChannels.None,  // 1

        /// <inheritdoc cref="ZhvModels.Mapping.Enums.OpenKlant.DistributionChannels.Email"/>
        [JsonPropertyName("email")]
        Email = DistributionChannels.Email,  // 2

        /// <inheritdoc cref="ZhvModels.Mapping.Enums.OpenKlant.DistributionChannels.Sms"/>
        [JsonPropertyName("sms")]
        Sms = DistributionChannels.Sms  // 3
    }
}