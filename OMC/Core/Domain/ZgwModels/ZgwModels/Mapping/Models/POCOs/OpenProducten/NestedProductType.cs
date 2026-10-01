// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenProducten
{
    /// <summary>
    /// The type of a <see cref="Product"/>, as embedded in the product response by "Open Product".
    /// </summary>
    /// <remarks>
    ///   Version: "Open Product" (Producten API 1.7.0) Web API service.
    ///   <para>
    ///     <see cref="Code"/> is the field OMC whitelists on. It is also published as a notification
    ///     "kenmerk", but OMC reads it from here instead, so that routing does not depend on the
    ///     casing "Open Notificaties" happens to apply to kenmerken keys.
    ///   </para>
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct NestedProductType : IJsonSerializable
    {
        /// <summary>
        /// The UUID / GUID of the product type.
        /// </summary>
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The code of the product type, e.g. "PARKEERVERGUNNING-A".
        /// </summary>
        [JsonPropertyName("code")]
        [JsonPropertyOrder(1)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// The name of the product type, e.g. "Parkeervergunning".
        /// </summary>
        [JsonPropertyName("naam")]
        [JsonPropertyOrder(2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The national "Uniforme Product Naam" of the product type.
        /// </summary>
        [JsonPropertyName("uniforme_product_naam")]
        [JsonPropertyOrder(3)]
        public string UniformProductName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the product type may be shown.
        /// </summary>
        [JsonPropertyName("gepubliceerd")]
        [JsonPropertyOrder(4)]
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// The JSON properties which couldn't be matched with the mapped properties of this model.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Orphans { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedProductType"/> struct.
        /// </summary>
        public NestedProductType()
        {
        }
    }
}
