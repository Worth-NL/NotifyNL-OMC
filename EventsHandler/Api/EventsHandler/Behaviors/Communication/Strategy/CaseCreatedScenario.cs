// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;
using CitizenData = EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.CitizenData;

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

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(Case, CitizenData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(Case @case, CitizenData citizen)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", @case.Name },
                { "zaak.identificatie", @case.Identification },
                { "klant.voornaam", citizen.Name },
                { "klant.voorvoegselAchternaam", citizen.SurnamePrefix },
                { "klant.achternaam", citizen.Surname }
            };
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
          => this.Configuration.User.TemplateIds.Email.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(Case, CitizenData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(Case @case, CitizenData citizen)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", @case.Name },
                { "zaak.identificatie", @case.Identification },
                { "klant.voornaam", citizen.Name },
                { "klant.voorvoegselAchternaam", citizen.SurnamePrefix },
                { "klant.achternaam", citizen.Surname }
            };
        }
        #endregion
    }
}