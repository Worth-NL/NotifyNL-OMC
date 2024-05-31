// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.Converters
{
    /// <summary>
    /// Converts subject data from different versions of "OpenKlant" into a unified <see cref="CommonPartyData"/>.
    /// </summary>
    internal static class CitizenDataConverter
    {
        /// <summary>
        /// Converts <see cref="CitizenResult"/> from "OpenKlant" (1.0) Web service.
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
        /// Converts <see cref="CitizenResult"/> from "OpenKlant" (2.0) Web service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        internal static CommonPartyData ConvertToUnified(this PartyResult party)
        {
            DistributionChannels distributionChannel = DetermineDistributionChannel(party);
            (string emailAddress, string telephoneNumber) = DetermineDigitalAddress(party, distributionChannel);

            return new CommonPartyData
            {
                Name                = party.Identification.Details.Name,
                SurnamePrefix       = party.Identification.Details.SurnamePrefix,
                Surname             = party.Identification.Details.Surname,
                DistributionChannel = distributionChannel,
                EmailAddress        = emailAddress,
                TelephoneNumber     = telephoneNumber
            };

            static DistributionChannels DetermineDistributionChannel(PartyResult party)
            {
                return party.Expansion.DigitalAddress.Type switch
                {
                    "Email" or "email" or "e-mail" => DistributionChannels.Email,
                    "SMS"   or "Sms"   or "sms"    => DistributionChannels.Sms,
                    _                              => DistributionChannels.Unknown
                };
            }

            static (string /* Email address */, string /* Telephone number */)
                DetermineDigitalAddress(PartyResult party, DistributionChannels distributionChannel)
            {
                return distributionChannel switch
                {
                    DistributionChannels.Email => (party.Expansion.DigitalAddress.Address, string.Empty),
                    DistributionChannels.Sms   => (string.Empty, party.Expansion.DigitalAddress.Address),
                    _                          => (string.Empty, string.Empty)

                };
            }
        }
    }
}