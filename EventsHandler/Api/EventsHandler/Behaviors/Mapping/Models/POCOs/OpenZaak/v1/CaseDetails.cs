// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1
{
    /// <summary>
    /// The details of the case retrieved from "OpenZaak" (1.0) Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseDetails : IJsonSerializable
    {
        /// <summary>
        /// Gets the <see cref="Uri"/> to OpenZaak service.
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