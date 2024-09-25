// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;

namespace EventsHandler.Services.DataProcessing.Strategy.Models.DTOs
{
    /// <summary>
    /// The package of data retrieved from <see cref="INotifyScenario"/>s.
    /// </summary>
    internal readonly struct PreparedData
    {
        /// <inheritdoc cref="CommonPartyData"/>
        internal CommonPartyData Party { get; }

        /// <inheritdoc cref="Case.Uri"/>
        internal Uri CaseUri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedData"/> struct.
        /// </summary>
        public PreparedData(CommonPartyData party, Uri caseUri)
        {
            this.Party = party;
            this.CaseUri = caseUri;
        }
    }
}