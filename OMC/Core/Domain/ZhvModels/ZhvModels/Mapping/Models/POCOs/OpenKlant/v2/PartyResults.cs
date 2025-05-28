﻿// © 2024, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.OpenKlant;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Properties;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant.v2
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
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; set; }

        /// <inheritdoc cref="PartyResult"/>
        [JsonRequired]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<PartyResult> Results { get; set; } = [];

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
        public readonly (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber)
            Party(OmcConfiguration configuration,
                string? caseIdentifier = null)
        {
            // Validation #1: Results
            if (this.Results.IsEmpty())
            {
                throw new HttpRequestException(ZhvResources.HttpRequest_ERROR_EmptyPartiesResults);
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
                if (partyResult.Expansion.DigitalAddresses.IsEmpty())
                {
                    continue; // Do not waste time on processing party data which would be for 100% invalid
                }

                // Determine which address is preferred
                if (IsPreferredFound(configuration, partyResult,
                        ref fallbackEmailOwningParty, ref fallbackPhoneOwningParty, ref distributionChannel,
                        ref fallbackEmailAddress, ref fallbackPhoneNumber, caseIdentifier))
                {
                    return (partyResult, distributionChannel, fallbackEmailAddress, fallbackPhoneNumber);
                }
            }

            // Pick any matching address
            return GetMatchingContactDetails(
                fallbackEmailOwningParty, fallbackPhoneOwningParty,
                fallbackEmailAddress, fallbackPhoneNumber);
        }

        /// <inheritdoc>
        ///     <cref>Party(OmcConfiguration)</cref>
        /// </inheritdoc>
        public static (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber)
            Party(
                OmcConfiguration configuration,
                PartyResult partyResult,
                string? caseIdentifier = null)
        {
            // Validation #1: Addresses
            if (partyResult.Expansion.DigitalAddresses.IsEmpty())
            {
                throw new HttpRequestException(ZhvResources.HttpRequest_ERROR_NoDigitalAddresses);
            }

            PartyResult fallbackEmailOwningParty = default;
            PartyResult fallbackPhoneOwningParty = default;
            DistributionChannels distributionChannel = default;
            string fallbackEmailAddress = string.Empty;
            string fallbackPhoneNumber = string.Empty;

            // Determine which address is preferred
            if (IsPreferredFound(configuration, partyResult,
                    ref fallbackEmailOwningParty, ref fallbackPhoneOwningParty, ref distributionChannel,
                    ref fallbackEmailAddress, ref fallbackPhoneNumber, caseIdentifier))
            {
                return (partyResult, distributionChannel, fallbackEmailAddress, fallbackPhoneNumber);
            }

            // Pick any matching address
            return GetMatchingContactDetails(
                fallbackEmailOwningParty, fallbackPhoneOwningParty,
                fallbackEmailAddress, fallbackPhoneNumber);
        }

        #region Helper methods

        // NOTE: Checks preferred contact address
        private static bool IsPreferredFound(
            OmcConfiguration configuration,
            PartyResult party,
            ref PartyResult fallbackEmailOwningParty,
            ref PartyResult fallbackPhoneOwningParty,
            ref DistributionChannels distributionChannel,
            ref string fallbackEmailAddress,
            ref string fallbackPhoneNumber,
            string? caseIdentifier = null)
        {
            Guid prefDigitalAddressId = party.PreferredDigitalAddress.Id;
            bool
                prefDigitalAddressFoundFlag =
                    false; //Flag to indicate that in case no match was made on case identifier a preferred digital address was found

            foreach (DigitalAddressLong digitalAddress in party.Expansion.DigitalAddresses)
            {
                // Determine distribution channel
                DistributionChannels tempDistributionChannel = DetermineDistributionChannel(digitalAddress, configuration);

                // Skip invalid distribution channels
                if (tempDistributionChannel == DistributionChannels.Unknown)
                {
                    continue;
                }

                (string emailAddress, string phoneNumber) =
                    DetermineDigitalAddresses(digitalAddress, tempDistributionChannel);

                // Skip if both email and phone number are empty
                if (emailAddress.IsNullOrEmpty() && phoneNumber.IsNullOrEmpty())
                {
                    continue;
                }

                // Check if digital address matches case identifier. Proceeds preferred digital address
                if (caseIdentifier != null &&
                    digitalAddress.Reference == caseIdentifier)
                {
                    // Set fallback values when case identifier matches, and return true
                    SetPreferredAddress(party, digitalAddress, configuration, ref fallbackEmailOwningParty,
                        ref fallbackPhoneOwningParty, ref fallbackEmailAddress, ref fallbackPhoneNumber, ref distributionChannel);
                    return true;
                }

                // Skip when preferred digital address has already been found
                if (prefDigitalAddressFoundFlag)
                {
                    continue;
                }

                // Check for preferred digital address after case identifier match
                if (prefDigitalAddressId != Guid.Empty && prefDigitalAddressId == digitalAddress.Id)
                {
                    // Update fallback values with the preferred address
                    SetPreferredAddress(party, digitalAddress, configuration, ref fallbackEmailOwningParty,
                        ref fallbackPhoneOwningParty, ref fallbackEmailAddress, ref fallbackPhoneNumber, ref distributionChannel);
                    // Set flag that a preferred address is found. This can still be overridden by case identification match
                    prefDigitalAddressFoundFlag = true;
                }
                else if (emailAddress.IsNotNullOrEmpty() && fallbackEmailAddress.IsNullOrEmpty())
                {
                    // Update fallback email if not already set
                    fallbackEmailAddress = emailAddress;
                    fallbackEmailOwningParty = party;
                }
                else if (phoneNumber.IsNotNullOrEmpty() && fallbackPhoneNumber.IsNullOrEmpty())
                {
                    // Update fallback phone if not already set
                    fallbackPhoneNumber = phoneNumber;
                    fallbackPhoneOwningParty = party;
                }
            }
            // If nothing found return false
            return prefDigitalAddressFoundFlag;
        }

        private static void SetPreferredAddress(
            PartyResult party,
            DigitalAddressLong digitalAddress,
            OmcConfiguration configuration,
            // ReSharper disable once RedundantAssignment
            ref PartyResult fallbackEmailOwningParty,
            // ReSharper disable once RedundantAssignment
            ref PartyResult fallbackPhoneOwningParty,
            // ReSharper disable once RedundantAssignment
            ref string fallbackEmailAddress,
            // ReSharper disable once RedundantAssignment
            ref string fallbackPhoneNumber,
            // ReSharper disable once RedundantAssignment
            ref DistributionChannels distributionChannel)
        {
            DistributionChannels preferredDistributionChannel = DetermineDistributionChannel(digitalAddress, configuration);

            // Determine email and phone for this address
            (string emailAddress, string phoneNumber) = DetermineDigitalAddresses(digitalAddress,
                preferredDistributionChannel);

            // Update the fallback values
            fallbackEmailOwningParty = party;
            fallbackEmailAddress = emailAddress;
            fallbackPhoneOwningParty = party;
            fallbackPhoneNumber = phoneNumber;
            distributionChannel = preferredDistributionChannel;
        }

        // NOTE: Checks alternative contact addresses
        private static (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber)
            GetMatchingContactDetails(
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
            throw new HttpRequestException(ZhvResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        /// <summary>
        /// Checks if the value from generic JSON property "Type" is
        /// matching to the predefined names of digital address types.
        /// </summary>
        private static DistributionChannels DetermineDistributionChannel(
            DigitalAddressLong digitalAddress, OmcConfiguration configuration)
        {
            return string.Equals(digitalAddress.Type, configuration.AppSettings.Variables.EmailGenericDescription(), StringComparison.CurrentCultureIgnoreCase)
                ? DistributionChannels.Email
                : digitalAddress.Type.Contains(configuration.AppSettings.Variables.PhoneGenericDescription(), StringComparison.CurrentCultureIgnoreCase)
                    ? DistributionChannels.Sms
                    // NOTE: We use Contains because there is a difference between v1 "Telefoon" and v2 (v2.4.0 +) "telefoonnummer".
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
                DistributionChannels.Sms => (string.Empty, digitalAddress.Value),

                _ => (string.Empty, string.Empty)
            };
        }

        #endregion
    }
}