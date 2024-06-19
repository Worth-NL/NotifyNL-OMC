// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;
using System.Diagnostics.CodeAnalysis;

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

        /// <inheritdoc cref="BaseScenario.GetSmsNotifyDataAsync(NotificationEvent, CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<NotifyData> GetSmsNotifyDataAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<NotifyData>());
        }

        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        [ExcludeFromCodeCoverage]
        protected override string GetSmsTemplateId()
        {
            return NotImplemented<string>();
        }

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<Dictionary<string, object>>());
        }

        /// <inheritdoc cref="BaseScenario.GetEmailNotifyDataAsync(NotificationEvent, CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<NotifyData> GetEmailNotifyDataAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<NotifyData>());
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        [ExcludeFromCodeCoverage]
        protected override string GetEmailTemplateId()
        {
            return NotImplemented<string>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<Dictionary<string, object>>());
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