// © 2025, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The type of the <see cref="Case"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseResultType : IJsonSerializable
    {
        /// <summary>
        /// The name of the <see cref="CaseResultType"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(0)]
        public string Name { get; set; } = string.Empty;

        

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseType"/> struct.
        /// </summary>
        public CaseResultType()
        {
        }
    }
}