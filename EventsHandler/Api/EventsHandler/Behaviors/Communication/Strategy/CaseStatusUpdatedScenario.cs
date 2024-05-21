// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;
using CitizenData = EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.CitizenData;

namespace EventsHandler.Behaviors.Communication.Strategy
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Case status updated" scenario.
    /// </summary>
    /// <seealso cref="BaseStatusScenario"/>
    internal class CaseStatusUpdatedScenario : BaseStatusScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatusUpdatedScenario"/> class.
        /// </summary>
        public CaseStatusUpdatedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic
        /// <inheritdoc cref="BaseScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal sealed override async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            this.LastCaseStatusType ??= await ReQueryCaseStatusTypeAsync(notification);

            return await base.GetAllNotifyDataAsync(notification);
        }

        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
          => this.Configuration.User().TemplateIds.Sms.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(Case, CitizenData)"/>
        protected sealed override Dictionary<string, object> GetSmsPersonalization(Case @case, CitizenData citizen)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", @case.Name },
                { "zaak.identificatie", @case.Identification },
                { "klant.voornaam", citizen.Name },
                { "klant.voorvoegselAchternaam", citizen.SurnamePrefix },
                { "klant.achternaam", citizen.Surname },
                { "status.omschrijving", base.LastCaseStatusType!.Value.Description }
            };
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
          => this.Configuration.User().TemplateIds.Email.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(Case, CitizenData)"/>
        protected sealed override Dictionary<string, object> GetEmailPersonalization(Case @case, CitizenData citizen)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", @case.Name },
                { "zaak.identificatie", @case.Identification },
                { "klant.voorvoegselAchternaam", citizen.SurnamePrefix },
                { "klant.voornaam", citizen.Name },
                { "klant.achternaam", citizen.Surname },
                { "status.omschrijving", this.LastCaseStatusType!.Value.Description }
            };
        }
        #endregion
    }
}