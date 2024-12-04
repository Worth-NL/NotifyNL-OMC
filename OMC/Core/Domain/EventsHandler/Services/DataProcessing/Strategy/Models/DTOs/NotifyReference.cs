// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using Interfaces;
using NotificatieApi;
using OpenKlant;
using OpenZaak;

namespace EventsHandler.Services.DataProcessing.Strategy.Models.DTOs
{
    /// <summary>
    /// The set of data used to pass as a "reference" to "Notify NL" Web API service.
    /// </summary>
    /// <seealso cref="ZhvModels.Mapping.Models.Interfaces.IJsonSerializable" />
    public struct NotifyReference : IJsonSerializable  // NOTE: This model is used in endpoints + Swagger UI examples and it must be public
    {
        /// <inheritdoc cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(0)]
        public NotificationEvent Notification { get; internal set; } = default;

        /// <summary>
        /// The extracted GUID component from <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case.Uri"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        public Guid? CaseId { get; internal set; } = Guid.Empty;  // NOTE: Sometimes, case URI might be missing

        /// <summary>
        /// The extracted GUID component from <see cref="ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData.Uri"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(2)]
        public Guid PartyId { get; internal set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyReference"/> struct.
        /// </summary>
        public NotifyReference()
        {
        }
    }
}