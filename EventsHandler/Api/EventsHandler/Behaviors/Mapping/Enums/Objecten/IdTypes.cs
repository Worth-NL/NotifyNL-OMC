// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Converters;
using EventsHandler.Behaviors.Mapping.Models.POCOs.Objecten;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Enums.Objecten
{
    /// <summary>
    /// The type of the <see cref="Identification"/> from "Objecten" Web API service.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<IdTypes>))]
    public enum IdTypes
    {
        /// <summary>
        /// Default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The BSN type of the <see cref="Identification"/>.
        /// </summary>
        [JsonPropertyName("bsn")]
        Bsn = 1
    }
}