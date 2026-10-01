// © 2026, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Products
{
    /// <summary>
    /// <inheritdoc cref="Base.Interfaces.INotifyScenario"/>
    /// The marker for the "Product created" scenario.
    /// </summary>
    /// <remarks>
    ///   Like <see cref="Print.PrintScenario"/>, <see cref="Kto.KtoScenario"/> and
    ///   <see cref="MessageBox.MessageBoxScenario"/>, this only exists so the resolver has something to
    ///   return: the work itself happens in
    ///   <see cref="WebQueries.Producten.Interfaces.IProductScenario"/>, reached from
    ///   <c>NotifyProcessor</c>.
    ///   <para>
    ///     <see cref="BaseScenario"/>'s members all throw here because the shape does not fit. Its
    ///     pipeline resolves exactly one party and then picks e-mail, SMS or post from that party's
    ///     preference in "OpenKlant". A created product notifies every "eigenaar" it has, and always by
    ///     e-mail, so neither the single recipient nor the channel choice applies.
    ///   </para>
    /// </remarks>
    /// <seealso cref="BaseScenario"/>
    internal sealed class ProductCreatedScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductCreatedScenario"/> class.
        /// </summary>
        public ProductCreatedScenario(
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
            => this.Configuration.ZGW.Whitelist.ProductCreate_IDs().ToString();
        #endregion
    }
}
