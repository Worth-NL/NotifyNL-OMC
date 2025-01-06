// © 2024, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Enums.OpenZaak;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// The information object related to the <see cref="DecisionResource"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct InfoObject : IJsonSerializable
    {
        /// <inheritdoc cref="PrivacyNotices"/>
        [JsonRequired]
        [JsonPropertyName("vertrouwelijkheidaanduiding")]
        [JsonPropertyOrder(0)]
        public PrivacyNotices Confidentiality { get; set; }  // TODO: Test options for this enum. Only "openbaar" is expected and this enum is reused from notification

        /// <inheritdoc cref="MessageStatus"/>
        [JsonRequired]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(1)]
        public MessageStatus Status { get; set; }

        /// <summary>
        /// The type of the <see cref="InfoObject"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("informatieobjecttype")]
        [JsonPropertyOrder(2)]
        public Uri TypeUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoObject"/> struct.
        /// </summary>
        public InfoObject()
        {
        }
    }
}