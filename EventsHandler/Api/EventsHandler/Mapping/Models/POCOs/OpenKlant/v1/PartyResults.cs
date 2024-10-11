// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v1
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
        public int Count { get; internal set; }

        /// <inheritdoc cref="PartyResult"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<PartyResult> Results { get; internal set; } = new();

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
        internal readonly PartyResult Party
        {
            get
            {
                if (this.Results.IsNullOrEmpty())
                {
                    throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyPartyResults);
                }

                return this.Results[^1];  // TODO: Get party by initiator role
            }
        }
    }
}