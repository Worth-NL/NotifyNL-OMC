using Common.Constants;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenVtb
{
    /// <summary>
    /// Represents a message (Bericht) retrieved from the OpenVTB API.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct VtbMessage : IJsonSerializable
    {
        /// <summary>
        /// The unique URL of the Bericht within this API.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Url { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The Uniform Resource Name of the Bericht.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("urn")]
        [JsonPropertyOrder(1)]
        public string Urn { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier (UUID4) for the Bericht.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(2)]
        public string Uuid { get; set; } = string.Empty;

        /// <summary>
        /// Subject of the message (max 50 characters).
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("onderwerp")]
        [JsonPropertyOrder(3)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Text of the message (max 4000 characters). May contain Markdown or newlines.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("berichtTekst")]
        [JsonPropertyOrder(4)]
        public string MessageText { get; set; } = string.Empty;

        /// <summary>
        /// Date/time when the message becomes visible to the recipient. Nullable.
        /// </summary>
        [JsonPropertyName("publicatiedatum")]
        [JsonPropertyOrder(5)]
        public DateTime? PublicationDate { get; set; }

        /// <summary>
        /// Sender reference / internal reference (max 25 characters).
        /// </summary>
        [JsonPropertyName("referentie")]
        [JsonPropertyOrder(6)]
        public string? Reference { get; set; }

        /// <summary>
        /// URN of the recipient (natural or legal person). Required.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("ontvanger")]
        [JsonPropertyOrder(7)]
        public string RecipientUrn { get; set; } = string.Empty;

        /// <summary>
        /// Date/time when the message was opened by the recipient in the local portal. Nullable.
        /// </summary>
        [JsonPropertyName("geopendOp")]
        [JsonPropertyOrder(8)]
        public DateTime? OpenedOn { get; set; }

        /// <summary>
        /// Code for technical identification of the message type/origin (max 8 characters).
        /// </summary>
        [JsonPropertyName("berichtType")]
        [JsonPropertyOrder(9)]
        public string? MessageType { get; set; }

        /// <summary>
        /// List of related objects (e.g., case or product URNs).
        /// </summary>
        [JsonPropertyName("isGerelateerdAan")]
        [JsonPropertyOrder(10)]
        public RelatedObject[]? RelatedTo { get; set; }

        /// <summary>
        /// The action to be performed by the assigned person or company (max 50 characters).
        /// </summary>
        [JsonPropertyName("handelingsPerspectief")]
        [JsonPropertyOrder(11)]
        public string? ActionsPerspective { get; set; }

        /// <summary>
        /// Deadline for completing the action. Nullable.
        /// </summary>
        [JsonPropertyName("einddatumHandelingsTermijn")]
        [JsonPropertyOrder(12)]
        public DateTime? ActionDeadline { get; set; }

        /// <summary>
        /// Indicates whether this message is suitable for publication in the MijnOverheid Berichtenbox.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("mijnOverheidBerichtenbox")]
        [JsonPropertyOrder(13)]
        public bool IsInMyGovernmentMessageBox { get; set; }

        /// <summary>
        /// List of attachments for this message.
        /// </summary>
        [JsonPropertyName("bijlagen")]
        [JsonPropertyOrder(14)]
        public Attachment[]? Attachments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bericht"/> struct.
        /// </summary>
        public VtbMessage()
        {
        }

        /// <summary>
        /// Represents a related object (e.g., case or product) linked to the message.
        /// </summary>
        public struct RelatedObject
        {
            /// <summary>
            /// URN of the related object (e.g., zaak or product).
            /// </summary>
            [JsonRequired]
            [JsonPropertyName("urn")]
            public string Urn { get; set; } = string.Empty;

            public RelatedObject()
            {
            }
        }

        /// <summary>
        /// Represents an attachment (bijlage) of the message.
        /// </summary>
        public struct Attachment
        {
            /// <summary>
            /// URN of the information object (ENKELVOUDIGINFORMATIEOBJECT).
            /// </summary>
            [JsonRequired]
            [JsonPropertyName("informatieObject")]
            public string InformationObjectUrn { get; set; } = string.Empty;

            /// <summary>
            /// Human-readable description (max 40 characters) used as filename.
            /// </summary>
            [JsonPropertyName("omschrijving")]
            public string? Description { get; set; }

            /// <summary>
            /// Indicates whether this is a standard pre‑uploaded attachment (should be ignored for Berichtenbox).
            /// </summary>
            [JsonPropertyName("isBerichtTypeBijlage")]
            public bool IsMessageTypeAttachment { get; set; }

            public Attachment()
            {
            }
        }
    }
}