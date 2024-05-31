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
        internal static CommonPartyData ConvertToUnified(this CitizenResult citizenResult)
        {
            return new CommonPartyData
            {
                Name                = citizenResult.Name,
                SurnamePrefix       = citizenResult.SurnamePrefix,
                Surname             = citizenResult.Surname,
                DistributionChannel = citizenResult.DistributionChannel,
                EmailAddress        = citizenResult.EmailAddress,
                TelephoneNumber     = citizenResult.TelephoneNumber
            };
        }

        /// <summary>
        /// Converts <see cref="CitizenResult"/> from "OpenKlant" (2.0) Web service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        internal static CommonPartyData ConvertToUnified(this PartyResult partyResult)
        {
            DistributionChannels distributionChannel = DetermineDistributionChannel(partyResult);
            (string emailAddress, string telephoneNumber) = DetermineDigitalAddress(partyResult, distributionChannel);

            return new CommonPartyData
            {
                Name                = partyResult.Identification.Details.Name,
                SurnamePrefix       = partyResult.Identification.Details.SurnamePrefix,
                Surname             = partyResult.Identification.Details.Surname,
                DistributionChannel = distributionChannel,
                EmailAddress        = emailAddress,
                TelephoneNumber     = telephoneNumber
            };

            static DistributionChannels DetermineDistributionChannel(PartyResult partyResult)
            {
                return partyResult.Expansion.DigitalAddress.Type switch
                {
                    "Email" or "email" or "e-mail" => DistributionChannels.Email,
                    "SMS"   or "Sms"   or "sms"    => DistributionChannels.Sms,
                    _                              => DistributionChannels.Unknown
                };
            }

            static (string /* Email address */, string /* Telephone number */)
                DetermineDigitalAddress(PartyResult partyResult, DistributionChannels distributionChannel)
            {
                return distributionChannel switch
                {
                    DistributionChannels.Email => (partyResult.Expansion.DigitalAddress.Address, string.Empty),
                    DistributionChannels.Sms   => (string.Empty, partyResult.Expansion.DigitalAddress.Address),
                    _                          => (string.Empty, string.Empty)

                };
            }
        }
    }
}