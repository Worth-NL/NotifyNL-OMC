using System.Text.Json.Serialization;
using IJsonSerializable = WebQueries.KTO.Models.IJsonSerializable;

namespace WebQueries.KTO.Models
{
    /// <summary>
    /// Represents the root object containing a list of case type settings.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseTypeSettingsObject : IJsonSerializable
    {
        /// <summary>
        /// A list of settings for different case types.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("ApproveAutomatically")]
        [JsonPropertyOrder(1)]
        public bool ApproveAutomatically { get; init; }

        /// <summary>
        /// A list of settings for different case types.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("IsTest")]
        [JsonPropertyOrder(1)]
        public bool IsTest { get; init; }

        /// <summary>
        /// A list of settings for different case types.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("caseTypeSettings")]
        [JsonPropertyOrder(1)]
        public CaseTypeSetting[] CaseTypeSettings { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseTypeSettingsObject"/> struct.
        /// </summary>
        /// <param name="caseTypeSettings">The list of case type settings.</param>
        public CaseTypeSettingsObject(CaseTypeSetting[] caseTypeSettings)
        {
            CaseTypeSettings = caseTypeSettings;
        }
    }

    /// <summary>
    /// Represents settings for a specific case type in the OpenZaak API.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseTypeSetting : IJsonSerializable
    {
        /// <summary>
        /// The unique identifier for the case type.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("caseTypeId")]
        [JsonPropertyOrder(0)]
        public string CaseTypeId { get; init; } = string.Empty;

        /// <summary>
        /// The name of the associated survey for the case type.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("Vragenlijst_naam")]
        [JsonPropertyOrder(1)]
        public string SurveyName { get; init; } = string.Empty;

        /// <summary>
        /// The name of the service associated with the case type.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("Dienst_naam")]
        [JsonPropertyOrder(2)]
        public string ServiceName { get; init; } = string.Empty;

        /// <summary>
        /// The type of survey used for the case type.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("Type_meting")]
        [JsonPropertyOrder(3)]
        public string SurveyType { get; init; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseTypeSetting"/> struct.
        /// </summary>
        public CaseTypeSetting()
        {
        }
    }
}