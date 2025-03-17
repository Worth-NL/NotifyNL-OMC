// © 2023, Worth Systems.

using System.Globalization;
using System.Text.Json;
using Common.Settings.Configuration;
using JetBrains.Annotations;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.KTO.Models;
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
        public async Task<HttpRequestResponse> ReportCompletionAsync(NotifyReference reference,
            NotifyMethods notificationMethod, string referenceAddress, params string[] messages)
        {
            HttpRequestResponse requestResponse = default;
            CaseStatuses caseStatuses = default;

            try
            {
                this.QueryContext.SetNotification(reference.Notification);

                caseStatuses =
                    await this.QueryContext.GetCaseStatusesAsync(reference.CaseId.RecreateCaseUri());

                // Register processed notification
                ContactMoment contactMoment = await this.QueryContext.CreateContactMomentAsync(
                    GetCreateContactMomentJsonBody(reference, notificationMethod, messages, caseStatuses.LastStatus()));

                // Linking to the case and the customer
                if ((requestResponse = await this.QueryContext.LinkCaseToContactMomentAsync(GetLinkCaseJsonBody(contactMoment, reference))).IsFailure ||
                    (requestResponse = await this.QueryContext.LinkPartyToContactMomentAsync(GetLinkCustomerJsonBody(contactMoment, reference))).IsFailure)
                {
                    return HttpRequestResponse.Failure(requestResponse.JsonResponse);
                }

                return HttpRequestResponse.Success(QueryResources.Registering_SUCCESS_NotificationSentToNotifyNL);
            }
            catch (Exception exception)
            {
                return HttpRequestResponse.Failure(exception.Message);
            }
            finally
            {
                //if (requestResponse.IsSuccess)
                {
                    await SendKtoAsync(notificationMethod, referenceAddress,
                        await this.QueryContext.GetLastCaseTypeAsync(caseStatuses));
                }
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

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="referenceAddress">The phone number or email address.</param>
        /// <param name="lastCaseType">The last case status and type.</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        private async Task SendKtoAsync(NotifyMethods notificationMethod, string referenceAddress, CaseType lastCaseType)
        {
            // TODO: Handle if no KTO Settings Provided. This is a valid situation.
            CaseTypeSettingsObject caseTypeSettingsObject = JsonSerializer.Deserialize<CaseTypeSettingsObject>(this.Omc.KTO.CaseTypeSettings());

            CaseTypeSetting? caseTypeSetting = caseTypeSettingsObject.CaseTypeSettings.FirstOrDefault(x => x.CaseTypeId == lastCaseType.Identification);

            if (caseTypeSetting != null)
            {
                if (notificationMethod == NotifyMethods.Email && referenceAddress != string.Empty)
                {
                    await this.QueryContext.SendKtoAsync(JsonSerializer.Serialize(new KtoCustomerObject
                    {
                        Emailadres = referenceAddress,
                        TransactionDate = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        SendTime = DateTime.Today.AddHours(9).ToString(CultureInfo.CurrentCulture),
                        Columns = new CustomerDataColumns
                        {
                            //SurveyName = caseTypeSetting.SurveyName
                        }
                    }));
                }
            }
        }

        #endregion
    }
}