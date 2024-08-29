// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v1
{
    /// <summary>
    /// The results about the citizens retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CitizenResults : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <inheritdoc cref="CitizenResult"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CitizenResult> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CitizenResults"/> struct.
        /// </summary>
        public CitizenResults()
        {
        }

        /// <summary>
        /// Gets the <see cref="CitizenResult"/>.
        /// </summary>
        /// <value>
        ///   The data of a single citizen.
        /// </value>
        /// <exception cref="HttpRequestException"/>
        internal readonly CitizenResult Citizen
        {
            get
            {
                if (this.Results.IsNullOrEmpty())
                {
                    throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyCitizensResults);
                }

                return this.Results[^1];
            }
        }
    }
}