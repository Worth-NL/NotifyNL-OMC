// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// A single status from <see cref="CaseStatuses"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseStatus : IJsonSerializable
    {
        /// <summary>
        /// The type of the <see cref="CaseStatus"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("statustype")]
        [JsonPropertyOrder(0)]
        public Uri TypeUri { get; public set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The date and time when the <see cref="CaseStatus"/> was created.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("datumStatusGezet")]
        [JsonPropertyOrder(1)]
        public DateTime Created { get; public set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatus"/> struct.
        /// </summary>
        public CaseStatus()
        {
        }
    }
}