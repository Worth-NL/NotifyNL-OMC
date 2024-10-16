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
            // Validation #1: Results
            if (this.Results.IsNullOrEmpty())
            {
                throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyPartiesResults);
            }

            PartyResult fallbackEmailOwningParty = default;
            PartyResult fallbackPhoneOwningParty = default;
            DistributionChannels distributionChannel = default;
            string fallbackEmailAddress = string.Empty;
            string fallbackPhoneNumber = string.Empty;

            // Determine which party result should be returned and match the data
            foreach (PartyResult partyResult in this.Results)
            {
                // Validation #2: Addresses
                if (partyResult.Expansion.DigitalAddresses.IsNullOrEmpty())
                {
                    continue;  // Do not waste time on processing party data which would be for 100% invalid
                }

                if (IsPreferredFound(configuration, partyResult,
                        ref fallbackEmailOwningParty, ref fallbackPhoneOwningParty, ref distributionChannel,
                        ref fallbackEmailAddress, ref fallbackPhoneNumber))
                {
                    return (partyResult, distributionChannel, fallbackEmailAddress, fallbackPhoneNumber);
                }
            }

            return GetMatchingContactDetails(
                fallbackEmailOwningParty, fallbackPhoneOwningParty,
                fallbackEmailAddress, fallbackPhoneNumber);
        }

        /// <inheritdoc cref="Party(WebApiConfiguration)"/>
        internal static (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber)
            Party(WebApiConfiguration configuration, PartyResult partyResult)
        {
            // Validation #1: Addresses
            if (partyResult.Expansion.DigitalAddresses.IsNullOrEmpty())
            {
                throw new HttpRequestException(Resources.HttpRequest_ERROR_NoDigitalAddresses);
            }

            PartyResult fallbackEmailOwningParty = default;
            PartyResult fallbackPhoneOwningParty = default;
            DistributionChannels distributionChannel = default;
            string fallbackEmailAddress = string.Empty;
            string fallbackPhoneNumber = string.Empty;

            if (IsPreferredFound(configuration, partyResult,
                    ref fallbackEmailOwningParty, ref fallbackPhoneOwningParty, ref distributionChannel,
                    ref fallbackEmailAddress, ref fallbackPhoneNumber))
            {
                return (partyResult, distributionChannel, fallbackEmailAddress, fallbackPhoneNumber);
            }

            return GetMatchingContactDetails(
                fallbackEmailOwningParty, fallbackPhoneOwningParty,
                fallbackEmailAddress, fallbackPhoneNumber);
        }

        #region Helper methods
        // NOTE: Checks preferred contact address
        private static bool IsPreferredFound(WebApiConfiguration configuration, PartyResult party,
            ref PartyResult fallbackEmailOwningParty,
            ref PartyResult fallbackPhoneOwningParty,
            ref DistributionChannels distributionChannel,
            ref string fallbackEmailAddress,
            ref string fallbackPhoneNumber)
        {
            Guid prefDigitalAddressId = party.PreferredDigitalAddress.Id;

            // Looking which digital address should be used
            foreach (DigitalAddressLong digitalAddress in party.Expansion.DigitalAddresses)
            {
                // Recognize what type of digital address it is
                distributionChannel = DetermineDistributionChannel(digitalAddress, configuration);

                // Validation #1: Distribution channel
                if (distributionChannel is DistributionChannels.Unknown)
                {
                    continue;  // Any digital address couldn't be found
                }

                (string emailAddress, string phoneNumber) =
                    DetermineDigitalAddresses(digitalAddress, distributionChannel);

                // Validation #2: E-mail and phone number
                if (emailAddress.IsNullOrEmpty() && phoneNumber.IsNullOrEmpty())
                {
                    continue;  // Empty results cannot be used anyway
                }

                // 1. This address is the preferred one and should be prioritized
                if (prefDigitalAddressId != Guid.Empty &&
                    prefDigitalAddressId == digitalAddress.Id)
                {
                    fallbackEmailOwningParty = party;
                    fallbackEmailAddress = emailAddress;

                    fallbackPhoneOwningParty = party;
                    fallbackPhoneNumber = phoneNumber;

                    return true;  // Preferred address is found
                }

                // 2a. This is one of many other addresses to be checked (e-mail has priority)
                if (fallbackEmailAddress.IsNullOrEmpty() &&  // Only the first encountered one matters
                    emailAddress.IsNotNullOrEmpty())
                {
                    fallbackEmailAddress = emailAddress;
                    fallbackEmailOwningParty = party;

                    continue;  // The e-mail address always has priority over the phone number.
                               // If any e-mail address was found during this run then the phone
                               // number doesn't matter anymore since it will not be returned anyway
                }

                // 2b. This address is not preferred but could be the only which was found as matching
                if (fallbackPhoneNumber.IsNullOrEmpty() &&  // Only the first encountered one matters
                    phoneNumber.IsNotNullOrEmpty())
                {
                    fallbackPhoneNumber = phoneNumber;
                    fallbackPhoneOwningParty = party;
                }
            }

            return false;
        }

        // NOTE: Checks alternative contact addresses
        private static (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber) GetMatchingContactDetails(
            PartyResult fallbackEmailOwningParty, PartyResult fallbackPhoneOwningParty,
            string fallbackEmailAddress, string fallbackPhoneNumber)
        {
            // 3a. FALLBACK APPROACH: If the party's preferred address couldn't be determined
            //     the email address has priority and the first encountered one should be returned
            if (fallbackEmailAddress.IsNotNullOrEmpty())
            {
                return (fallbackEmailOwningParty, DistributionChannels.Email,
                        EmailAddress: fallbackEmailAddress, PhoneNumber: string.Empty);
            }

            // 3b. FALLBACK APPROACH: If the email also couldn't be determined then alternatively
            //     the first encountered telephone number (for SMS) should be returned instead
            if (fallbackPhoneNumber.IsNotNullOrEmpty())
            {
                return (fallbackPhoneOwningParty, DistributionChannels.Sms,
                        EmailAddress: string.Empty, PhoneNumber: fallbackPhoneNumber);
            }

            // 3c. In the case of worst possible scenario, that preferred address couldn't be determined
            //     neither any existing email address nor telephone number, then process can't be finished
            throw new HttpRequestException(Resources.HttpRequest_ERROR_NoDigitalAddresses);
        }

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