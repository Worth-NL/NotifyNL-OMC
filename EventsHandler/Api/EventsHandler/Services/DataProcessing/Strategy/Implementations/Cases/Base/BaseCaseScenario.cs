﻿// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Settings.Configuration;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base
{
    /// <summary>
    /// Common methods and properties used only by case-related scenarios.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal abstract class BaseCaseScenario : BaseScenario
    {
        /// <inheritdoc cref="Mapping.Models.POCOs.OpenZaak.Case"/>
        protected Case Case { get; set; }

        /// <inheritdoc cref="Mapping.Models.POCOs.OpenZaak.CaseType"/>
        protected CaseType? CaseType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCaseScenario"/> class.
        /// </summary>
        protected BaseCaseScenario(
            WebApiConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotificationEvent, NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Parent
        /// <summary>
        /// Passes an already queried <see cref="Mapping.Models.POCOs.OpenZaak.CaseType"/> result.
        /// </summary>
        /// <param name="caseType">Type of the <see cref="Mapping.Models.POCOs.OpenZaak.Case"/>.</param>
        internal void Cache(CaseType caseType)
        {
            this.CaseType = caseType;
        }
        #endregion
    }
}