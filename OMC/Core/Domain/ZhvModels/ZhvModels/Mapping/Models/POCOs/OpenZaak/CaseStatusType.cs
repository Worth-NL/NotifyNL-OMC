using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
{
    // TODO: Remove redundancy or duplication between this and CaseType
    /// <summary>
    /// A single statustype from <see cref="CaseStatus"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseStatusType : IJsonSerializable
    {
        /// <summary>
        /// The name of the <see cref="CaseType"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(0)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the <see cref="CaseType"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("omschrijvingGeneriek")]
        [JsonPropertyOrder(1)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The identification of the <see cref="Case"/> in the following format:
        /// <code>
        /// ZAAKTYPE-2023-0000000010
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("zaaktypeIdentificatie")]
        [JsonPropertyOrder(2)]
        public string Identification { get; set; } = string.Empty;

        /// <summary>
        /// The serial number of the CaseStatusType
        /// <inheritdoc cref="CaseStatusType"/>
        /// </summary>
        [JsonPropertyName("volgnummer")]
        [JsonPropertyOrder(3)]
        public int SerialNumber { get; set; } = 0;

        /// <summary>
        /// Determines whether the <see cref="CaseStatus"/> is final, which means that the <see cref="Case"/> is closed.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("isEindstatus")]
        [JsonPropertyOrder(4)]
        public bool IsFinalStatus { get; set; } = false;

        /// <summary>
        /// Determines whether the party (e.g., user or organization) wants to be notified about this certain <see cref="CaseStatus"/> update.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("informeren")]
        [JsonPropertyOrder(5)]
        public bool IsNotificationExpected { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatus"/> struct.
        /// </summary>
        public CaseStatusType()
        {
        }
    }
}
