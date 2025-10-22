// © 2023, Worth Systems.

using Common.Settings.Configuration;
using JetBrains.Annotations;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Properties;
using WebQueries.Versioning.Interfaces;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

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

        #endregion
    }
}