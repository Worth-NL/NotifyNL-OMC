// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant
{
    /// <summary>
    /// The sensitive data about a single party (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Common DTO for all versions of "OpenKlant" Web API service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public readonly struct CommonPartyData : IJsonSerializable
    {
        /// <summary>
        /// The first name of the citizen.
        /// </summary>
        internal string Name { get; init; }

        /// <summary>
        /// The prefix added before the Dutch / Belgian surname.
        /// <para>
        ///   <example>
        ///     Prefixes: "de", "van", "van de", "van der", "van den", "te", "ter", "ten".
        ///   </example>
        /// </para>
        /// </summary>
        internal string SurnamePrefix { get; init; }

        /// <summary>
        /// The last name (surname) of the citizen.
        /// </summary>
        public string Surname { get; internal init; }

        /// <inheritdoc cref="DistributionChannels"/>
        internal DistributionChannels DistributionChannel { get; init; }

        /// <summary>
        /// The e-mail address of the citizen.
        /// </summary>
        internal string EmailAddress { get; init; }

        /// <summary>
        /// The telephone number of the citizen.
        /// </summary>
        internal string TelephoneNumber { get; init; }

        /// <summary>
        /// Determines whether this instance is not initialized properly.
        /// </summary>
        internal bool IsDefault()
        {
            return this.DistributionChannel == DistributionChannels.Unknown &&
                   this.Name                == null &&
                   this.SurnamePrefix       == null &&
                   this.Surname             == null &&
                   this.EmailAddress        == null &&
                   this.TelephoneNumber     == null;
        }
    }
}