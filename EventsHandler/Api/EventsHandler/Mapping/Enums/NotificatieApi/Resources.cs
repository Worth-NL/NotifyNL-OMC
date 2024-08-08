// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Converters;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.NotificatieApi
{
    /// <summary>
    /// The name of the notification resource.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<Resources>))]
    public enum Resources
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The <see cref="Models.POCOs.OpenZaak.CaseStatus"/> resource.
        /// </summary>
        [JsonPropertyName("status")]
        Status = 1,

        /// <summary>
        /// The <see cref="TaskObject"/> resource.
        /// </summary>
        [JsonPropertyName("object")]
        Object = 2,

        /// <summary>
        /// The <see cref="Models.POCOs.OpenZaak.Decision"/> resource.
        /// </summary>
        [JsonPropertyName("besluitinformatieobject")]
        Decision = 3
    }
}