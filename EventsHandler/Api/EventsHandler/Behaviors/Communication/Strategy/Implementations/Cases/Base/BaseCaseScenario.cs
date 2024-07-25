// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;

namespace EventsHandler.Behaviors.Communication.Strategy.Implementations.Cases.Base
{
    /// <summary>
    /// Common methods and properties used only by case-related scenarios.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal abstract class BaseCaseScenario : BaseScenario
    {
        /// <inheritdoc cref="Case"/>
        protected Case? CachedCase { get; set; }

        /// <inheritdoc cref="CaseStatusType"/>
        protected CaseStatusType? CachedLastCaseStatusType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCaseScenario"/> class.
        /// </summary>
        protected BaseCaseScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Parent
        /// <summary>
        /// Passes the already queried result.
        /// </summary>
        /// <param name="caseStatusType">Type of the case status.</param>
        internal void PassAlreadyQueriedResult(CaseStatusType caseStatusType)
        {
            this.CachedLastCaseStatusType = caseStatusType;
        }
        #endregion

        #region Polymorphic (GetAllNotifyDataAsync)
        /// <inheritdoc cref="BaseScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal sealed override async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            this.QueryContext ??= this.DataQuery.From(notification);
            this.CachedCommonPartyData ??= await this.QueryContext.GetPartyDataAsync();

            return await base.GetAllNotifyDataAsync(notification);
        }
        #endregion

        #region Polymorphic (DropCache)
        /// <summary>
        ///   <inheritdoc cref="BaseScenario.DropCache()"/>
        ///   <para>
        ///   <list type="bullet">
        ///     <item><see cref="CachedCase"/></item>
        ///     <item><see cref="CachedLastCaseStatusType"/></item>
        ///   </list>
        ///   </para>
        /// </summary>
        protected override void DropCache()
        {
            base.DropCache();

            this.CachedCase = null;
            this.CachedLastCaseStatusType = null;
        }
        #endregion
    }
}