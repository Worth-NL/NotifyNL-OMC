// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenZaak" Web service.
    /// </summary>
    internal interface IQueryZaak
    {
        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<Case> GetCaseAsync(IQueryBase queryBase);
        
        /// <summary>
        /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<CaseStatuses> GetCaseStatusesAsync(IQueryBase queryBase);
        
        /// <summary>
        /// Gets the type of <see cref="CaseStatus"/> from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<CaseStatusType> GetLastCaseStatusTypeAsync(IQueryBase queryBase, CaseStatuses statuses);
        
        /// <summary>
        /// Gets BSN number of a specific citizen from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<string> GetBsnNumberAsync(IQueryBase queryBase);
    }
}