using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Kto
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class KtoScenario : BaseScenario
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="KtoScenario"/> class.
        /// </summary>
        public KtoScenario(OmcConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)  // Dependency Injection (DI)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.Notify.TemplateId.Email.MessageReceived();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.Notify.TemplateId.Sms.MessageReceived();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            throw new NotImplementedException();
        }

        protected override Guid GetLetterTemplateId()
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<string, object> GetLetterPersonalization(CommonPartyData partyData)
        {
            throw new NotImplementedException();
        }

        protected override string GetWhitelistEnvVarName()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}