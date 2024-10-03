// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;

namespace EventsHandler.Services.Register.v2
{
    /// <inheritdoc cref="ITelemetryService"/>
    /// <remarks>
    ///   Version: "Klantcontacten" Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class ContactRegistration : ITelemetryService
    {
        /// <inheritdoc cref="ITelemetryService.QueryContext"/>
        public IQueryContext QueryContext { get; }

        private readonly WebApiConfiguration _configuration;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Klantcontacten";

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(WebApiConfiguration configuration, IQueryContext queryContext)
        {
            this._configuration = configuration;
            this.QueryContext = queryContext;
        }
        
        /// <inheritdoc cref="ITelemetryService.GetCreateContactMomentJsonBody(NotificationEvent, NotifyReference, NotifyMethods, IReadOnlyList{string})"/>
        string ITelemetryService.GetCreateContactMomentJsonBody(
            NotificationEvent notification, NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : string.Empty;
            string userMessageBody    = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : string.Empty;

            return $"{{" +
                     $"\"kanaal\":\"{notificationMethod}\"," +              // ENG: Channel of communication (notification)
                     $"\"onderwerp\":\"{userMessageSubject}\"," +           // ENG: Subject (of the message to be sent to the user)
                     $"\"inhoud\":\"{userMessageBody}\"," +                 // ENG: Content (of the message to be sent to the user)
                     $"\"indicatieContactGelukt\":{isSuccessfullySent}," +  // ENG: Indication of successful contact
                     $"\"taal\":\"nl\"," +                                  // ENG: Language (of the notification)
                     $"\"vertrouwelijk\":false" +                           // ENG: Confidentiality (of the notification)
                   $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetLinkCaseJsonBody(ContactMoment, NotifyReference)"/>
        string ITelemetryService.GetLinkCaseJsonBody(ContactMoment contactMoment, NotifyReference reference)
        {
            return $"{{" +
                     $"\"klantcontact\":{{" +  // ENG: Customer contact
                       $"\"uuid\":\"{contactMoment.ReferenceUri.GetGuid()}\"" +  // GUID
                     $"}}," +
                     $"\"wasKlantcontact\":{{" +
                       $"\"uuid\":\"{contactMoment.ReferenceUri.GetGuid()}\"" +  // GUID
                     $"}}," +
                     $"\"onderwerpobjectidentificator\":{{" +  // ENG: Subject Object Identifier
                       $"\"objectId\":\"{reference.CaseId}\"," +
                       $"\"codeObjecttype\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType()}\"," +
                       $"\"codeRegister\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeRegister()}\"," +
                       $"\"codeSoortObjectId\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()}\"" +
                     $"}}" +
                   $"}}";
        }
        
        /// <inheritdoc cref="ITelemetryService.GetLinkCustomerJsonBody(ContactMoment, NotifyReference)"/>
        string ITelemetryService.GetLinkCustomerJsonBody(ContactMoment contactMoment, NotifyReference reference)
        {
            return $"{{" +
                     $"\"wasPartij\":{{" +
                       $"\"uuid\":\"{reference.PartyId}\"" +  // GUID
                     $"}}," +
                     $"\"hadKlantcontact\":{{" +
                       $"\"uuid\":\"{contactMoment.ReferenceUri.GetGuid()}\"" +  // GUID
                     $"}}," +
                     $"\"rol\":\"klant\"," +
                     $"\"initiator\":true" +
                   $"}}";
        }
    }
}