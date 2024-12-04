// © 2024, Worth Systems.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.NotifyNL;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.NotifyNL
{
    /// <summary>
    /// The delivery receipt coming from "Notify" Web API service.
    /// </summary>
    /// <remarks>
    /// When you send an email or text message, "Notify" will send a receipt to your callback URL with the status of the message.
    /// This is an automated method to get the status of messages. The callback message is formatted in JSON. All the values are
    /// strings, apart from the template version, which is a number.
    /// <para>
    ///   Source:
    ///
    ///   <code>
    ///     https://docs.notifications.service.gov.uk/rest-api.html#delivery-receipts
    ///   </code> 
    /// </para>
    /// </remarks>
    public struct DeliveryReceipt : IJsonSerializable
    {
        /// <summary>
        /// The default <see cref="DeliveryReceipt"/>.
        /// </summary>
        public static DeliveryReceipt Default { get; } = new();

        /// <summary>
        /// The "Notify" ID for the status receipts.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("id")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; public set; }

        /// <summary>
        /// The reference sent by the "Notify" Web API service.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("reference")]
        [JsonPropertyOrder(1)]
        public string? Reference { get; public set; }

        /// <summary>
        /// The email address or phone number of the recipient.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("to")]
        [JsonPropertyOrder(2)]
        public string Recipient { get; public set; } = string.Empty;

        /// <summary>
        /// The status of the notification.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(3)]
        public DeliveryStatuses Status { get; public set; }

        /// <summary>
        /// The time when the "Notify" Web API service sent the request.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(4)]
        public DateTime CreatedAt { get; public set; }

        /// <summary>
        /// The last time when the status of the notification was updated.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("completed_at")]
        [JsonPropertyOrder(5)]
        public DateTime CompletedAt { get; public set; }

        /// <summary>
        /// The time when the notification was sent.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("sent_at")]
        [JsonPropertyOrder(6)]
        public DateTime SentAt { get; public set; }

        /// <summary>
        /// The notification type.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("notification_type")]
        [JsonPropertyOrder(7)]
        public NotificationTypes Type { get; public set; }

        /// <summary>
        /// The ID of the template that was used.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("template_id")]
        [JsonPropertyOrder(8)]
        public Guid TemplateId { get; public set; }

        /// <summary>
        /// The version number of the template that was used.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("template_version")]
        [JsonPropertyOrder(9)]
        public int TemplateVersion { get; public set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryReceipt"/> struct.
        /// </summary>
        public DeliveryReceipt()
        {
        }
    }
}