// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.v2
{
    /// <summary>
    /// A single role from <see cref="CaseRoles"/> results retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseRole : IJsonSerializable
    {
        /// <summary>
        /// The general description of the <see cref="CaseRole"/> which includes the "initiator role" of the <see cref="Case"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("omschrijvingGeneriek")]  // ENG: General description
        [JsonPropertyOrder(0)]
        public string InitiatorRole { get; internal set; } = string.Empty;

        /// <summary>
        /// The data subject identification which includes details about a single citizen related to this <see cref="Case"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("betrokkeneIdentificatie")]  // ENG: Data subject identification
        [JsonPropertyOrder(1)]
        public CitizenData Citizen { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseRole"/> struct.
        /// </summary>
        public CaseRole()
        {
        }
    }
}