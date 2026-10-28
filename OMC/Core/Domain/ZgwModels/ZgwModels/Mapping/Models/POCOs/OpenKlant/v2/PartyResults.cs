// © 2024, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using System.Text.Json.Serialization;
using ZgwModels.Exceptions;
using ZgwModels.Mapping.Enums.OpenKlant;
using ZgwModels.Mapping.Models.Interfaces;
using ZgwModels.Properties;

namespace ZgwModels.Mapping.Models.POCOs.OpenKlant.v2
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
        /// <param name="configuration"><inheritdoc cref="OmcConfiguration" path="/summary"/></param>
        /// <param name="caseIdentifier">The Case identifier used to select the digital address with the highest priority if match is found.</param>
        /// <param name="requireDigitalAddress">
        ///   When <see langword="true"/> (default), no usable digital address (email or phone) among any
        ///   of the results is a hard failure - this is the only delivery option classic ZGW scenarios have.
        ///   Callers with a non-digital fallback of their own (e.g. MOBB's postal-letter fallback) can pass
        ///   <see langword="false"/> to instead get the first result's identity back with an empty address,
        ///   so they can still resolve the party UUID/name and route to that fallback themselves.
        /// </param>
        /// <param name="requiredChannel">
        ///   When set, only digital addresses of that channel are considered at all, so an address of any
        ///   other channel can neither be selected nor block one that could have been. Scenarios that can
        ///   only deliver one way need this: without it, a party whose preferred address is a phone number
        ///   reads as having no e-mail, because the search stops at the preferred address and never reaches
        ///   the e-mail behind it.
        ///   <para>
        ///     Leave it unset (default) to consider every channel, which is what the classic ZGW scenarios
        ///     want - they follow whichever channel the party prefers.
        ///   </para>
        /// </param>
        /// <returns>
        ///   The data of a single party (e.g., citizen or organization).
        /// </returns>
        /// <exception cref="PartyNotFoundException">No party matched the search.</exception>
        /// <exception cref="HttpRequestException"/>
        public readonly (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber, string Reason)
            Party(OmcConfiguration configuration,
                string? caseIdentifier = null,
                bool requireDigitalAddress = true,
                DistributionChannels? requiredChannel = null)
        {
            // Validation #1: Results
            if (this.Results.IsEmpty())
            {
                throw new PartyNotFoundException(ZgwResources.HttpRequest_ERROR_EmptyPartiesResults);
            }

            // Determine which party result should be returned and match the data
            foreach (PartyResult partyResult in this.Results)
            {
                // Validation #2: Addresses
                if (partyResult.Expansion.DigitalAddresses.IsEmpty())
                {
                    continue; // Do not waste time on processing party data which would be for 100% invalid
                }

                if (TrySelectAddress(configuration, partyResult, caseIdentifier, requiredChannel,
                        out DistributionChannels distributionChannel, out string emailAddress, out string phoneNumber, out string reason))
                {
                    return (partyResult, distributionChannel, emailAddress, phoneNumber, reason);
                }
            }

            // Nobody had a digital address that qualifies. A tolerant caller still gets the first result's
            // identity back, with no resolved channel, so it can proceed via a fallback that doesn't need a
            // digital address.
            if (!requireDigitalAddress)
            {
                return (this.Results[0], DistributionChannels.Unknown, string.Empty, string.Empty,
                    "no qualifying digital address on file; caller has a non-digital fallback of its own");
            }

            throw new HttpRequestException(ZgwResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        /// <inheritdoc>
        ///     <cref>Party(OmcConfiguration)</cref>
        /// </inheritdoc>
        public static (PartyResult, DistributionChannels, string EmailAddress, string PhoneNumber, string Reason)
            Party(
                OmcConfiguration configuration,
                PartyResult partyResult,
                string? caseIdentifier = null)
        {
            // Validation #1: Addresses
            if (partyResult.Expansion.DigitalAddresses.IsEmpty())
            {
                throw new HttpRequestException(ZgwResources.HttpRequest_ERROR_NoDigitalAddresses);
            }

            if (TrySelectAddress(configuration, partyResult, caseIdentifier, requiredChannel: null,
                    out DistributionChannels distributionChannel, out string emailAddress, out string phoneNumber, out string reason))
            {
                return (partyResult, distributionChannel, emailAddress, phoneNumber, reason);
            }

            throw new HttpRequestException(ZgwResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        #region Helper methods
        /// <summary>
        /// The "referentie" Den Haag marks a party's portal e-mail address with, as it does not fill "voorkeursDigitaalAdres".
        /// </summary>
        public const string PortaalvoorkeurReference = "portaalvoorkeur";

        /// <summary>
        /// Selects the digital address to notify the party on, in this order of priority:
        /// <list type="number">
        ///   <item>the address whose "referentie" is the case identifier;</item>
        ///   <item>the party's "voorkeursDigitaalAdres";</item>
        ///   <item>the address marked "isStandaardAdres" - the e-mail address when both an e-mail and a phone number are;</item>
        ///   <item>the address whose "referentie" is <see cref="PortaalvoorkeurReference"/> - again e-mail before phone.</item>
        /// </list>
        /// Nothing else qualifies: an address that is none of these is never picked just for being there.
        /// </summary>
        /// <returns><see langword="true"/> when an address qualified.</returns>
        private static bool TrySelectAddress(
            OmcConfiguration configuration,
            PartyResult party,
            string? caseIdentifier,
            DistributionChannels? requiredChannel,
            out DistributionChannels distributionChannel,
            out string emailAddress,
            out string phoneNumber,
            out string reason)
        {
            Guid? preferredAddressId = party.PreferredDigitalAddress?.Id;

            DigitalAddressLong? preferred = null;
            DigitalAddressLong? standardEmail = null;
            DigitalAddressLong? standardPhone = null;
            DigitalAddressLong? portalEmail = null;
            DigitalAddressLong? portalPhone = null;

            foreach (DigitalAddressLong digitalAddress in party.Expansion.DigitalAddresses)
            {
                DistributionChannels channel = DetermineDistributionChannel(digitalAddress, configuration);

                // Skip invalid distribution channels
                if (channel == DistributionChannels.Unknown)
                {
                    continue;
                }

                // Skip channels the caller cannot deliver on. Deliberately before every other check, so an
                // address of the wrong channel cannot win on "referentie" or on being the party's preferred
                // one, and cannot stop the loop from reaching an address the caller can actually use.
                if (requiredChannel != null && channel != requiredChannel)
                {
                    continue;
                }

                // Skip addresses without a value
                if (digitalAddress.Value.IsNullOrEmpty())
                {
                    continue;
                }

                // 1. Linked to the case: nothing beats it
                if (caseIdentifier != null && digitalAddress.Reference == caseIdentifier)
                {
                    return Select(digitalAddress, channel, $"digital address is linked to case \"{caseIdentifier}\"",
                        out distributionChannel, out emailAddress, out phoneNumber, out reason);
                }

                // 2. The party's preferred address
                if (preferred == null && preferredAddressId != Guid.Empty && preferredAddressId == digitalAddress.Id)
                {
                    preferred = digitalAddress;
                }

                // 3. The default address of its kind
                if (digitalAddress.IsStandardAddress)
                {
                    if (channel == DistributionChannels.Email)
                    {
                        standardEmail ??= digitalAddress;
                    }
                    else
                    {
                        standardPhone ??= digitalAddress;
                    }
                }

                // 4. Marked as the portal address
                if (string.Equals(digitalAddress.Reference, PortaalvoorkeurReference, StringComparison.OrdinalIgnoreCase))
                {
                    if (channel == DistributionChannels.Email)
                    {
                        portalEmail ??= digitalAddress;
                    }
                    else
                    {
                        portalPhone ??= digitalAddress;
                    }
                }
            }

            (DigitalAddressLong? Address, string Reason)[] candidates =
            [
                (preferred, "this is the party's preferred digital address in OpenKlant"),
                (standardEmail, "this is the party's default (isStandaardAdres) e-mail address"),
                (standardPhone, "this is the party's default (isStandaardAdres) phone number, and it has no default e-mail address"),
                (portalEmail, $"this e-mail address is marked \"{PortaalvoorkeurReference}\""),
                (portalPhone, $"this phone number is marked \"{PortaalvoorkeurReference}\", and no e-mail address is")
            ];

            foreach ((DigitalAddressLong? address, string candidateReason) in candidates)
            {
                if (address is { } selected)
                {
                    return Select(selected, DetermineDistributionChannel(selected, configuration), candidateReason,
                        out distributionChannel, out emailAddress, out phoneNumber, out reason);
                }
            }

            distributionChannel = DistributionChannels.Unknown;
            emailAddress = string.Empty;
            phoneNumber = string.Empty;
            reason = string.Empty;

            return false;
        }

        private static bool Select(
            DigitalAddressLong digitalAddress,
            DistributionChannels channel,
            string reasonText,
            out DistributionChannels distributionChannel,
            out string emailAddress,
            out string phoneNumber,
            out string reason)
        {
            (emailAddress, phoneNumber) = DetermineDigitalAddresses(digitalAddress, channel);
            distributionChannel = channel;
            reason = reasonText;

            return true;
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