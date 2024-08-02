// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.v1;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.v2;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.Converters
{
    /// <summary>
    /// Converts subject data from different versions of "OpenKlant" into a unified <see cref="CommonPartyData"/>.
    /// </summary>
    internal static class CitizenDataConverter
    {
        /// <summary>
        /// Converts <see cref="CitizenResult"/> from "OpenKlant" (1.0) Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        internal static CommonPartyData ConvertToUnified(this CitizenResult citizen)
        {
            return new CommonPartyData
            {
                Name                = citizen.Name,
                SurnamePrefix       = citizen.SurnamePrefix,
                Surname             = citizen.Surname,
                DistributionChannel = citizen.DistributionChannel,
                EmailAddress        = citizen.EmailAddress,
                TelephoneNumber     = citizen.TelephoneNumber
            };
        }

        /// <summary>
        /// Converts <see cref="PartyResult"/> from "OpenKlant" (2.0) Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        internal static CommonPartyData ConvertToUnified(this
            (PartyResult Party, DistributionChannels DistributionChannel, string EmailAddress, string PhoneNumber) data)
        {
            return new CommonPartyData
            {
                Name                = data.Party.Identification.Details.Name,
                SurnamePrefix       = data.Party.Identification.Details.SurnamePrefix,
                Surname             = data.Party.Identification.Details.Surname,
                DistributionChannel = data.DistributionChannel,
                EmailAddress        = data.EmailAddress,
                TelephoneNumber     = data.PhoneNumber
            };
        }
    }
}