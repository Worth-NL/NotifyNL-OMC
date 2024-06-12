// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Case finished" scenario.
    /// </summary>
    /// <seealso cref="CaseStatusUpdatedScenario"/>
    internal sealed class CaseFinishedScenario : CaseStatusUpdatedScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseFinishedScenario"/> class.
        /// </summary>
        public CaseFinishedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.ZaakClose();

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
          => this.Configuration.User.TemplateIds.Email.ZaakClose();
        #endregion
    }
}