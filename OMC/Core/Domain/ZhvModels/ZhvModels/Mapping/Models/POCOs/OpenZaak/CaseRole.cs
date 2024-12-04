// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.v2;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// A single role from <see cref="CaseRoles"/> results retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseRole : IJsonSerializable
    {
        /// <summary>
        /// The involved party associated with the <see cref="Case"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("betrokkene")]  // ENG: Involved party
        [JsonPropertyOrder(0)]
        public Uri InvolvedPartyUri { get; public set; } = CommonValues.Default.Models.EmptyUri;  // NOTE: Might be missing for Citizens

        /// <summary>
        /// The general description of the <see cref="CaseRole"/> which includes the "initiator role" of the <see cref="Case"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("omschrijvingGeneriek")]  // ENG: General description
        [JsonPropertyOrder(1)]
        public string InitiatorRole { get; public set; } = string.Empty;

        /// <summary>
        /// The data subject identification which includes details about a single citizen related to this <see cref="Case"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("betrokkeneIdentificatie")]  // ENG: Data subject (party) identification
        [JsonPropertyOrder(2)]
        public PartyData? Party { get; public set; } = new PartyData();  // NOTE: Might be missing for Organizations

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseRole"/> struct.
        /// </summary>
        public CaseRole()
        {
        }
    }
}