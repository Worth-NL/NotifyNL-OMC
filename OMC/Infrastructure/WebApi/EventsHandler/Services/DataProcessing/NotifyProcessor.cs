// © 2023, Worth Systems.

using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using Common.Models.Responses;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Models.Responses;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.Validation.Interfaces;
using Notify.Exceptions;
using System.Text.Json;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Kto;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.KTO.Interfaces;
using WebQueries.MOBB.Interfaces;
using ZgwModels.Enums;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Properties;
using ZgwModels.Serialization.Interfaces;

namespace EventsHandler.Services.DataProcessing
{
    /// <inheritdoc cref="IProcessingService"/>
    internal sealed class NotifyProcessor : IProcessingService
    {
        private readonly ISerializationService _serializer;
        private readonly IValidationService<NotificationEvent> _validator;
        private readonly IScenariosResolver<INotifyScenario, NotificationEvent> _resolver;
        private readonly IKtoScenarioFactory _ktoScenarioFactory;
        private readonly IMessageBoxScenario _messageBoxScenario;

        public NotifyProcessor(
            ISerializationService serializer,
            IValidationService<NotificationEvent> validator,
            IScenariosResolver<INotifyScenario, NotificationEvent> resolver,
            IKtoScenarioFactory ktoScenarioFactory,
            IMessageBoxScenario messageBoxScenario)
        {
            this._serializer = serializer;
            this._validator = validator;
            this._resolver = resolver;
            this._ktoScenarioFactory = ktoScenarioFactory;
            this._messageBoxScenario = messageBoxScenario;
        }

        /// <inheritdoc cref="IProcessingService.ProcessAsync(object)"/>
        async Task<ProcessingResult> IProcessingService.ProcessAsync(object json)
        {
            BaseEnhancedDetails details = InfoDetails.Empty;

            try
            {
                // Step 1: Convert incoming object to JsonElement for inspection
                JsonElement jsonElement = json switch
                {
                    JsonElement je => je,
                    string jsonString => JsonDocument.Parse(jsonString).RootElement,  // Already raw JSON text – parse directly, don't re-encode it
                    _ => JsonDocument.Parse(JsonSerializer.Serialize(json)).RootElement
                };

                // Step 2: Check if this is a CloudEvent (has specversion, type, source, id)
                bool isCloudEvent = IsCloudEvent(jsonElement);

                if (isCloudEvent)
                {
                    // Step 2a: CloudEvent handling – bypass NotificationEvent deserialization
                    string cloudEventType = jsonElement.GetProperty("type").GetString() ?? string.Empty;

                    // Route based on CloudEvent type – look for ".berichten." (case-insensitive)
                    if (cloudEventType.Contains(".berichten.", StringComparison.OrdinalIgnoreCase))
                    {
                        // Pass the entire CloudEvent (jsonElement) to the scenario
                        HttpRequestResponse response = await _messageBoxScenario.ProcessCloudEventAsync(jsonElement);
                        return response.IsFailure
                            ? ProcessingResult.Failure(response.JsonResponse, json, details)
                            : ProcessingResult.Success("CloudEvent processed via MessageBoxScenario", json, details);
                    }
                    else
                    {
                        // Unsupported CloudEvent type – skip
                        return ProcessingResult.Skipped($"Unsupported CloudEvent type: {cloudEventType}", json, details);
                    }
                }

                // Step 3: Not a CloudEvent – proceed with the existing notification flow
                // (deserialize into NotificationEvent, validate, test detection, etc.)

                // Step 3a: Deserialize the payload into NotificationEvent
                NotificationEvent notification = this._serializer.Deserialize<NotificationEvent>(jsonElement);
                details = notification.Details;

                // Step 3b: Validate deserialized notification
                if (this._validator.Validate(ref notification) is HealthCheck.ERROR_Invalid)
                {
                    return ProcessingResult.NotPossible(
                        ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message,
                        json, notification.Details);
                }

                // Step 3c: Ping/test detection – silently skip
                if (IsTest(notification))
                {
                    return ProcessingResult.Skipped(
                        ApiResources.Processing_ERROR_Notification_Test, json, details);
                }

                // Step 3d: Determine business scenario
                INotifyScenario scenario = await this._resolver.DetermineScenarioAsync(notification);

                // Step 3e: Special handling for Kto scenario
                if (scenario is KtoScenario)
                {
                    try
                    {
                        WebQueries.KTO.Models.KtoScenario ktoScenario = _ktoScenarioFactory.Create();
                        HttpRequestResponse ktoResponse = await ktoScenario.SendKtoAsync(notification);

                        return ktoResponse.IsFailure
                            ? ProcessingResult.Failure(ktoResponse.JsonResponse, json, details)
                            : ProcessingResult.Success("Successfully sent KTO to KTO provider", json, details);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }

                // Step 3f: For all other scenarios – query external data (OpenZaak, etc.)
                QueryingDataResponse queryDataResponse;

                if ((queryDataResponse = await scenario.TryGetDataAsync(notification)).IsFailure)
                {
                    string message = string.Format(
                        ApiResources.Processing_ERROR_Scenario_NotificationNotSent,
                        queryDataResponse.Message);

                    return ProcessingResult.Failure(message, json, details);
                }

                // Step 3g: Process the data (e.g., send to Notify NL)
                ProcessingDataResponse processingDataResponse = await scenario.ProcessDataAsync(notification, queryDataResponse.Content);

                return processingDataResponse.IsFailure
                    ? ProcessingResult.Failure(
                        string.Format(ApiResources.Processing_ERROR_Scenario_NotificationNotSent,
                            processingDataResponse.Message), json, details)
                    : ProcessingResult.Success(
                        ApiResources.Processing_SUCCESS_Scenario_NotificationSent, json, details);
            }
            catch (Exception exception)
            {
                return HandleException(exception, json, details);
            }
        }

        #region Helper methods

        private static bool IsCloudEvent(JsonElement potentialCloudEvent)
        {
            return potentialCloudEvent.TryGetProperty("specversion", out _) &&
                   potentialCloudEvent.TryGetProperty("type", out _) &&
                   potentialCloudEvent.TryGetProperty("source", out _) &&
                   potentialCloudEvent.TryGetProperty("id", out _);
        }

        private static bool IsTest(NotificationEvent notification)
        {
            const string testUrl = "http://some.hoofdobject.nl/";

            return notification is
            {
                Channel: Channels.Unknown,
                Resource: Resources.Unknown
            } &&
            string.Equals(notification.MainObjectUri.AbsoluteUri, testUrl) &&
            string.Equals(notification.ResourceUri.AbsoluteUri, testUrl);
        }

        private static ProcessingResult HandleException(Exception exception, object json, BaseEnhancedDetails details)
        {
            return exception switch
            {
                JsonException => ProcessingResult.Skipped(exception.Message, json, details),
                NotImplementedException => ProcessingResult.Skipped(ApiResources.Processing_ERROR_Scenario_NotImplemented, json, details),
                AbortedNotifyingException => ProcessingResult.Aborted(exception.Message, json, details),
                NotifyClientException => ProcessingResult.Failure(
                    string.Format(ApiResources.Processing_ERROR_Exception_Notify, exception.Message), json, details),
                _ => ProcessingResult.Failure(
                    string.Format(ApiResources.Processing_ERROR_Exception_Unhandled, exception.GetType().Name, exception.Message), json, details)
            };
        }

        #endregion
    }
}