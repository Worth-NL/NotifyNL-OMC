using Common.Settings.Configuration;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenZaak.Documents;

namespace WebQueries.DataQuerying.Strategies.Queries.Documenten.Interfaces
{
    /// <summary>
    /// Methods for querying documenten (EnkelvoudigInformatieObjecten) from OpenZaak Documenten API.
    /// </summary>
    public interface IQueryDocumenten : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="OmcConfiguration"/>
        protected internal OmcConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenZaak (Documenten)";

        /// <summary>
        /// Gets a specific SingularInformationObject by its UUID from the Documenten API.
        /// </summary>
        /// <param name="queryBase">The query base providing HTTP context.</param>
        /// <param name="documentUuid">The UUID of the document.</param>
        /// <returns>The document metadata.</returns>
        internal Task<SingularInformationObject> TryGetDocumentAsync(IQueryBase queryBase, Guid documentUuid);

        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.ZGW.Endpoint.Documenten();
    }
}