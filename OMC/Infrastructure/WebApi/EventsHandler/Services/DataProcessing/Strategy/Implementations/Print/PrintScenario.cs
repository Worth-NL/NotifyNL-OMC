// © 2026, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Print
{
    /// <summary>
    /// <inheritdoc cref="Base.Interfaces.INotifyScenario"/>
    /// The marker for the "Print" scenario.
    /// </summary>
    /// <remarks>
    ///   Like <see cref="Kto.KtoScenario"/> and <see cref="MessageBox.MessageBoxScenario"/>, this only
    ///   exists so the resolver has something to return: the work itself happens in
    ///   <see cref="WebQueries.Print.Interfaces.IPrintScenario"/>, reached from <c>NotifyProcessor</c>.
    ///   <see cref="BaseScenario"/>'s members all throw here because none of them apply - the print flow
    ///   picks no channel from the party's preferences and fills in no template, it always posts one
    ///   already-composed PDF.
    /// </remarks>
    /// <seealso cref="BaseScenario"/>
    internal sealed class PrintScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintScenario"/> class.
        /// </summary>
        public PrintScenario(
            OmcConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)  // Dependency Injection (DI)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
            => throw new NotImplementedException();
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => throw new NotImplementedException();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
            => throw new NotImplementedException();
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
            => throw new NotImplementedException();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
            => throw new NotImplementedException();
        #endregion

        #region Polymorphic (Letter logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetLetterTemplateId()"/>
        protected override Guid GetLetterTemplateId()
            => throw new NotImplementedException();

        /// <inheritdoc cref="BaseScenario.GetLetterPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetLetterPersonalization(CommonPartyData partyData)
            => throw new NotImplementedException();
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        protected override string GetWhitelistEnvVarName()
            => Common.Settings.Extensions.ConfigExtensions.GetWhitelistPrintAllowedEnvVarName();
        #endregion
    }
}
