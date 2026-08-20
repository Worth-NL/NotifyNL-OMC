// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

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
        /// The boolean flag indicating if it was sent to Logius Message Box.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(3)]
        public bool Mobb { get; set; } = false;

        /// <summary>
        /// The boolean flag indicating if a notification was sent successfully.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(4)]
        public bool Notified { get; set; } = false;

        /// <summary>
        /// The dashboard trace this notification belongs to, if any — carried through opaquely
        /// (never interpreted by "Notify NL" itself) so its later delivery confirmation
        /// callback can still be attributed to the same trace, even though that callback
        /// arrives on a separate, later request with no other link back to this one.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(5)]
        public string? TraceId { get; set; }

        /// <summary>
        /// Unix milliseconds when this notification was handed off to "Notify NL", used to work
        /// out how long a later delivery confirmation took to arrive.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(6)]
        public long? SentAtUnixMs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyReference"/> struct.
        /// </summary>
        public NotifyReference()
        {
        }
    }
}