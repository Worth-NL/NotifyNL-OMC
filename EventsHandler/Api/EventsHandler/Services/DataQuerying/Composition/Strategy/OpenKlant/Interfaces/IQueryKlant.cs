// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenKlant" Web service.
    /// </summary>
    internal interface IQueryKlant
    {
        /// <summary>
        /// Gets the details of a specific citizen from "OpenKlant" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<CitizenDetails> GetCitizenDetailsAsync(IQueryBase queryBase, string bsnNumber);
    }
}