﻿// © 2024, Worth Systems.

using ZhvModels.Mapping.Enums.OpenKlant;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant.Converters
{
    /// <summary>
    /// Converts subject data from different versions of "OpenKlant" into a unified <see cref="CommonPartyData"/>.
    /// </summary>
    public static class CommonPartyDataConverter
    {
        /// <summary>
        /// Converts <see cref="v1.PartyResult"/> from "OpenKlant" (1.0) Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        public static CommonPartyData ConvertToUnified(this v1.PartyResult party)
        {
            return new CommonPartyData
            {
                Uri                 = party.Uri,
                Name                = party.Name,
                SurnamePrefix       = party.SurnamePrefix,
                Surname             = party.Surname,
                DistributionChannel = party.DistributionChannel,
                EmailAddress        = party.EmailAddress,
                TelephoneNumber     = party.TelephoneNumber,
                Gender              = party.SubjectIdentification.Gender
            };
        }

        /// <summary>
        /// Converts <see cref="v2.PartyResult"/> from "OpenKlant" (2.0) Web API service.
        /// </summary>
        /// <returns>
        ///   The unified <see cref="CommonPartyData"/> DTO model.
        /// </returns>
        public static CommonPartyData ConvertToUnified(this (v2.PartyResult Party, DistributionChannels DistributionChannel, string EmailAddress, string PhoneNumber) data)
        {
            return new CommonPartyData
            {
                Uri                 = data.Party.Uri,
                Name                = data.Party.Identification.Details.Name,
                SurnamePrefix       = data.Party.Identification.Details.SurnamePrefix,
                Surname             = data.Party.Identification.Details.Surname,
                DistributionChannel = data.DistributionChannel,
                EmailAddress        = data.EmailAddress,
                TelephoneNumber     = data.PhoneNumber,
                Gender              = data.Party.SubjectIdentification.Gender
            };
        }
    }
}