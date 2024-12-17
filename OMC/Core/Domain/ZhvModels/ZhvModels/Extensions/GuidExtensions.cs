// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Settings.Extensions;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace ZhvModels.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Guid"/>s.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Restores <see cref="Case.Uri"/> based on provided <see cref="Case"/> ID in <see cref="Guid"/> format
        /// and the respective domain retrieved from <see cref="OmcConfiguration"/>.
        /// </summary>
        /// <param name="caseId">The <see cref="Case"/> ID.</param>
        /// <returns>
        ///   The recreated <see cref="Case.Uri"/>.
        /// </returns>
        public static Uri RecreateCaseUri(this Guid? caseId)
            => RecreateCaseUri(caseId ?? Guid.Empty);

        /// <inheritdoc cref="RecreateCaseUri(Guid?)"/>
        public static Uri RecreateCaseUri(this Guid caseId)
        {
            const string caseUri = "https://{0}/zaken/{1}";
            
            return string.Format(caseUri, ConfigExtensions.OpenZaakDomain(), caseId)
                         .GetValidUri();
        }
        
        /// <summary>
        /// Restores <see cref="CommonPartyData.Uri"/> based on provided <see cref="CommonPartyData"/> ID in <see cref="Guid"/> format
        /// and the respective domain retrieved from <see cref="OmcConfiguration"/>.
        /// </summary>
        /// <param name="partyId">The <see cref="CommonPartyData"/> ID.</param>
        /// <returns>
        ///   The recreated <see cref="CommonPartyData.Uri"/>.
        /// </returns>
        public static Uri RecreatePartyUri(this Guid partyId)
        {
            const string partyUri = "https://{0}/klanten/{1}";
            
            return string.Format(partyUri, ConfigExtensions.OpenKlantDomain(), partyId)
                .GetValidUri();
        }
    }
}