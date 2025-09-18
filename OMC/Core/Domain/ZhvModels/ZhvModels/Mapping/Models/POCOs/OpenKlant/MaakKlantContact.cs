// © 2025, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant
{
    /// <summary>
    /// 
    /// </summary>
    public struct MaakKlantContact : IJsonSerializable
    {
        /// <summary>
        /// The reference to the <see cref="ContactMoment"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("klantcontact")]
        [JsonPropertyOrder(0)]
        public ContactMoment ContactMoment { get; set; }

    }
}
