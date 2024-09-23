// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Versioning.Interfaces;

namespace EventsHandler.Services.Register.v1
{
    /// <summary>
    /// <inheritdoc cref="ITelemetryService"/>
    /// </summary>
    /// <remarks>
    ///   Version: "Contactmomenten" Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class ContactRegistration : ITelemetryService
    {
        private readonly IQueryContext _queryContext;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Contactmomenten";

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(IQueryContext queryContext)
        {
            this._queryContext = queryContext;
        }

        /// <inheritdoc cref="ITelemetryService.CreateContactMomentAsync(NotificationEvent, NotifyMethods, IReadOnlyList{string}"/>
        async Task<ContactMoment> ITelemetryService.CreateContactMomentAsync(
            NotificationEvent notification, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            // Prepare the body
            CaseStatus caseStatus = (await queryContext.GetCaseStatusesAsync()).LastStatus();
            string logMessage = messages.Count > 0 ? messages[0] : string.Empty;

            string jsonBody =
                $"{{" +
                  $"\"bronorganisatie\":{notification.GetOrganizationId()}," +             // ENG: Source organization
                  $"\"registratiedatum\":\"{caseStatus.Created:yyyy-MM-ddThh:mm:ss}\"," +  // ENG: Date of registration (of the case)
                  $"\"kanaal\":\"{notificationMethod}\"," +                                // ENG: Channel (of communication / notification)
                  $"\"tekst\":\"{logMessage}\"," +                                         // ENG: Text (to be logged)
                  $"\"initiatief\":\"gemeente\"," +                                        // ENG: Initiator (of the case)
                  $"\"medewerkerIdentificatie\":{{" +                                      // ENG: Worker / collaborator / contributor
                    $"\"identificatie\":\"omc\"," +
                    $"\"achternaam\":\"omc\"," +
                    $"\"voorletters\":\"omc\"," +
                    $"\"voorvoegselAchternaam\":\"omc\"" +
                  $"}}" +
                $"}}";

            return await queryContext.SendFeedbackToOpenKlantAsync(jsonBody);
        }

        private static async Task<string> SendFeedbackToOpenZaakAsync(  // TODO: This is not needed anymore
            IQueryContext queryContext, NotificationEvent notification, ContactMoment contactMoment)
        {
            // Prepare the body
            string jsonBody =
                $"{{" +
                  $"\"zaak\":\"{notification.MainObjectUri}\"," +         // ENG: Case
                  $"\"contactmoment\":\"{contactMoment.ReferenceUri}\"" +  // ENG: Moment of contact
                $"}}";

            return await queryContext.SendFeedbackToOpenZaakAsync(jsonBody);
        }

        // TODO: Link to the case

        // POST method

        // URI: {OpenKlant}/objectcontactmomenten

        // Body:
        // string caseId = (await queryContext.GetMainObjectAsync()).Id

        // {
        // "contactmoment": "{contactMoment.ReferenceUri}",
        // "object": "{notification.MainObjectUri}",
        // "objectType": "zaak"
        // }


        // TODO: Link to the customer
        
        // POST method

        // URI: {OpenKlant}/klantcontactmomenten

        // Body:
        // {
        // "contactmoment": "{contactMoment.ReferenceUri}",
        // "klant": "CitizenResult.Id",  => Obtain it somehow
        // "rol": "belanghebbende",
        // "gelezen": false
        // }
        #endregion
    }
}