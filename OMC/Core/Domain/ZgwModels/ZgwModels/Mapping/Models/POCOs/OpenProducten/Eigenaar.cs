// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenProducten
{
    /// <summary>
    /// An owner ("eigenaar") of a <see cref="Product"/> in "Open Product" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "Open Product" (Producten API 1.7.0) Web API service.
    ///   <para>
    ///     NOTE: <see cref="Id"/> is "Open Product"'s own primary key for this owner row - a plain uuid4 with
    ///     no relation to anything outside "Open Product". It cannot be used to find a party in "OpenKlant".
    ///     Only <see cref="BsnNumber"/>, <see cref="KvkNumber"/> (optionally narrowed by
    ///     <see cref="BranchNumber"/>) and <see cref="CustomerNumber"/> identify a real party.
    ///   </para>
    ///   <para>
    ///     "Open Product" validates that an owner carries either a BSN (and/or a customer number) or a KVK
    ///     number, never both, so the two identification routes are mutually exclusive by construction.
    ///   </para>
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct Eigenaar : IJsonSerializable
    {
        /// <summary>
        /// The UUID / GUID of the owner row inside "Open Product". Not an "OpenKlant" party identifier.
        /// </summary>
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The BSN ("Burgerservicenummer") of a natural person owner.
        /// </summary>
        [JsonPropertyName("bsn")]
        [JsonPropertyOrder(1)]
        public string BsnNumber { get; set; } = string.Empty;

        /// <summary>
        /// The KVK number of an organization owner.
        /// </summary>
        [JsonPropertyName("kvk_nummer")]
        [JsonPropertyOrder(2)]
        public string KvkNumber { get; set; } = string.Empty;

        /// <summary>
        /// The branch number ("vestigingsnummer") narrowing <see cref="KvkNumber"/> to a single branch.
        /// </summary>
        [JsonPropertyName("vestigingsnummer")]
        [JsonPropertyOrder(3)]
        public string BranchNumber { get; set; } = string.Empty;

        /// <summary>
        /// The generic customer number identifying a client or party.
        /// </summary>
        [JsonPropertyName("klantnummer")]
        [JsonPropertyOrder(4)]
        public string CustomerNumber { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Eigenaar"/> struct.
        /// </summary>
        public Eigenaar()
        {
        }
    }
}
