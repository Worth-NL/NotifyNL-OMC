// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;
using CitizenData = EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1.CitizenData;

namespace EventsHandler.Behaviors.Communication.Strategy
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy representing not implemented scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class NotImplementedScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseScenario"/> class.
        /// </summary>
        public NotImplementedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic
        /// <inheritdoc cref="BaseScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal override Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            return NotImplemented<Task<NotifyData[]>>();
        }

        /// <inheritdoc cref="BaseScenario.GetSmsNotifyData(Case, CitizenData)"/>
        protected override NotifyData GetSmsNotifyData(Case @case, CitizenData citizen)
        {
            return NotImplemented<NotifyData>();
        }

        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
        {
            return NotImplemented<string>();
        }

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(Case, CitizenData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(Case @case, CitizenData citizen)
        {
            return NotImplemented<Dictionary<string, object>>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailNotifyData(Case, CitizenData)"/>
        protected override NotifyData GetEmailNotifyData(Case @case, CitizenData citizen)
        {
            return NotImplemented<NotifyData>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
        {
            return NotImplemented<string>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(Case, CitizenData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(Case @case, CitizenData citizen)
        {
            return NotImplemented<Dictionary<string, object>>();
        }
        #endregion

        /// <summary>
        /// This result is expected when using <see cref="NotImplementedScenario"/>.
        /// </summary>
        /// <returns>
        ///   The <see cref="NotImplementedException"/>.
        /// </returns>
        private static TResult NotImplemented<TResult>()
        {
            throw new NotImplementedException();
        }
    }
}