// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1
{
    /// <summary>
    /// The details of the case retrieved from "OpenZaak" Web service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseDetails : IJsonSerializable
    {
        /// <summary>
        /// Gets the case type.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(0)]
        public Uri CaseType { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseDetails"/> struct.
        /// </summary>
        public CaseDetails()
        {
        }
    }
}