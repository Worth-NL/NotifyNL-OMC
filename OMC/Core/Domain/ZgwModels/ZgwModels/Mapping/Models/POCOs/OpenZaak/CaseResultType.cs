// © 2025, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenZaak
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
        /// The generic, citizen-friendly description of the <see cref="CaseResultType"/>.
        /// </summary>
        /// <remarks>
        ///   Not [JsonRequired]: the Catalogi API derives it from the selectielijst, so it can be missing or empty.
        /// </remarks>
        [JsonPropertyName("omschrijvingGeneriek")]
        [JsonPropertyOrder(1)]
        public string GenericName { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseType"/> struct.
        /// </summary>
        public CaseResultType()
        {
        }
    }
}