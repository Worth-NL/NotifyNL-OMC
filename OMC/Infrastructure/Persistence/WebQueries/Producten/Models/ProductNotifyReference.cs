// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace WebQueries.Producten.Models
{
    /// <summary>
    /// The set of data passed as a "reference" to "Notify NL" Web API service, for the "Product created"
    /// scenario, and handed back on the delivery receipt.
    /// </summary>
    /// <remarks>
    ///   This is the only state that survives to the callback, so everything the contactmoment needs has to
    ///   be in it.
    ///   <para>
    ///     It deliberately declares no "ObjectId" and no "MessageId". The callback responder tells the
    ///     reference shapes apart by peeking at the raw JSON for exactly those two property names, so
    ///     reusing either would route a product's delivery receipt into the print or MOBB flow.
    ///   </para>
    ///   <para>
    ///     Adding a required field here breaks receipts already in flight from an older deploy, which can
    ///     arrive days later. New fields stay optional.
    ///   </para>
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct ProductNotifyReference : IJsonSerializable
    {
        /// <summary>
        /// The UUID of the product in "Open Product" that this notification was about.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(0)]
        public Guid ProductId { get; set; } = Guid.Empty;

        /// <summary>
        /// The UUID of the party in "OpenKlant" this notification was sent to. One owner of the product.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        public Guid PartyId { get; set; } = Guid.Empty;

        /// <summary>
        /// The name of the product, used as the subject of the klantcontact.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(2)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// The code of the product's type, carried for traceability on the klantcontact.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(3)]
        public string ProductTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// The absolute URI of the product, registered on the klantcontact's "metadata" so external systems
        /// can relate the contactmoment back to what caused it.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(4)]
        public string OriginalResourceUrl { get; set; } = string.Empty;

        /// <summary>
        /// The dashboard trace this notification belongs to, so a delivery receipt arriving later can be
        /// correlated back to it.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(5)]
        public string? TraceId { get; set; } = null;

        /// <summary>
        /// When the notification was handed to "Notify NL", as Unix milliseconds.
        /// </summary>
        [JsonInclude]
        [JsonPropertyOrder(6)]
        public long? SentAtUnixMs { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductNotifyReference"/> struct.
        /// </summary>
        public ProductNotifyReference()
        {
        }
    }
}
