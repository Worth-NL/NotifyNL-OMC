// © 2023, Worth Systems.

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public async Task<HttpRequestResponse> ReportCompletionAsync(NotifyReference reference,
            NotifyMethods notificationMethod, string referenceAddress, params string[] messages)
        {
            HttpRequestResponse requestResponse = default;
            CaseStatuses caseStatuses = default;

            try
            {
                //NotificationData notificationData =  await this.NotifyService.GetNotificationDataAsync(
                //    new NotifyData(notificationMethod, string.Empty, Guid.Empty, [], reference), "");
                this.QueryContext.SetNotification(reference.Notification);

                caseStatuses = await this.QueryContext.GetCaseStatusesAsync(reference.CaseId.RecreateCaseUri());

                string json = GetNewCreateContactMomentJsonBody(reference, notificationMethod, messages,
                    caseStatuses.LastStatus());
                // Register processed notification
                MaakKlantContact contactMoment = await this.QueryContext.CreateNewContactMomentAsync(
                    json);

                // Linking to the case and the customer
                return (requestResponse = await this.QueryContext.LinkActorToContactMomentAsync(
                    GetActorCustomerContactMomentJsonBody(
                        this.Omc.OMC.Actor.Id(), contactMoment.ContactMoment.ReferenceUri.GetGuid()))
                    ).IsFailure 
                    ? HttpRequestResponse.Failure(requestResponse.JsonResponse) 
                    : HttpRequestResponse.Success(QueryResources.Registering_SUCCESS_NotificationSentToNotifyNL);
            }
            catch (Exception exception)
            {
                return HttpRequestResponse.Failure(exception.Message);
            }
            finally
            {
                // Only if ContactMoment is successfully created send KTO 
                if (requestResponse.IsSuccess)
                {
                    // Start Customer Satisfaction Service
                    await SendKtoAsync(notificationMethod, referenceAddress,
                        await this.QueryContext.GetLastCaseTypeAsync(caseStatuses), reference.CaseId);
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
        /// <param name="actor"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <param name="customerContactMoment"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        string GetActorCustomerContactMomentJsonBody(Guid actor, Guid customerContactMoment);

        /// <summary>
        /// Checks if there are KTO settings, if so tries to send a request to KTO Service.
        /// </summary>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="referenceAddress">The phone number or email address.</param>
        /// <param name="lastCaseType">The last case status and type.</param>
        /// <param name="caseId">CaseId used to fetch party data</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        private async Task SendKtoAsync(NotifyMethods notificationMethod, string referenceAddress, CaseType lastCaseType, Guid? caseId)
        {
            if (!ShouldSendKto(notificationMethod, referenceAddress, lastCaseType))
                return;

            KtoCustomerObject ktoCustomer = await CreateKtoCustomerAsync(referenceAddress, lastCaseType, caseId);
            HttpRequestResponse result = await SendKtoRequestAsync(ktoCustomer);

            if (result.IsFailure)
                throw new HttpRequestException("Failed to send KTO request.");
        }

        private bool ShouldSendKto(NotifyMethods notificationMethod, string referenceAddress, CaseType lastCaseType)
        {
            if (this.Omc.KTO.CaseTypeSettings() == "-")
                return false;

            string caseTypeSettingsJson = this.Omc.KTO.CaseTypeSettings();

            CaseTypeSettingsObject caseTypeSettingsObject =
                JsonSerializer.Deserialize<CaseTypeSettingsObject>(caseTypeSettingsJson);

            CaseTypeSetting? caseTypeSetting = caseTypeSettingsObject.CaseTypeSettings
                .FirstOrDefault(x => x.CaseTypeId == lastCaseType.Identification);

            return caseTypeSetting != null
                   && notificationMethod == NotifyMethods.Email
                   && !string.IsNullOrWhiteSpace(referenceAddress);
        }

        private async Task<KtoCustomerObject> CreateKtoCustomerAsync(string referenceAddress, CaseType lastCaseType, Guid? caseId)
        {
            string caseTypeSettingsJson = Omc.KTO.CaseTypeSettings();
            CaseTypeSettingsObject caseTypeSettingsObject = JsonSerializer.Deserialize<CaseTypeSettingsObject>(caseTypeSettingsJson);
            CaseTypeSetting? caseTypeSetting = caseTypeSettingsObject.CaseTypeSettings.FirstOrDefault(x => x.CaseTypeId == lastCaseType.Identification);

            CommonPartyData? partyData = await QueryContext.GetPartyDataAsync(caseId?.RecreateCaseUri());
            if (partyData == null)
            {
                throw new InvalidOperationException("Failed to retrieve party data.");
            }

            return new KtoCustomerObject
            {
                ApproveAutomatically = caseTypeSettingsObject.ApproveAutomatically,
                IsTest = caseTypeSettingsObject.IsTest,
                Customers =
                [
                    new Customer
                    {
                        Email = referenceAddress,
                        TransactionDate = DateOnly.FromDateTime(DateTime.Now),
                        SendDate = DateOnly.FromDateTime(DateTime.Now),
                        Data =
                        [
                            new CustomerData
                            {
                                CustomerDataColumnId = 8,
                                Name = "Voornaam",
                                Value = partyData.Value.Name
                            },
                            new CustomerData
                            {
                                CustomerDataColumnId = 11,
                                Name = "Achternaam",
                                Value = partyData.Value.Surname
                            },
                            new CustomerData
                            {
                                CustomerDataColumnId = 10,
                                Name = "Tussenvoegsel",
                                Value = partyData.Value.SurnamePrefix
                            },
                            new CustomerData
                            {
                                CustomerDataColumnId = 9,
                                Name = "Geslacht",
                                Value = partyData.Value.Gender
                            },
                            new CustomerData
                            {
                                CustomerDataColumnId = 2,
                                Name = typeof(CaseTypeSetting)
                                    .GetProperty(nameof(CaseTypeSetting.SurveyName))
                                    ?.GetCustomAttribute<JsonPropertyNameAttribute>()
                                    ?.Name ?? string.Empty,
                                Value = caseTypeSetting.Value.SurveyName
                            },
                            new CustomerData
                            {
                                CustomerDataColumnId = 7,
                                Name = typeof(CaseTypeSetting)
                                    .GetProperty(nameof(CaseTypeSetting.ServiceName))
                                    ?.GetCustomAttribute<JsonPropertyNameAttribute>()
                                    ?.Name ?? string.Empty,
                                Value = caseTypeSetting.Value.ServiceName
                            },
                            new CustomerData
                            {
                                CustomerDataColumnId = 6,
                                Name = typeof(CaseTypeSetting)
                                    .GetProperty(nameof(CaseTypeSetting.SurveyType))
                                    ?.GetCustomAttribute<JsonPropertyNameAttribute>()
                                    ?.Name ?? string.Empty,
                                Value = caseTypeSetting.Value.SurveyType
                            }
                        ]
                    }
                ]
            };
        }

        private async Task<HttpRequestResponse> SendKtoRequestAsync(KtoCustomerObject ktoCustomer)
        {
            string serializedKtoCustomer = JsonSerializer.Serialize(ktoCustomer);
            return await this.QueryContext.SendKtoAsync(serializedKtoCustomer);
        }

        #endregion

        /// <inheritdoc cref="ITelemetryService.GetNewCreateContactMomentJsonBody(NotifyReference, NotifyMethods, IReadOnlyList{string}, CaseStatus?)"/>
        string GetNewCreateContactMomentJsonBody(
                NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages,
                CaseStatus? caseStatus) // CaseStatus is only used for v1 implementation
            ;
    }
}