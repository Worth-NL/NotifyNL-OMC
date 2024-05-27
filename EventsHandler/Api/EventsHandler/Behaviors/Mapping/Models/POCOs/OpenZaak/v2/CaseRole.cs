// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v2
{
    /// <summary>
    /// A single role from <see cref="CaseRoles"/> results retrieved from "OpenZaak" (2.0) Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseRole : IJsonSerializable
    {
        /// <summary>
        /// The general description of the <see cref="CaseRole"/> which includes the "initiator role" of the case.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("omschrijvingGeneriek")]  // ENG: General description
        [JsonPropertyOrder(0)]
        public string InitiatorRole { get; internal set; } = string.Empty;

        /// <summary>
        /// The data subject identification which includes details about a single citizen related to this case.
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