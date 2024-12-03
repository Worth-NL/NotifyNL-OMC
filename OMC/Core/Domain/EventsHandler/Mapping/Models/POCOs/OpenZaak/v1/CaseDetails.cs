// © 2023, Worth Systems.

using Common.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.v1
{
    /// <summary>
    /// The details of the case retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseDetails : IJsonSerializable
    {
        /// <summary>
        /// The <see cref="CaseType"/> in <seealso cref="Uri"/> format.
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(0)]
        public Uri CaseTypeUrl { get; internal set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseDetails"/> struct.
        /// </summary>
        public CaseDetails()
        {
        }
    }
}