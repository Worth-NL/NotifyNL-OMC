// © 2026, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenZaak.Documents
{
    /// <summary>
    /// Represents a single (non-compound) information object / document retrieved from the OpenZaak Documenten API.
    /// </summary>
    /// <remarks>
    /// This object contains metadata about the document and the base64-encoded content.
    /// By default, it returns the latest version of the document.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct SingularInformationObject : IJsonSerializable
    {
        /// <summary>
        /// Gets the unique URL of the document resource within the API.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("url")]
        public Uri Url { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Gets the identification code of the document.
        /// </summary>
        [JsonPropertyName("identificatie")]
        public string? Identification { get; set; }

        /// <summary>
        /// Gets the RSIN of the organization that created or received the document.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("bronorganisatie")]
        public string? SourceOrganization { get; set; }

        /// <summary>
        /// Gets the date when the document was created.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("creatiedatum")]
        public DateOnly? CreationDate { get; set; }

        /// <summary>
        /// Gets the formal name of the document.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("titel")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets the confidentiality level of the document (e.g., "openbaar", "intern", "geheim").
        /// </summary>
        [JsonPropertyName("vertrouwelijkheidaanduiding")]
        public string? Confidentiality { get; set; }

        /// <summary>
        /// Gets the person or organization primarily responsible for creating the document.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("auteur")]
        public string? Author { get; set; }

        /// <summary>
        /// Gets the status of the document (e.g., "in_bewerking", "definitief", "gearchiveerd").
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets the media type (MIME type) of the document content.
        /// </summary>
        [JsonPropertyName("formaat")]
        public string? Format { get; set; }

        /// <summary>
        /// Gets the ISO 639-2/B language code of the document content.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("taal")]
        public string? Language { get; set; }

        /// <summary>
        /// Gets the automatic version number of the document.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("versie")]
        public int Version { get; set; }

        /// <summary>
        /// Gets the date and time when this version of the document was created or modified.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("beginRegistratie")]
        public DateTime? RegistrationStart { get; set; }

        /// <summary>
        /// Gets the filename of the document, including the extension.
        /// </summary>
        [JsonPropertyName("bestandsnaam")]
        public string? Filename { get; set; }

        /// <summary>
        /// Gets the base64-encoded content of the document.
        /// </summary>
        [JsonPropertyName("inhoud")]
        public string? Content { get; set; }

        /// <summary>
        /// Gets the file size in bytes.
        /// </summary>
        [JsonPropertyName("bestandsomvang")]
        public long? FileSize { get; set; }

        /// <summary>
        /// Gets the download URL for the document content.
        /// </summary>
        [JsonPropertyName("link")]
        public Uri? DownloadLink { get; set; }

        /// <summary>
        /// Gets a generic description of the document content.
        /// </summary>
        [JsonPropertyName("beschrijving")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets the date when the document was received.
        /// </summary>
        [JsonPropertyName("ontvangstdatum")]
        public DateOnly? ReceiptDate { get; set; }

        /// <summary>
        /// Gets the date when the document was dispatched.
        /// </summary>
        [JsonPropertyName("verzenddatum")]
        public DateOnly? DispatchDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether usage restrictions apply to the document.
        /// </summary>
        [JsonPropertyName("indicatieGebruiksrecht")]
        public bool? UsageRestrictionIndicator { get; set; }

        /// <summary>
        /// Gets the essential formatting aspects of the document.
        /// </summary>
        [JsonPropertyName("verschijningsvorm")]
        public string? Manifestation { get; set; }

        /// <summary>
        /// Gets the URL reference to the document type.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("informatieobjecttype")]
        public Uri InformationObjectType { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Gets a value indicating whether the document is locked.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("locked")]
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets the list of keywords associated with the document.
        /// </summary>
        [JsonPropertyName("trefwoorden")]
        public string[]? Keywords { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularInformationObject"/> struct.
        /// </summary>
        public SingularInformationObject()
        {
        }
    }
}