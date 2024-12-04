// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant.v1
{
    /// <summary>
    /// The results about the parties (e.g., citizen or organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct PartyResults : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; public set; }

        /// <inheritdoc cref="PartyResult"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<PartyResult> Results { get; public set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResults"/> struct.
        /// </summary>
        public PartyResults()
        {
        }

        /// <summary>
        /// Gets the <see cref="PartyResult"/>.
        /// </summary>
        /// <value>
        ///   The data of a single party (e.g., citizen or organization).
        /// </value>
        /// <exception cref="HttpRequestException"/>
        public readonly PartyResult Party
        {
            get
            {
                if (this.Results.IsNullOrEmpty())
                {
                    throw new HttpRequestException(ApiResources.HttpRequest_ERROR_EmptyPartiesResults);
                }

                return this.Results[^1];
            }
        }
    }
}