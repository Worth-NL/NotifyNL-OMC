// © 2024, Worth Systems.

using Common.Constants;
using Common.Enums.Converters;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task.vNijmegen;

namespace ZhvModels.Mapping.Enums.Objecten.vNijmegen
{
    /// <summary>
    /// The type of the <see cref="Coupling"/> from "Objecten" Web API service.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<Registrations>))]
    public enum Registrations
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The case type of the <see cref="Coupling"/>.
        /// </summary>
        [JsonPropertyName("zaak")]
        Case = 1
    }
}