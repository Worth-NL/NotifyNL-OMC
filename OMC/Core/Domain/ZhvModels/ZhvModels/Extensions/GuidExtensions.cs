// © 2024, Worth Systems.

using Common.Settings.Extensions;
using ConfigExtensions = Common.Settings.Extensions.ConfigExtensions;

namespace ZhvModels.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Guid"/>s.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Restores <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case.Uri"/> based on provided <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case"/> ID in <see cref="Guid"/> format
        /// and the respective domain retrieved from <see cref="Common.Settings.Configuration.WebApiConfiguration"/>.
        /// </summary>
        /// <param name="caseId">The <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case"/> ID.</param>
        /// <returns>
        ///   The recreated <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case.Uri"/>.
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
        /// Restores <see cref="ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData.Uri"/> based on provided <see cref="ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData"/> ID in <see cref="Guid"/> format
        /// and the respective domain retrieved from <see cref="Common.Settings.Configuration.WebApiConfiguration"/>.
        /// </summary>
        /// <param name="partyId">The <see cref="ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData"/> ID.</param>
        /// <returns>
        ///   The recreated <see cref="ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData.Uri"/>.
        /// </returns>
        public static Uri RecreatePartyUri(this Guid partyId)
        {
            const string partyUri = "https://{0}/klanten/{1}";
            
            return string.Format(partyUri, ConfigExtensions.OpenKlantDomain(), partyId)
                .GetValidUri();
        }
    }
}