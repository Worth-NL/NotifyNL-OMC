// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1
{
    /// <summary>
    /// A single role from <see cref="CaseRoles"/> results retrieved from "OpenZaak" (1.0) Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseRole : IJsonSerializable
    {
        /// <summary>
        /// The data about a single citizen.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("betrokkeneIdentificatie")]  // ENG: Data subject identification
        [JsonPropertyOrder(0)]
        public CitizenData Citizen { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseRole"/> struct.
        /// </summary>
        public CaseRole()
        {
        }
    }
}