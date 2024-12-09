﻿// © 2024, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Models.DTOs.Processing
{
    /// <summary>
    /// The package of data retrieved from <see cref="INotifyScenario"/>s.
    /// </summary>
    internal readonly struct PreparedData
    {
        /// <inheritdoc cref="CommonPartyData"/>
        internal CommonPartyData Party { get; }

        /// <inheritdoc cref="Case.Uri"/>
        internal Uri? CaseUri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedData"/> struct.
        /// </summary>
        internal PreparedData(CommonPartyData party, Uri? caseUri)
        {
            this.Party = party;
            this.CaseUri = caseUri;
        }
    }
}