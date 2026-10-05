// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenProducten
{
    /// <summary>
    /// The details of a single product retrieved from "Open Product" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "Open Product" (Producten API 1.7.0) Web API service.
    ///   <para>
    ///     NOTE: "Open Product" serializes its fields in snake_case, unlike the ZGW services which use camelCase.
    ///     Every property therefore carries an explicit <see cref="JsonPropertyNameAttribute"/>.
    ///   </para>
    ///   <para>
    ///     Only the fields OMC needs are mapped. The response also carries "documenten", "zaken", "taken",
    ///     "verbruiksobject", "dataobject", "prijs", "frequentie" and the "aanvraag_zaak_*" pair, which are
    ///     left to fall into <see cref="Orphans"/> rather than modelled.
    ///   </para>
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct Product : IJsonSerializable
    {
        /// <summary>
        /// The UUID / GUID of the product.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The reference <see cref="Uri"/> of the product.
        /// </summary>
        [JsonPropertyName("url")]
        [JsonPropertyOrder(1)]
        public Uri? Uri { get; set; } = null;

        /// <summary>
        /// The name of the product.
        /// </summary>
        [JsonPropertyName("naam")]
        [JsonPropertyOrder(2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The type of the product, embedded in full by "Open Product".
        /// </summary>
        /// <remarks>
        ///   NOTE: No separate request is required to obtain the product type - "Open Product" nests the whole
        ///   resource (code, name, uniforme product naam, ...) inside the product response.
        /// </remarks>
        [JsonPropertyName("producttype")]
        [JsonPropertyOrder(3)]
        public NestedProductType ProductType { get; set; } = new();

        /// <summary>
        /// Indicates whether the product may be shown (and therefore notified about).
        /// </summary>
        [JsonPropertyName("gepubliceerd")]
        [JsonPropertyOrder(4)]
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// The owners of the product. A product can have one or more.
        /// </summary>
        [JsonPropertyName("eigenaren")]
        [JsonPropertyOrder(5)]
        public List<Owner> Owners { get; set; } = [];

        /// <summary>
        /// The status of the product, e.g. "initieel", "gereed", "actief".
        /// </summary>
        [JsonPropertyName("status")]
        [JsonPropertyOrder(6)]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The JSON properties which couldn't be matched with the mapped properties of this model.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Orphans { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> struct.
        /// </summary>
        public Product()
        {
        }
    }
}
