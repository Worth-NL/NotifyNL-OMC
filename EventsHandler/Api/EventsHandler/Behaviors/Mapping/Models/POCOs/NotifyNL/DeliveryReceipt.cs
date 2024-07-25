// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.NotifyNL
{
    /// <summary>
    /// When you send an email or text message, Notify will send a receipt to your callback URL with the status of the message.
    /// This is an automated method to get the status of messages. The callback message is formatted in JSON. All the values are
    /// strings, apart from the template version, which is a number.
    /// <para>
    ///   Source:
    ///
    ///   <code>
    ///     https://docs.notifications.service.gov.uk/rest-api.html#delivery-receipts
    ///   </code> 
    /// </para>
    /// </summary>
    internal struct DeliveryReceipt : IJsonSerializable
    {
        internal static DeliveryReceipt Default { get; } = new();

        /// <summary>
        /// Notify’s id for the status receipts.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("id")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; internal set; }

        /// <summary>
        /// The reference sent by the service.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("reference")]
        [JsonPropertyOrder(1)]
        public string? Reference { get; internal set; }

        /// <summary>
        /// The email address or phone number of the recipient.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("to")]
        [JsonPropertyOrder(2)]
        public string Recipient { get; internal set; } = string.Empty;

        /// <summary>
        /// The status of the notification.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(3)]
        public DeliveryStatuses Status { get; internal set; }

        /// <summary>
        /// The time the service sent the request.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(4)]
        public DateTime CreatedAt { get; internal set; }

        /// <summary>
        /// The last time the status was updated.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("completed_at")]
        [JsonPropertyOrder(5)]
        public DateTime CompletedAt { get; internal set; }

        /// <summary>
        /// The time the notification was sent.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("sent_at")]
        [JsonPropertyOrder(6)]
        public DateTime SentAt { get; internal set; }

        /// <summary>
        /// The notification type.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("notification_type")]
        [JsonPropertyOrder(7)]
        public NotificationTypes Type { get; internal set; }

        /// <summary>
        /// The id of the template that was used.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("template_id")]
        [JsonPropertyOrder(8)]
        public Guid TemplateId { get; internal set; }

        /// <summary>
        /// The version number of the template that was used.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("template_version")]
        [JsonPropertyOrder(9)]
        public int TemplateVersion { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryReceipt"/> struct.
        /// </summary>
        public DeliveryReceipt()
        {
        }
    }
}