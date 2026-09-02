// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.Objecten.Print
{
    /// <summary>
    /// The data related to the <see cref="PrintRecord"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct PrintData : IJsonSerializable
    {
        /// <summary>
        /// The absolute URI of the "enkelvoudiginformatieobject" (in the "Documenten" Web API service)
        /// holding the PDF to be printed.
        /// </summary>
        /// <remarks>
        ///   Supplied by whoever wrote the object and therefore untrusted - it is validated against the
        ///   configured "Documenten" domain before anything is fetched, so that OMC cannot be used as a
        ///   fetch-anything proxy by anyone able to write an object.
        /// </remarks>
        [JsonRequired]
        [JsonPropertyName("pdfurl")]
        [JsonPropertyOrder(0)]
        public Uri? PdfUri { get; [UsedImplicitly] set; }

        /// <summary>
        /// The subject ("onderwerp") to register on the klantcontact.
        /// </summary>
        /// <remarks>
        ///   The PDF is opaque to OMC, so there is no other source for a meaningful subject - the
        ///   composing party has to supply one.
        /// </remarks>
        [JsonPropertyName("contact_onderwerp")]
        [JsonPropertyOrder(1)]
        public string Subject { get; [UsedImplicitly] set; } = string.Empty;

        /// <summary>
        /// The "onderwerpobjectidentificator" block to register on the klantcontact, identifying what the
        /// letter is about (normally the zaak).
        /// </summary>
        [JsonPropertyName("contact_onderwerpobjectidentificator")]
        [JsonPropertyOrder(2)]
        public SubjectObjectIdentifier? SubjectObjectIdentifier { get; [UsedImplicitly] set; }

        /// <summary>
        /// The URN identifying the recipient to register as "betrokkene" on the klantcontact, for example
        /// <c>urn:nld:bsn:nummer:123456782</c>.
        /// </summary>
        /// <remarks>
        ///   Only BSN-bearing URNs can currently be resolved to a partij; anything else is rejected
        ///   explicitly rather than silently dropped. See <c>BetrokkeneUrn</c>.
        /// </remarks>
        [JsonPropertyName("contact_betrokkene_urn")]
        [JsonPropertyOrder(3)]
        public string BetrokkeneUrn { get; [UsedImplicitly] set; } = string.Empty;

        /// <summary>
        /// The absolute URI of the zaaktype that the "onderwerpobjectidentificator" object (normally the
        /// zaak) belongs to, to register as the klantcontact's "hoofdOnderwerpType".
        /// </summary>
        /// <remarks>
        ///   OMC never resolves or fetches this URI - it is opaque to OMC and forwarded verbatim to
        ///   OpenKlant, so unlike <c>PdfUri</c> it needs no domain validation.
        /// </remarks>
        [JsonPropertyName("contact_hoofdOnderwerpType")]
        [JsonPropertyOrder(4)]
        public Uri? SubjectObjectTypeUri { get; [UsedImplicitly] set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintData"/> struct.
        /// </summary>
        public PrintData()
        {
        }
    }
}
