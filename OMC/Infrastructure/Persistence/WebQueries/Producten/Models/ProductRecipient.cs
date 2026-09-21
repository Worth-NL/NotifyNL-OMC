// © 2026, Worth Systems.

using ZgwModels.Mapping.Models.POCOs.OpenKlant;

namespace WebQueries.Producten.Models
{
    /// <summary>
    /// One owner of a product, resolved to the party behind it and to the address it can be reached at.
    /// </summary>
    /// <remarks>
    ///   The party is always present - an owner without one aborts the whole notification before any
    ///   recipient is built. The address may not be, and that is not a reason to stop: an unreachable
    ///   recipient is recorded as a failed contactmoment against its party, which is only possible because
    ///   the party is guaranteed.
    /// </remarks>
    public readonly struct ProductRecipient
    {
        /// <summary>
        /// The party behind the owner, as resolved in "OpenKlant".
        /// </summary>
        public CommonPartyData Party { get; init; }

        /// <summary>
        /// The e-mail address to notify, or empty when the party has none on file.
        /// </summary>
        public string EmailAddress { get; init; }

        /// <summary>
        /// Whether this recipient can be notified at all.
        /// </summary>
        public bool IsReachable => !string.IsNullOrWhiteSpace(this.EmailAddress);

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductRecipient"/> struct.
        /// </summary>
        public ProductRecipient()
        {
            this.Party = default;
            this.EmailAddress = string.Empty;
        }
    }
}
