// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Converters;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.Objecten
{
    /// <summary>
    /// The type of the <see cref="Identification"/> from "Objecten" Web API service.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<IdTypes>))]
    public enum IdTypes
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The BSN (citizen service number) type of the <see cref="Identification"/>.
        /// </summary>
        [JsonPropertyName("bsn")]
        Bsn = 1
    }
}