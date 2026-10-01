// © 2024, Worth Systems.

using ZgwModels.Mapping.Enums.OpenKlant;

namespace ZgwModels.Mapping.Models.POCOs.OpenKlant.Converters
{
    /// <summary>
    /// Converts subject data from different versions of "OpenKlant" into a unified <see cref="CommonPartyData"/>.
    /// </summary>
    public static class CommonPartyDataConverter
    {
        /// <summary>
        /// Converts <see cref="v2.PartyResult"/> from "OpenKlant" (2.0) Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        public static CommonPartyData ConvertToUnified(this (v2.PartyResult Party, DistributionChannels DistributionChannel, string EmailAddress, string PhoneNumber, string Reason) data)
        {
            return new CommonPartyData
            {
                Uri                     = data.Party.Uri,
                Name                    = data.Party.Identification?.Details.Name ?? string.Empty,
                SurnamePrefix           = data.Party.Identification?.Details.SurnamePrefix ?? string.Empty,
                Surname                 = GetSurname(data.Party),
                DistributionChannel     = data.DistributionChannel,
                DistributionChannelReason = data.Reason,
                EmailAddress            = data.EmailAddress,
                TelephoneNumber         = data.PhoneNumber,
                Gender                  = data.Party.SubjectIdentification.Gender
            };
        }

        // NOTE: An organization has no surname, only a name. It goes where a surname does, so the existing
        //       "Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam))" salutation
        //       reads correctly for a company instead of leaving it blank.
        private static string GetSurname(v2.PartyResult party)
        {
            string surname = party.Identification?.Details.Surname ?? string.Empty;

            return surname.Length > 0
                ? surname
                : party.Identification?.OrganizationName ?? string.Empty;
        }
    }
}
