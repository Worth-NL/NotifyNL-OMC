// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.DataSending.Models.DTOs
{
    /// <summary>
    /// The set of data used to pass as a "reference" to "Notify NL" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct NotifyReference : IJsonSerializable  // NOTE: This model is used in endpoints + Swagger UI examples and it must be public
    {
        /// <inheritdoc cref="NotificationEvent"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(0)]
        public NotificationEvent Notification { get; set; } = default;

        /// <summary>
        /// The extracted GUID component from <see cref="Case.Uri"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        public Guid? CaseId { get; set; } = Guid.Empty;  // NOTE: Sometimes, case URI might be missing

        /// <summary>
        /// The extracted GUID component from <see cref="CommonPartyData.Uri"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(2)]
        public Guid PartyId { get; set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyReference"/> struct.
        /// </summary>
        public NotifyReference()
        {
        }
    }
}