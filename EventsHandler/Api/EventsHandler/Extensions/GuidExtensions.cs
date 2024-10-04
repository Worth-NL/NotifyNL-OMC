// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Settings.Extensions;
using ConfigurationExtensions = EventsHandler.Services.Settings.Extensions.ConfigurationExtensions;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Guid"/>s.
    /// </summary>
    internal static class GuidExtensions
    {
        /// <summary>
        /// Restores <see cref="Case.Uri"/> based on provided <see cref="Case"/> ID in <see cref="Guid"/> format
        /// and the respective domain retrieved from <see cref="WebApiConfiguration"/>.
        /// </summary>
        /// <param name="caseId">The <see cref="Case"/> ID.</param>
        /// <returns>
        ///   The recreated <see cref="Case.Uri"/>.
        /// </returns>
        internal static Uri RecreateCaseUri(this Guid? caseId)
            => RecreateCaseUri(caseId ?? Guid.Empty);

        /// <inheritdoc cref="RecreateCaseUri(Guid?)"/>
        internal static Uri RecreateCaseUri(this Guid caseId)
        {
            const string caseUri = "https://{0}/zaken/{1}";
            
            return string.Format(caseUri, ConfigurationExtensions.OpenZaakDomain(), caseId)
                         .GetValidUri();
        }
        
        /// <summary>
        /// Restores <see cref="CommonPartyData.Uri"/> based on provided <see cref="CommonPartyData"/> ID in <see cref="Guid"/> format
        /// and the respective domain retrieved from <see cref="WebApiConfiguration"/>.
        /// </summary>
        /// <param name="partyId">The <see cref="CommonPartyData"/> ID.</param>
        /// <returns>
        ///   The recreated <see cref="CommonPartyData.Uri"/>.
        /// </returns>
        internal static Uri RecreatePartyUri(this Guid partyId)
        {
            const string partyUri = "https://{0}/klanten/{1}";
            
            return string.Format(partyUri, ConfigurationExtensions.OpenKlantDomain(), partyId)
                .GetValidUri();
        }
    }
}