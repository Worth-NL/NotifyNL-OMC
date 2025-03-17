using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.KTO.Models
{
    /// <summary>
    /// Represents the root object containing case type settings.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseTypeSettingsObject : IJsonSerializable
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("caseTypeSettings")]
        [JsonPropertyOrder(1)]
        public List<CaseTypeSetting> CaseTypeSettings { get; init; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseType"/> struct.
        /// </summary>
        public CaseTypeSettingsObject()
        {
        }
    }

    /// <summary>
    /// Represents settings for case types in the OpenZaak API.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseTypeSetting : IJsonSerializable
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("caseTypeId")]
        [JsonPropertyOrder(0)]
        public string CaseTypeId { get; init; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("vragenlijstNaam")]
        [JsonPropertyOrder(1)]
        public string SurveyName { get; init; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("dienstNaam")]
        [JsonPropertyOrder(2)]
        public string ServiceName { get; init; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("typeMeting")]
        [JsonPropertyOrder(3)]
        public string SurveyType { get; init; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseType"/> struct.
        /// </summary>
        public CaseTypeSetting()
        {
        }
    }
}