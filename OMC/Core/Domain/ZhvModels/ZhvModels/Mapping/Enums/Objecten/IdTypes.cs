// © 2024, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Converters;
using ZhvModels.Mapping.Models.POCOs.Objecten;

namespace ZhvModels.Mapping.Enums.Objecten
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
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The BSN (citizen service number) type of the <see cref="Identification"/>.
        /// </summary>
        [JsonPropertyName("bsn")]
        Bsn = 1,

        /// <summary>
        /// The KVK (Chamber of Commerce number) type of the <see cref="Identification"/>.
        /// </summary>
        [JsonPropertyName("kvk")]
        Kvk = 2
    }
}