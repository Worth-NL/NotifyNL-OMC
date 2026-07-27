// © 2026, Worth Systems.

using System.Text.Json;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace WebQueries.MOBB.Models
{
    /// <summary>
    /// The set of data used to pass as a "reference" to "Notify NL" Web API service, for the MOBB / Berichtenbox scenario.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct MessageBoxNotifyReference : IJsonSerializable
    {
        /// <summary>
        /// The full CloudEvent that triggered this MOBB notification.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(0)]
        public JsonElement CloudEvent { get; set; } = default;

        /// <summary>
        /// The UUID of the VTB message (Bericht) this notification was built from.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        public Guid MessageId { get; set; } = Guid.Empty;

        /// <summary>
        /// The extracted GUID component from <see cref="ZgwModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData.Uri"/>
        /// of the recipient, resolved from their BSN.
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
        public bool Mobb { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxNotifyReference"/> struct.
        /// </summary>
        public MessageBoxNotifyReference()
        {
        }
    }
}
