using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
{

    /// <summary>
    /// A single statustype from <see cref="CaseStatus"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseStatusType : IJsonSerializable
    {
        /// <summary>
        /// The serial number of the CaseStatusType
        /// <inheritdoc cref="CaseStatusType"/>
        /// </summary>
        [JsonPropertyName("volgnummer")]
        [JsonPropertyOrder(0)]
        public int SerialNumber { get; set; } = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatus"/> struct.
        /// </summary>
        public CaseStatusType()
        {
        }
    }
}
