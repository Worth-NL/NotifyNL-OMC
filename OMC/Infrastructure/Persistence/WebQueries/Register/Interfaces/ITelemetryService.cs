// © 2023, Worth Systems.

using Common.Settings.Configuration;
using JetBrains.Annotations;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.MOBB.Models;
using WebQueries.Print.Models;
using WebQueries.Properties;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Enums;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.Register.Interfaces
{
    /// <summary>
    /// The service to collect and send feedback about the current business activities to the dedicated external API endpoint.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    public interface ITelemetryService : IVersionDetails
    {
        /// <inheritdoc cref="IQueryContext"/>
        internal IQueryContext QueryContext { get; }

        //internal INotifyService<NotifyData> NotifyService { get; }

        internal OmcConfiguration Omc { get; }

        /// <summary>
        /// Reports to external API service that notification of type <see cref="NotifyMethods"/> was sent to "Notify NL" service.
        /// </summary>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="referenceAddress">Address like email or telephone number.</param>
        /// <param name="messages">The messages to be used during registration of this event.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        public async Task<HttpRequestResponse> ReportCompletionAsync(
            NotifyReference reference,
            NotifyMethods notificationMethod,
            string referenceAddress,
            params string[] messages)
        {
            try
            {
                this.QueryContext.SetNotification(reference.Notification);


                string json = GetNewCreateContactMomentJsonBody(reference, notificationMethod, messages);

                MaakKlantContact contactMoment = await this.QueryContext.CreateNewContactMomentAsync(json);

                HttpRequestResponse linkResponse = await this.QueryContext.LinkActorToContactMomentAsync(
                    GetActorCustomerContactMomentJsonBody(
                        this.Omc.OMC.Actor.Id(), contactMoment.ContactMoment.ReferenceUri.GetGuid()));

                return linkResponse.IsFailure
                    ? HttpRequestResponse.Failure(linkResponse.JsonResponse) // Throw soft error to prevent retries
                    : HttpRequestResponse.Success(QueryResources.Registering_SUCCESS_NotificationSentToNotifyNL);
            }
            catch (Exception ex)
            {
                return ex.Message.Contains("duplicate key value")
                    ? HttpRequestResponse.Failure("Duplicate key conflict in OpenKlant API")
                    : // For all other exceptions, just return failure
                    HttpRequestResponse.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Reports to external API service that a MOBB / Berichtenbox notification of type <see cref="NotifyMethods"/>
        /// was sent to "Notify NL" service, creating a contactmoment linked to the Bericht (VTB message) itself
        /// rather than a Case.
        /// </summary>
        /// <param name="reference"><inheritdoc cref="MessageBoxNotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="referenceAddress">Address like email or telephone number.</param>
        /// <param name="messages">The messages to be used during registration of this event.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        /// <remarks>
        ///   First-version draft: unlike <see cref="ReportCompletionAsync"/>, this does not call
        ///   <see cref="IQueryContext.SetNotification"/> - there is no real <c>NotificationEvent</c> for a
        ///   MOBB/CloudEvent-driven send. If something downstream turns out to depend on
        ///   <c>QueryContext</c> having a notification set, this will need revisiting.
        /// </remarks>
        public async Task<HttpRequestResponse> ReportMessageBoxCompletionAsync(
            MessageBoxNotifyReference reference,
            NotifyMethods notificationMethod,
            [UsedImplicitly] string referenceAddress,
            params string[] messages)
        {
            try
            {
                string json = GetMessageBoxContactMomentJsonBody(reference, notificationMethod, messages);

                MaakKlantContact contactMoment = await this.QueryContext.CreateNewContactMomentAsync(json);

                HttpRequestResponse linkResponse = await this.QueryContext.LinkActorToContactMomentAsync(
                    GetActorCustomerContactMomentJsonBody(
                        this.Omc.OMC.Actor.Id(), contactMoment.ContactMoment.ReferenceUri.GetGuid()));

                return linkResponse.IsFailure
                    ? HttpRequestResponse.Failure(linkResponse.JsonResponse) // Throw soft error to prevent retries
                    : HttpRequestResponse.Success(QueryResources.Registering_SUCCESS_NotificationSentToNotifyNL);
            }
            catch (Exception ex)
            {
                return ex.Message.Contains("duplicate key value")
                    ? HttpRequestResponse.Failure("Duplicate key conflict in OpenKlant API")
                    : // For all other exceptions, just return failure
                    HttpRequestResponse.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Reports to external API service that a printed letter was handed to "Notify NL" service,
        /// creating a contactmoment for the print job.
        /// </summary>
        /// <param name="reference"><inheritdoc cref="PrintNotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages to be used during registration of this event.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        /// <remarks>
        ///   Driven by the delivery receipt rather than by the send, exactly as the e-mail and SMS paths are:
        ///   a 201 from "Notify NL" only means the request was accepted, and a precompiled letter can still
        ///   fail PDF validation afterwards.
        ///   <para>
        ///     The "onderwerpobject" is built from the triggering object's payload rather than from a
        ///     <c>NotificationEvent</c>'s case, so there is no real notification to hand
        ///     <see cref="IQueryContext.SetNotification"/> - but one still has to be set, because the query
        ///     context's <c>IQueryBase</c> carries it and the registration calls dereference it.
        ///   </para>
        /// </remarks>
        public async Task<HttpRequestResponse> ReportPrintCompletionAsync(
            PrintNotifyReference reference,
            NotifyMethods notificationMethod,
            params string[] messages)
        {
            try
            {
                // Same reason ReportCompletionAsync calls SetNotification: the query context's IQueryBase
                // carries a notification that the registration calls dereference. A delivery receipt is not
                // a NotificationEvent, so the print flow supplies the object-scenario shape it actually is.
                this.QueryContext.SetNotification(new NotificationEvent
                {
                    Action = Actions.Create,
                    Channel = Channels.Objects,
                    Resource = Resources.Object,
                });

                string json = GetPrintContactMomentJsonBody(reference, notificationMethod, messages);

                MaakKlantContact contactMoment = await this.QueryContext.CreateNewContactMomentAsync(json);

                Guid klantcontactUuid = contactMoment.ContactMoment.ReferenceUri.GetGuid();

                HttpRequestResponse linkResponse = await this.QueryContext.LinkActorToContactMomentAsync(
                    GetActorCustomerContactMomentJsonBody(this.Omc.OMC.Actor.Id(), klantcontactUuid));

                if (linkResponse.IsFailure)
                {
                    return HttpRequestResponse.Failure(linkResponse.JsonResponse); // Throw soft error to prevent retries
                }

                // The printed PDF is attached in a second call: "maak-klantcontact" has no field for
                // attachments (MBO-1025 - requested by Den Haag, deferred), so the klantcontact has to
                // exist before its bijlage can point at it.
                if (reference.AttachmentId != Guid.Empty)
                {
                    HttpRequestResponse bijlageResponse = await this.QueryContext.CreateBijlageAsync(
                        GetBijlageJsonBody(klantcontactUuid, reference.AttachmentId));

                    if (bijlageResponse.IsFailure)
                    {
                        return HttpRequestResponse.Failure(bijlageResponse.JsonResponse);
                    }
                }

                return HttpRequestResponse.Success(QueryResources.Registering_SUCCESS_NotificationSentToNotifyNL);
            }
            catch (Exception ex)
            {
                return ex.Message.Contains("duplicate key value")
                    ? HttpRequestResponse.Failure("Duplicate key conflict in OpenKlant API")
                    : // For all other exceptions, just return failure
                    HttpRequestResponse.Failure(ex.Message);
            }
        }

        #region Abstract

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="caseStatus">The last case status. Is only used for v1 implementation.</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetCreateContactMomentJsonBody(
            [UsedImplicitly] NotifyReference reference,
            NotifyMethods notificationMethod,
            IReadOnlyList<string> messages,
            CaseStatus? caseStatus = null);

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="actor"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <param name="customerContactMoment"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        string GetActorCustomerContactMomentJsonBody(Guid actor, Guid customerContactMoment);

        /// <inheritdoc cref="ITelemetryService.GetNewCreateContactMomentJsonBody(NotifyReference, NotifyMethods, IReadOnlyList{string})"/>
        string GetNewCreateContactMomentJsonBody(
                NotifyReference reference, NotifyMethods notificationMethod,
                IReadOnlyList<string> messages) // CaseStatus is only used for v1 implementation
            ;

        /// <summary>
        /// Prepares a dedicated JSON body for a MOBB / Berichtenbox contactmoment, linked to the
        /// Bericht (VTB message) itself instead of a Case.
        /// </summary>
        /// <param name="reference"><inheritdoc cref="MessageBoxNotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages.</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        string GetMessageBoxContactMomentJsonBody(
            MessageBoxNotifyReference reference, NotifyMethods notificationMethod,
            IReadOnlyList<string> messages);

        /// <summary>
        /// Prepares a dedicated JSON body for a print ("printstraat") contactmoment.
        /// </summary>
        /// <param name="reference"><inheritdoc cref="PrintNotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages.</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        string GetPrintContactMomentJsonBody(
            PrintNotifyReference reference, NotifyMethods notificationMethod,
            IReadOnlyList<string> messages);

        /// <summary>
        /// Prepares a dedicated JSON body linking a "bijlage" to an existing klantcontact.
        /// </summary>
        /// <param name="klantcontactUuid">The klantcontact the attachment belongs to.</param>
        /// <param name="documentUuid">The "enkelvoudiginformatieobject" holding the file.</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        string GetBijlageJsonBody(Guid klantcontactUuid, Guid documentUuid);

        #endregion
    }
}