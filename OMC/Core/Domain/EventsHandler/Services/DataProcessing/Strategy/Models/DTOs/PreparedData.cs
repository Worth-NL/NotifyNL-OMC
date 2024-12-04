// © 2024, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using OpenKlant;
using OpenZaak;

namespace EventsHandler.Services.DataProcessing.Strategy.Models.DTOs
{
    /// <summary>
    /// The package of data retrieved from <see cref="INotifyScenario"/>s.
    /// </summary>
    internal readonly struct PreparedData
    {
        /// <inheritdoc cref="ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData"/>
        internal CommonPartyData Party { get; }

        /// <inheritdoc cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case.Uri"/>
        internal Uri? CaseUri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedData"/> struct.
        /// </summary>
        public PreparedData(CommonPartyData party, Uri? caseUri)
        {
            this.Party = party;
            this.CaseUri = caseUri;
        }
    }
}