// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.v2
{
    /// <summary>
    /// The details of the case retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseDetails : IJsonSerializable
    {
        /// <summary>
        /// The <see cref="Case"/> identification.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(0)]
        public string Identification { get; internal set; } = string.Empty;

        /// <summary>
        /// The <see cref="Case"/> description.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(1)]
        public string Description { get; internal set; } = string.Empty;

        /// <summary>
        /// The <see cref="CaseType"/> in <seealso cref="Uri"/> format.
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(2)]
        public Uri CaseTypeUrl { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseDetails"/> struct.
        /// </summary>
        public CaseDetails()
        {
        }
    }
}