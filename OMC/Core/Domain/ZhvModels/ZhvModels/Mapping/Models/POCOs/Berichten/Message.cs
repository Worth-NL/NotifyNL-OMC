using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Berichten
{
    /// <summary>
    /// Represents a message fetched from the source system (e.g., a Berichtenbox message).
    /// </summary>
    public readonly struct MessageData : IJsonSerializable
    {
        /// <summary>
        /// Gets the absolute URL of the message resource.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; init; }

        /// <summary>
        /// Gets the Uniform Resource Name (URN) of the message.
        /// </summary>
        [JsonPropertyName("urn")]
        public string? Urn { get; init; }

        /// <summary>
        /// Gets the unique identifier (UUID) of the message.
        /// </summary>
        [JsonPropertyName("uuid")]
        public Guid? Uuid { get; init; }

        /// <summary>
        /// Gets the subject or title of the message (Dutch: "onderwerp").
        /// </summary>
        [JsonPropertyName("onderwerp")]
        public string? Subject { get; init; }

        /// <summary>
        /// Gets the main text content of the message (Dutch: "berichtTekst").
        /// </summary>
        [JsonPropertyName("berichtTekst")]
        public string? MessageText { get; init; }

        /// <summary>
        /// Gets the publication date of the message.
        /// </summary>
        [JsonPropertyName("publicatiedatum")]
        public DateTime? PublicationDate { get; init; }

        /// <summary>
        /// Gets the reference identifier (e.g., external reference number).
        /// </summary>
        [JsonPropertyName("referentie")]
        public string? Reference { get; init; }

        /// <summary>
        /// Gets the recipient identifier (URN) of the message.
        /// </summary>
        [JsonPropertyName("ontvanger")]
        public string? Recipient { get; init; }

        /// <summary>
        /// Gets the date and time when the message was opened.
        /// </summary>
        [JsonPropertyName("geopendOp")]
        public DateTime? OpenedOn { get; init; }

        /// <summary>
        /// Gets the type/category of the message (Dutch: "berichtType").
        /// This field is expected to be used for whitelist validation.
        /// </summary>
        [JsonPropertyName("berichtType")]
        public string? MessageType { get; init; }

        /// <summary>
        /// Gets the action perspective or expected handling instruction (Dutch: "handelingsPerspectief").
        /// </summary>
        [JsonPropertyName("handelingsPerspectief")]
        public string? ActionsPerspective { get; init; }

        /// <summary>
        /// Gets the deadline for performing the required action (Dutch: "einddatumHandelingsTermijn").
        /// </summary>
        [JsonPropertyName("einddatumHandelingsTermijn")]
        public DateTime? ActionDeadline { get; init; }

        /// <summary>
        /// Gets a value indicating whether the message is stored in the "MijnOverheid" message box.
        /// </summary>
        [JsonPropertyName("mijnOverheidBerichtenbox")]
        public bool IsInMyGovernmentMessageBox { get; init; }

        /// <summary>
        /// Gets the list of attachments (bijlagen) associated with the message.
        /// </summary>
        [JsonPropertyName("bijlagen")]
        public List<Attachment>? Attachments { get; init; }
    }

    /// <summary>
    /// Represents an attachment (bijlage) belonging to a message.
    /// </summary>
    public readonly struct Attachment
    {
        /// <summary>
        /// Gets the URN of the information object (bestand) attached.
        /// </summary>
        [JsonPropertyName("informatieObject")]
        public string? InformationObjectUrn { get; init; }

        /// <summary>
        /// Gets the description of the attachment.
        /// </summary>
        [JsonPropertyName("omschrijving")]
        public string? Description { get; init; }

        /// <summary>
        /// Gets a value indicating whether this attachment is of the same type as the message (i.e., a "berichtType bijlage").
        /// </summary>
        [JsonPropertyName("isBerichtTypeBijlage")]
        public bool IsMessageTypeAttachment { get; init; }
    }
}