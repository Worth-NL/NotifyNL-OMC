// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace WebQueries.BRP.Models
{
    /// <summary>
    /// The response of a BRP API Personen query (e.g., "RaadpleegMetBurgerservicenummer").
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct PersonenQueryResponse : IJsonSerializable
    {
        /// <summary>
        /// The type of query this is a response to (e.g., "RaadpleegMetBurgerservicenummer").
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(0)]
        public string? Type { get; set; }

        /// <summary>
        /// The persons matching the query.
        /// </summary>
        [JsonPropertyName("personen")]
        [JsonPropertyOrder(1)]
        public List<Persoon>? Personen { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonenQueryResponse"/> struct.
        /// </summary>
        public PersonenQueryResponse()
        {
        }
    }
}
