// © 2024, Worth Systems.

using Common.Settings.Configuration;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Register.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.Register.v2
{
    /// <inheritdoc cref="ITelemetryService"/>
    /// <remarks>
    ///   Version: "Klantcontacten" Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class ContactRegistration : ITelemetryService
    {
        /// <inheritdoc cref="ITelemetryService.QueryContext"/>
        public IQueryContext QueryContext { get; }

        /// <summary>
        /// 
        /// </summary>
        public OmcConfiguration Omc { get; }

        private readonly OmcConfiguration _configuration;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Klantcontacten";

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(OmcConfiguration configuration, IQueryContext queryContext, OmcConfiguration omc)  // Dependency Injection (DI)
        {
            this._configuration = configuration;
            this.QueryContext = queryContext;
            Omc = omc;
        }

        #region Polymorphic
        /// <inheritdoc cref="ITelemetryService.GetNewCreateContactMomentJsonBody(NotifyReference, NotifyMethods, IReadOnlyList{string}, CaseStatus?)"/>
        string ITelemetryService.GetNewCreateContactMomentJsonBody(
            NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages, CaseStatus? caseStatus) // CaseStatus is only used for v1 implementation
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : string.Empty;
            string userMessageBody = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : string.Empty;
            DateTime sentAt = messages.Count > 3 && DateTime.TryParse(messages[3], out DateTime parsedDate) ? parsedDate : DateTime.Now;

            return $"{{\"klantcontact\":{{" +
                       $"\"kanaal\":\"{notificationMethod}\"," +              // ENG: Channel of communication (notification)
                       $"\"onderwerp\":\"{userMessageSubject}\"," +           // ENG: Subject (of the message to be sent to the user)
                       $"\"inhoud\":\"{userMessageBody}\"," +                 // ENG: Content (of the message to be sent to the user) 
                       $"\"indicatieContactGelukt\":{isSuccessfullySent}," +  // ENG: Indication of successful contact
                       $"\"taal\":\"nl\"," +                                  // ENG: Language (of the notification)
                       $"\"vertrouwelijk\":true," +                           // Fixed: added comma
                       $"\"plaatsgevondenOp\":\"{sentAt:O}\"" +               // Fixed: interpolated variable with ISO 8601 format
                   $"}}," +
                   $"\"betrokkene\":{{" +
                       $"\"wasPartij\":{{\"uuid\":\"{reference.PartyId}\"}}," +
                       $"\"rol\":\"klant\"," +
                       $"\"initiator\":true" +
                       $"}}," +
                   $"\"onderwerpobject\":{{" +
                       $"\"onderwerpobjectidentificator\":{{" +
                       $"\"objectId\":\"{reference.CaseId}\"," +
                       $"\"codeObjecttype\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType()}\"," +
                       $"\"codeRegister\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeRegister()}\"," +
                       $"\"codeSoortObjectId\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()}\"" +
                       $"}}" +
                   $"}}" +
               $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetCreateContactMomentJsonBody(NotifyReference, NotifyMethods, IReadOnlyList{string}, CaseStatus?)"/>
        string ITelemetryService.GetCreateContactMomentJsonBody(
            NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages, CaseStatus? caseStatus) // CaseStatus is only used for v1 implementation
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
                       $"\"codeObjecttype\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType()}\"," + //Zaak
                       $"\"codeRegister\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeRegister()}\"," + //Open-Zaak
                       $"\"codeSoortObjectId\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()}\"" + //uuid
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
                     $"\"initiator\":true" + //false?
                   $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetActorCustomerContactMomentJsonBody(Guid, Guid)"/>
        string ITelemetryService.GetActorCustomerContactMomentJsonBody(Guid actor, Guid customerContactMoment)
        {
            return $"{{" +
                   $"\"actor\":{{" +
                   $"\"uuid\":\"{actor}\"" +
                   $"}}," +
                   $"\"klantcontact\":{{" +
                   $"\"uuid\":\"{customerContactMoment}\"" +
                   $"}}" +
                   $"}}";
        }
        #endregion
    }
}