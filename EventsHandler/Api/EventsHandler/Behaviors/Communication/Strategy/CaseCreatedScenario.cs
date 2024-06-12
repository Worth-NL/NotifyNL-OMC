// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Case created" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class CaseCreatedScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseCreatedScenario"/> class.
        /// </summary>
        public CaseCreatedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(Case, CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(Case @case, CommonPartyData partyData)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", @case.Name },
                { "zaak.identificatie", @case.Identification },
                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname }
            };
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
          => this.Configuration.User.TemplateIds.Email.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(Case, CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(Case @case, CommonPartyData partyData)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", @case.Name },
                { "zaak.identificatie", @case.Identification },
                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname }
            };
        }
        #endregion
    }
}