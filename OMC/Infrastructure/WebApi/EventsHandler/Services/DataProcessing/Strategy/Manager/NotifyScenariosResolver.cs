// © 2023, Worth Systems.

using Common.Settings.Configuration;
using Common.Settings.Extensions;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using PingenApiNet.Abstractions.Models.Letters.Embedded;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.PingenPost;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Services.DataProcessing.Strategy.Manager
{
    /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}"/>
    internal sealed class NotifyScenariosResolver : IScenariosResolver<INotifyScenario, NotificationEvent>
    {
        private readonly OmcConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataQueryService<NotificationEvent> _dataQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScenariosResolver"/> nested class.
        /// </summary>
        public NotifyScenariosResolver(
            OmcConfiguration configuration,
            IServiceProvider serviceProvider,
            IDataQueryService<NotificationEvent> dataQuery)  // Dependency Injection (DI)
        {
            this._configuration = configuration;
            this._serviceProvider = serviceProvider;
            this._dataQuery = dataQuery;
        }

        /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}.DetermineScenarioAsync(NotificationEvent)"/>
        async Task<INotifyScenario> IScenariosResolver<INotifyScenario, NotificationEvent>.DetermineScenarioAsync(NotificationEvent model)
        {
            // Case scenarios
            if (IsCaseScenario(model))
            {

                var httpClient = new HttpClient();
                var pingenService = new PingenService(httpClient, "A3SLYSW9RRUK7J69UIUA", "f3/mYhIG1D71NiZMG14dbHtqMstkHi+WCifzPFooZIoSq2vhzqjSVwfXZq5Vlh2dK/V7M0mMA477blOQ");

                try
                {
                    // Step 1: Authenticate
                    await pingenService.AuthenticateAsync();

                    // Step 2: Request file upload URL
                    var fileUploadResponse = await pingenService.RequestFileUploadUrlAsync();

                    // Step 3: Upload file
                    string filePath = "C:\\Users\\ThomasEelvelt\\Desktop\\desk\\Repos\\NotifyNL-OMC\\OMC\\Infrastructure\\Persistence\\WebQueries\\PingenPost\\prime-letter-worth.pdf";
                    var responseupload = await PingenService.UploadFileAsync(fileUploadResponse.Data.Attributes.Url, filePath);

                    // Step 4: Submit letter
                    string organisationId = "17466aee-d419-4ebe-ba60-a32e3abb4965";

                    var letterMetaData = new LetterMetaData
                    {
                        Recipient = new()
                        {
                            Name = "Ernout van der Waard",
                            Street = "Laan van Vredenoord",
                            Number = "11",
                            Zip = "2289 DA",
                            City = "Rijswijk",
                            Country = "NL"
                        },
                        Sender = new()
                        {
                            Name = "Dev-Team",
                            Street = "Laan van Vredenoord ",
                            Number = "11",
                            Zip = "2289 DA",
                            City = "Rijswijk",
                            Country = "NL"
                        }
                    };

                    await pingenService.SubmitLetterAsync(
                        organisationId,
                        letterMetaData,
                        fileUploadResponse.Data.Attributes.Url,
                        fileUploadResponse.Data.Attributes.UrlSignature,
                        "test-address.pdf"
                    );

                    Console.WriteLine("Letter submitted successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                IQueryContext queryContext = this._dataQuery.From(model);
                CaseStatuses caseStatuses = await queryContext.GetCaseStatusesAsync();

                // Scenario #1: "Case created"
                if (caseStatuses.WereNeverUpdated())
                {
                    return this._serviceProvider.GetRequiredService<CaseCreatedScenario>();
                }

                return !(await queryContext.GetLastCaseTypeAsync(caseStatuses)).IsFinalStatus
                    // Scenario #2: "Case status updated"
                    ? this._serviceProvider.GetRequiredService<CaseStatusUpdatedScenario>()
                    // Scenario #3: "Case finished"
                    : this._serviceProvider.GetRequiredService<CaseClosedScenario>();
            }

            // Object scenarios
            if (IsObjectScenario(model))
            {
                Guid objectTypeId = model.Attributes.ObjectTypeUri.GetGuid();

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.TaskObjectType_Uuid()))
                {
                    // Scenario #4: "Task assigned"
                    return this._serviceProvider.GetRequiredService<TaskAssignedScenario>();
                }

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.MessageObjectType_Uuid()))
                {
                    // Scenario #6: "Message received"
                    return this._serviceProvider.GetRequiredService<MessageReceivedScenario>();
                }

                throw new AbortedNotifyingException(
                    string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_GenObjectTypeGuid,
                        /* {0} */ $"{objectTypeId}",
                        /* {1} */ ConfigExtensions.GetGenericVariableObjectTypeEnvVarName()));
            }

            // Scenario #5: "Decision made"
            if (IsDecisionScenario(model))
            {
                return this._serviceProvider.GetRequiredService<DecisionMadeScenario>();
            }

            // No matching scenario. There is no clear instruction what to do with the received Notification
            return this._serviceProvider.GetRequiredService<NotImplementedScenario>();
        }

        #region Filters
        /// <summary>
        /// OMC is meant to process <see cref="NotificationEvent"/>s with certain characteristics (determining the workflow).
        /// </summary>
        /// <remarks>
        ///   This check is verifying whether case scenarios would be processed.
        /// </remarks>
        private static bool IsCaseScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action:   Actions.Create,
                Channel:  Channels.Cases,
                Resource: Resources.Status
            };
        }

        /// <summary>
        ///   <inheritdoc cref="IsCaseScenario(NotificationEvent)"/>
        /// </summary>
        /// <remarks>
        ///   This check is verifying whether task scenarios would be processed.
        /// </remarks>
        private static bool IsObjectScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action:   Actions.Create,
                Channel:  Channels.Objects,
                Resource: Resources.Object
            };
        }

        /// <summary>
        ///   <inheritdoc cref="IsCaseScenario(NotificationEvent)"/>
        /// </summary>
        /// <remarks>
        ///   This check is verifying whether decision scenarios would be processed.
        /// </remarks>
        private static bool IsDecisionScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action:   Actions.Create,
                Channel:  Channels.Decisions,
                Resource: Resources.Decision
            };
        }
        #endregion
    }
}