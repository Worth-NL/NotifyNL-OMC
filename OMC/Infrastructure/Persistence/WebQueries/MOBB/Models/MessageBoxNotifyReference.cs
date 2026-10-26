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
        /// <remarks>
        ///   This is the source of truth for "was this MOBB" - a fallback letter template's "you could
        ///   enable MijnOverheid Berichtenbox" upsell text (BPMN: "Was voor MOBB") should be driven by
        ///   <see langword="!"/><see cref="Mobb"/>, not a separately-stored flag: within this scenario every
        ///   reference with <see cref="Mobb"/> <see langword="false"/> is, by construction, a MOBB fallback.
        /// </remarks>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(3)]
        public bool Mobb { get; set; } = true;

        /// <summary>
        /// The boolean flag indicating a digitale-post (email) notification attempt failed before this
        /// send happened - only meaningful when this reference belongs to a letter fallback.
        /// </summary>
        /// <remarks>
        ///   Maps to the BPMN's "Was notificatie" flag on the email-failure contactmoment step, used to decide whether a
        ///   fallback letter template should include the "please fix your email address" text. Unlike
        ///   "Was voor MOBB", this genuinely cannot be derived from <see cref="Mobb"/> or the delivered
        ///   channel alone - it records a failed *prior* attempt on a different channel.
        /// </remarks>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(4)]
        public bool WasGefaaldeNotificatie { get; set; } = false;

        /// <summary>
        /// The Bericht's BerichtType (Open VTB), registered as the klantcontact's "hoofdOnderwerpType".
        /// </summary>
        /// <remarks>
        ///   Deliberately not [JsonRequired]: references minted before this field existed are still echoed back by
        ///   "Notify NL" on later delivery callbacks, and must keep deserializing.
        /// </remarks>
        [JsonInclude]
        [JsonPropertyOrder(5)]
        public Uri? MessageTypeUri { get; set; } = null;

        /// <summary>
        /// The id "Notify NL" gave this notification, filled in from the delivery receipt - never sent to "Notify NL".
        /// </summary>
        /// <remarks>
        ///   Written into the klantcontact's metadata, so a retried or duplicate delivery receipt finds the klantcontact
        ///   it already wrote instead of writing a second one. [JsonIgnore], because the reference handed to
        ///   "Notify NL" has to stay the same on every delivery of a notification (#247).
        /// </remarks>
        [JsonIgnore]
        public Guid NotifyNotificationId { get; set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxNotifyReference"/> struct.
        /// </summary>
        public MessageBoxNotifyReference()
        {
        }
    }
}
