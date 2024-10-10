// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.OpenKlant;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.Converters
{
    /// <summary>
    /// Converts subject data from different versions of "OpenKlant" into a unified <see cref="CommonPartyData"/>.
    /// </summary>
    internal static class CommonPartyDataConverter
    {
        /// <summary>
        /// Converts <see cref="v1.CitizenResult"/> from "OpenKlant" (1.0) Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        internal static CommonPartyData ConvertToUnified(this v1.CitizenResult citizen)
        {
            return new CommonPartyData
            {
                Uri                 = citizen.Uri,
                Name                = citizen.Name,
                SurnamePrefix       = citizen.SurnamePrefix,
                Surname             = citizen.Surname,
                DistributionChannel = citizen.DistributionChannel,
                EmailAddress        = citizen.EmailAddress,
                TelephoneNumber     = citizen.TelephoneNumber
            };
        }

        /// <summary>
        /// Converts <see cref="v2.CitizenResult"/> from "OpenKlant" (2.0) Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        internal static CommonPartyData ConvertToUnified(this
            (v2.CitizenResult Party, DistributionChannels DistributionChannel, string EmailAddress, string PhoneNumber) data)
        {
            return new CommonPartyData
            {
                Uri                 = data.Party.Uri,
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