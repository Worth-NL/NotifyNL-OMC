// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.Interfaces;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant
{
    /// <summary>
    /// The sensitive data about a single party (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Common DTO for all versions of "OpenKlant" Web API service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    internal readonly struct CommonPartyData : IJsonSerializable
    {
        /// <summary>
        /// The reference to the party (e.g., citizen or organization) in <see cref="System.Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        internal Uri Uri { get; init; }

        /// <summary>
        /// The first name of the party (e.g., citizen or organization - company name).
        /// </summary>
        internal string Name { get; init; }

        /// <summary>
        /// The prefix added before the Dutch / Belgian surname (in case of citizen party data).
        /// <para>
        ///   <example>
        ///     Prefixes: "de", "van", "van de", "van der", "van den", "te", "ter", "ten".
        ///   </example>
        /// </para>
        /// </summary>
        internal string SurnamePrefix { get; init; }

        /// <summary>
        /// The last name (surname) of the party (in case of citizen party data).
        /// </summary>
        internal string Surname { get; init; }

        /// <inheritdoc cref="DistributionChannels"/>
        internal DistributionChannels DistributionChannel { get; init; }

        /// <summary>
        /// The e-mail address of the party (e.g., citizen or organization).
        /// </summary>
        internal string EmailAddress { get; init; }

        /// <summary>
        /// The telephone number of the party (e.g., citizen or organization).
        /// </summary>
        internal string TelephoneNumber { get; init; }
    }
}