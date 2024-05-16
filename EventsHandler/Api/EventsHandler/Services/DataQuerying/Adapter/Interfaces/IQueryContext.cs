// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;

namespace EventsHandler.Services.DataQuerying.Adapter.Interfaces
{
    /// <summary>
    /// The adapter combining functionalities from other data-querying-related interfaces.
    /// </summary>
    internal interface IQueryContext
    {
        /// <inheritdoc cref="IQueryBase"/>
        internal IQueryBase QueryBase { get; set; }

        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        internal Task<Case> GetCaseAsync();
        
        /// <summary>
        /// Gets the details of a specific citizen from "OpenKlant" Web service.
        /// </summary>
        internal Task<CitizenDetails> GetCitizenDetailsAsync();

        /// <summary>
        /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        internal Task<CaseStatuses> GetCaseStatusesAsync();

        /// <summary>
        /// Gets the type of <see cref="CaseStatus"/>.
        /// </summary>
        internal Task<CaseStatusType> GetLastCaseStatusTypeAsync(CaseStatuses statuses);
    }
}