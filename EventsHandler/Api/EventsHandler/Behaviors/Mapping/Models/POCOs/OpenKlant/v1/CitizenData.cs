// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1
{
    /// <summary>
    /// The sensitive data about a single citizen ("burger").
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CitizenData : IJsonSerializable
    {
        /// <summary>
        /// The first name of the citizen.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("voornaam")]
        [JsonPropertyOrder(0)]
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// The prefix added before the Dutch / Belgian surname.
        /// <para>
        ///   <example>
        ///     Prefixes: "de", "van", "van de", "van der", "van den", "te", "ter", "ten".
        ///   </example>
        /// </para>
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("voorvoegselAchternaam")]
        [JsonPropertyOrder(1)]
        public string SurnamePrefix { get; internal set; } = string.Empty;

        /// <summary>
        /// The last name (surname) of the citizen.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("achternaam")]
        [JsonPropertyOrder(2)]
        public string Surname { get; internal set; } = string.Empty;

        /// <inheritdoc cref="DistributionChannels"/>
        [JsonInclude]
        [JsonPropertyName("aanmaakkanaal")]
        [JsonPropertyOrder(3)]
        public DistributionChannels DistributionChannel { get; internal set; }

        /// <summary>
        /// The telephone number of the citizen.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("telefoonnummer")]
        [JsonPropertyOrder(4)]
        public string TelephoneNumber { get; internal set; } = string.Empty;

        /// <summary>
        /// The e-mail address of the citizen.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("emailadres")]
        [JsonPropertyOrder(5)]
        public string EmailAddress { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CitizenData"/> struct.
        /// </summary>
        public CitizenData()
        {
        }
    }
}