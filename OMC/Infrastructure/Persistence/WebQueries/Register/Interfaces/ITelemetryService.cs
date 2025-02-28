// © 2023, Worth Systems.

using JetBrains.Annotations;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Properties;
using WebQueries.Versioning.Interfaces;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.OpenKlant;
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

        /// <summary>
        /// Reports to external API service that notification of type <see cref="NotifyMethods"/> was sent to "Notify NL" service.
        /// </summary>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages to be used during registration of this event.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        public async Task<HttpRequestResponse> ReportCompletionAsync(NotifyReference reference,
            NotifyMethods notificationMethod, params string[] messages)
        {
            try
            {
                this.QueryContext.SetNotification(reference.Notification);

                CaseStatuses caseStatuses =
                    await this.QueryContext.GetCaseStatusesAsync(reference.CaseId.RecreateCaseUri());

                Case referenceCase = await this.QueryContext.GetCaseAsync(reference.CaseId.RecreateCaseUri());

                CommonPartyData partyData = await this.QueryContext.GetPartyDataAsync(referenceCase.Uri,
                    caseIdentifier: referenceCase.Identification);

                CaseType lastCaseType = await this.QueryContext.GetLastCaseTypeAsync(caseStatuses);

                if (lastCaseType.IsFinalStatus)
                {
                    if (partyData.DistributionChannel == DistributionChannels.Email)
                    {
                        // TODO: Check in LaunchSettings if CaseTypeIdentification matches and contains KTOSettings 
                        // if(!ktoSend)
                        // {
                        // TODO: Send call to KTO api initiating procedure
                        // }
                    }
                }

                // Register processed notification
                ContactMoment contactMoment = await this.QueryContext.CreateContactMomentAsync(
                    GetCreateContactMomentJsonBody(reference, notificationMethod, messages, caseStatuses.LastStatus()));

                HttpRequestResponse requestResponse;

                // Linking to the case and the customer
                if ((requestResponse =
                        await this.QueryContext.LinkCaseToContactMomentAsync(GetLinkCaseJsonBody(contactMoment,
                            reference))).IsFailure ||
                    (requestResponse =
                        await this.QueryContext.LinkPartyToContactMomentAsync(
                            GetLinkCustomerJsonBody(contactMoment, reference))).IsFailure)
                {
                    return HttpRequestResponse.Failure(requestResponse.JsonResponse);
                }

                return HttpRequestResponse.Success(QueryResources.Registering_SUCCESS_NotificationSentToNotifyNL);
            }
            catch (Exception exception)
            {
                return HttpRequestResponse.Failure(exception.Message);
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
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetLinkCaseJsonBody(ContactMoment contactMoment, NotifyReference reference);

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetLinkCustomerJsonBody(ContactMoment contactMoment, NotifyReference reference);

        #endregion
    }
}