// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using Common.Constants;
using ZgwModels.Mapping.Models.Interfaces;
using ZgwModels.Mapping.Models.POCOs.Objecten.Print;

namespace WebQueries.Print.Models
{
    /// <summary>
    /// The set of data used to pass as a "reference" to "Notify NL" Web API service, for the print
    /// (SZW "printstraat") scenario.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct PrintNotifyReference : IJsonSerializable
    {
        /// <summary>
        /// The UUID of the "Objecten" object that triggered this print job.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(0)]
        public Guid ObjectId { get; set; } = Guid.Empty;

        /// <summary>
        /// The extracted GUID component from <see cref="ZgwModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData.Uri"/>
        /// of the recipient, resolved from the BSN carried by the object's "contact_betrokkene_urn".
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        public Guid PartyId { get; set; } = Guid.Empty;

        /// <summary>
        /// The subject to register on the klantcontact, taken from the object's "contact_onderwerp".
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(2)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// The "onderwerpobjectidentificator" supplied on the object, if any. Fields left blank here fall
        /// back to their configured values.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(3)]
        public SubjectObjectIdentifier? SubjectObject { get; set; } = null;

        /// <summary>
        /// The UUID of the "enkelvoudiginformatieobject" holding the printed PDF, linked to the
        /// klantcontact as a "bijlage".
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(4)]
        public Guid AttachmentId { get; set; } = Guid.Empty;

        /// <summary>
        /// The absolute URI of the "Objecten" object that triggered this print job (i.e. the
        /// notification's own "hoofdObject") - registered on the klantcontact's "metadata" so that
        /// external systems (e.g. GZAC) can relate the contactmoment back to the print request.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(5)]
        public Uri OriginalResourceUrl { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintNotifyReference"/> struct.
        /// </summary>
        public PrintNotifyReference()
        {
        }
    }
}
