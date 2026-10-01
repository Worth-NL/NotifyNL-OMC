// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The identification of the party (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct PartyIdentification : IJsonSerializable
    {
        /// <inheritdoc cref="PartyDetails"/>
        /// <remarks>
        ///   Only a person has a contact name. An organization is identified by <see cref="OrganizationName"/>
        ///   instead, so this is not <c>[JsonRequired]</c>.
        /// </remarks>
        [JsonPropertyName("contactnaam")]
        [JsonPropertyOrder(0)]
        public PartyDetails Details { get; set; }

        /// <summary>
        /// The name of the organization (e.g., a company identified by its KVK number).
        /// </summary>
        /// <remarks>
        ///   Absent for a person.
        /// </remarks>
        [JsonPropertyName("naam")]
        [JsonPropertyOrder(1)]
        public string OrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyIdentification"/> struct.
        /// </summary>
        public PartyIdentification()
        {
        }
    }
}