// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The results about the parties (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct PartyResults : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <inheritdoc cref="PartyResult"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<PartyResult> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResults"/> struct.
        /// </summary>
        public PartyResults()
        {
        }

        /// <summary>
        /// Gets the <see cref="PartyResult"/>.
        /// </summary>
        /// <returns>
        ///   The data of a single party (e.g., citizen or organization).
        /// </returns>
        /// <exception cref="HttpRequestException"/>
        internal readonly (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber)
            Party(WebApiConfiguration configuration)
        {
            if (this.Results.IsNullOrEmpty())
            {
                throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyPartiesResults);
            }

            string fallbackEmailAddress = string.Empty;
            string fallbackPhoneNumber = string.Empty;
            PartyResult fallbackEmailOwningParty = default;
            PartyResult fallbackPhoneOwningParty = default;

            // Determine which party result should be returned and match the data
            foreach (PartyResult party in this.Results)
            {
                // VALIDATION: Addresses
                if (party.Expansion.DigitalAddresses.IsNullOrEmpty())
                {
                    continue;  // Do not waste time on processing party data which would be for 100% invalid
                }

                Guid prefDigitalAddressId = party.PreferredDigitalAddress.Id;

                // Looking which digital address should be used
                foreach (DigitalAddressLong digitalAddress in party.Expansion.DigitalAddresses)
                {
                    // Recognize what type of digital address it is
                    DistributionChannels distributionChannel =
                        DetermineDistributionChannel(digitalAddress, configuration);

                    // VALIDATION: Distribution channel
                    if (distributionChannel is DistributionChannels.Unknown)
                    {
                        continue;  // Any digital address couldn't be found
                    }

                    (string emailAddress, string phoneNumber) =
                        DetermineDigitalAddresses(digitalAddress, distributionChannel);

                    // VALIDATION: e-mail and phone number
                    if (emailAddress.IsNullOrEmpty() && phoneNumber.IsNullOrEmpty())
                    {
                        continue;  // Empty results cannot be used anyway
                    }

                    // 1. This address is the preferred one and should be prioritized
                    if (prefDigitalAddressId != Guid.Empty &&
                        prefDigitalAddressId == digitalAddress.Id)
                    {
                        return (party, distributionChannel, emailAddress, phoneNumber);
                    }

                    // 2a. This is one of many other addresses to be checked
                    if (fallbackEmailAddress.IsNullOrEmpty() &&  // Only the first encountered one matters
                        emailAddress.IsNotNullOrEmpty())
                    {
                        fallbackEmailAddress = emailAddress;
                        fallbackEmailOwningParty = party;

                        continue;  // The e-mail address always has priority over the phone number.
                                   // If any e-mail address was found during this run then the phone
                                   // number doesn't matter anymore since it will not be returned anyway
                    }

                    if (fallbackPhoneNumber.IsNullOrEmpty() &&  // Only the first encountered one matters
                        phoneNumber.IsNotNullOrEmpty())
                    {
                        fallbackPhoneNumber = phoneNumber;
                        fallbackPhoneOwningParty = party;
                    }
                }
            }

            // 2b. FALLBACK APPROACH: If the party's preferred address couldn't be determined
            //     the email address has priority and the first encountered one should be returned
            if (fallbackEmailAddress.IsNotNullOrEmpty())
            {
                return (fallbackEmailOwningParty, DistributionChannels.Email,
                        EmailAddress: fallbackEmailAddress, PhoneNumber: string.Empty);
            }

            // 2c. FALLBACK APPROACH: If the email also couldn't be determined then alternatively
            //     the first encountered telephone number (for SMS) should be returned instead
            if (fallbackPhoneNumber.IsNotNullOrEmpty())
            {
                return (fallbackPhoneOwningParty, DistributionChannels.Sms,
                        EmailAddress: string.Empty, PhoneNumber: fallbackPhoneNumber);
            }

            // 2d. In the case of worst possible scenario, that preferred address couldn't be determined
            //     neither any existing email address nor telephone number, then process can't be finished
            throw new HttpRequestException(Resources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        #region Helper methods
        /// <summary>
        /// Checks if the value from generic JSON property "Type" is
        /// matching to the predefined names of digital address types.
        /// </summary>
        private static DistributionChannels DetermineDistributionChannel(
            DigitalAddressLong digitalAddress, WebApiConfiguration configuration)
        {
            return digitalAddress.Type == configuration.AppSettings.Variables.EmailGenericDescription()
                ? DistributionChannels.Email

                : digitalAddress.Type == configuration.AppSettings.Variables.PhoneGenericDescription()
                    ? DistributionChannels.Sms

                    // NOTE: Any address type doesn't match the generic address types defined in the app settings
                    : DistributionChannels.Unknown;
        }

        /// <summary>
        /// Tries to retrieve specific email address and telephone number.
        /// </summary>
        private static (string /* Email address */, string /* Telephone number */)
            DetermineDigitalAddresses(DigitalAddressLong digitalAddress, DistributionChannels distributionChannel)
        {
            return distributionChannel switch
            {
                DistributionChannels.Email => (digitalAddress.Value, string.Empty),
                DistributionChannels.Sms   => (string.Empty, digitalAddress.Value),

                _ => (string.Empty, string.Empty)
            };
        }
        #endregion
    }
}