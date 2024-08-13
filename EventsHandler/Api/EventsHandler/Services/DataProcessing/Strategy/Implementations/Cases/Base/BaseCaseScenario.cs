// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base
{
    /// <summary>
    /// Common methods and properties used only by case-related scenarios.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal abstract class BaseCaseScenario : BaseScenario
    {
        /// <inheritdoc cref="Case"/>
        protected Case? CachedCase { get; set; }

        /// <inheritdoc cref="CaseType"/>
        protected CaseType? CachedCaseType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCaseScenario"/> class.
        /// </summary>
        protected BaseCaseScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Parent
        /// <summary>
        /// Passes an already queried result.
        /// </summary>
        /// <param name="caseStatusType">Type of the case status.</param>
        internal void CacheCaseType(CaseType caseStatusType)
        {
            this.CachedCaseType = caseStatusType;
        }
        #endregion

        #region Polymorphic (DropCache)
        /// <summary>
        ///   <inheritdoc cref="BaseScenario.DropCache()"/>
        ///   <para>
        ///   <list type="bullet">
        ///     <item><see cref="CachedCase"/></item>
        ///     <item><see cref="CachedCaseType"/></item>
        ///   </list>
        ///   </para>
        /// </summary>
        protected override void DropCache()
        {
            base.DropCache();

            this.CachedCase = null;
            this.CachedCaseType = null;
        }
        #endregion
    }
}