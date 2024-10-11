// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.v1
{
    /// <summary>
    /// The roles of the case retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseRoles : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }
        
        /// <summary>
        /// The collection of:
        /// <inheritdoc cref="OpenZaak.CaseRole"/>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CaseRole> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseRoles"/> struct.
        /// </summary>
        public CaseRoles()
        {
        }

        /// <summary>
        /// Gets the most recent (last) <see cref="OpenZaak.CaseRole"/>.
        /// </summary>
        /// <value>
        ///   The single role.
        /// </value>
        /// <exception cref="HttpRequestException"/>  // TODO: KeyNotFoundException
        internal readonly CaseRole CaseRole
        {
            get
            {
                if (this.Results.IsNullOrEmpty())
                {
                    throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyCaseRoles);
                }

                return this.Results[^1];  // TODO: Get party by initiator role
            }
        }
    }
}