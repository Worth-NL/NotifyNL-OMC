// © 2023, Worth Systems.

using Common.Versioning.Interfaces;
using Microsoft.VisualStudio.Threading;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Register.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.Register.v1
{
    /// <summary>
    /// <inheritdoc cref="ITelemetryService"/>
    /// </summary>
    /// <remarks>
    ///   Version: "Contactmomenten" Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class ContactRegistration : ITelemetryService
    {
        /// <inheritdoc cref="ITelemetryService.QueryContext"/>
        public IQueryContext QueryContext { get; }

        private readonly JoinableTaskFactory _taskFactory;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Contactmomenten";

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(IQueryContext queryContext)  // Dependency Injection (DI)
        {
            QueryContext = queryContext;
            _taskFactory = new JoinableTaskFactory(new JoinableTaskContext());
        }

        /// <inheritdoc cref="ITelemetryService.GetCreateContactMomentJsonBody(NotificationEvent, NotifyReference, NotifyMethods, IReadOnlyList{string})"/>
        string ITelemetryService.GetCreateContactMomentJsonBody(
            NotificationEvent notification, NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            #pragma warning disable VSTHRD1  // This method doesn't have to be marked as async (only v1 implementation is making HTTP calls, nothing else)
            CaseStatus caseStatus = _taskFactory
                .RunAsync(() => QueryContext.GetCaseStatusesAsync(reference.CaseId.RecreateCaseUri()))
                .Join()
                .LastStatus();
            #pragma warning restore VSTHRD1

            string logMessage = messages.Count > 0 ? messages[0] : string.Empty;

            return $"{{" +
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
        }

        /// <inheritdoc cref="ITelemetryService.GetLinkCaseJsonBody(ContactMoment, NotifyReference)"/>
        string ITelemetryService.GetLinkCaseJsonBody(ContactMoment contactMoment, NotifyReference reference)
        {
            return $"{{" +
                     $"\"contactmoment\":\"{contactMoment.ReferenceUri}\"," +  // URI
                     $"\"zaak\":\"{reference.CaseId}\"" +                     // URI
                   $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetLinkCustomerJsonBody(ContactMoment, NotifyReference)"/>
        string ITelemetryService.GetLinkCustomerJsonBody(ContactMoment contactMoment, NotifyReference reference)
        {
            return $"{{" +
                     $"\"contactmoment\":\"{contactMoment.ReferenceUri}\"," +    // URI
                     $"\"klant\":\"{reference.PartyId.RecreatePartyUri()}\"," +  // URI
                     $"\"rol\":\"belanghebbende\"," +
                     $"\"gelezen\":false" +
                   $"}}";
        }
    }
}