// © 2024, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Converters;
using ZgwModels.Mapping.Models.POCOs.Objecten.Task.vNijmegen;

namespace ZgwModels.Mapping.Enums.Objecten.vNijmegen
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