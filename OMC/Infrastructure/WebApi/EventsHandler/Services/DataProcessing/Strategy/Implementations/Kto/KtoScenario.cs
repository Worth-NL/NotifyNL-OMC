using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using WebQueries.DataQuerying.Adapter;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Kto
{
    internal sealed partial class KtoScenario : BaseScenario
    {
        private IQueryContext _queryContext = null!;
        private string ktoPayload = string.Empty;
        private Data _messageData;

        /// <summary>
        /// Initializes a new instance of the <see cref="KtoScenario"/> class.
        /// </summary>
        public KtoScenario(
            OmcConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {

        }

        #region Polymorphic (PrepareDataAsync)
        protected override async Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
        {
            this._queryContext = this.DataQuery.From(notification);
            this._messageData = (await _queryContext.GetMessageAsync()).Record.Data;
            throw new NotImplementedException();
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        protected override Guid GetEmailTemplateId()
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        protected override Guid GetSmsTemplateId()
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        protected override string GetWhitelistEnvVarName()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
