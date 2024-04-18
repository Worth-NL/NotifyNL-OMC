// © 2024, Worth Systems.

using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotifyNL;
using EventsHandler.Behaviors.Responding.Messages.Models.Errors;
using EventsHandler.Controllers.Base;
using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;
using EventsHandler.Services.UserCommunication.Interfaces;
using EventsHandler.Utilities.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller used to get feedback from "NotifyNL" API web service.
    /// </summary>
    /// <seealso cref="OmcController"/>
    public sealed class NotifyController : OmcController
    {
        private readonly ISerializationService _serializer;
        private readonly IRespondingService<ProcessingResult, string> _responder;
        private readonly ITelemetryService _telemetry;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EventsController"/> class.
        /// </summary>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        /// <param name="logger">The logging service registering API events.</param>
        public NotifyController(
            ISerializationService serializer,
            IRespondingService<ProcessingResult, string> responder,
            ITelemetryService telemetry,
            ILogger<NotifyController> logger)
            : base(logger)
        {
            this._serializer = serializer;
            this._responder = responder;
            this._telemetry = telemetry;
        }

        /// <summary>
        /// Callback URL listening to delivery receipt send by "NotifyNL" API web service.
        /// </summary>
        /// <param name="json">The delivery receipt from "NotifyNL" Web service (as a plain JSON object).</param>
        [HttpPost]
        [Route("Confirm")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(DeliveryReceipt), typeof(DeliveryReceiptExample))]  // NOTE: Documentation of expected JSON schema with sample and valid payload values
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The delivery receipt with successful status
        [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(ProcessingFailed.Simplified))]  // REASON: The delivery receipt with failure status
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProcessingFailed.Simplified))]  // REASON: The JSON structure is invalid
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]                                              // REASON: Internal server error (if-else / try-catch-finally handle)
        public async Task<IActionResult> ConfirmAsync([Required, FromBody] object json)
        {
            DeliveryReceipt callback = DeliveryReceipt.Default;
            string callbackDetails = string.Empty;

            try
            {
                // Deserialize received JSON payload
                callback = this._serializer.Deserialize<DeliveryReceipt>(json);

                if (callback.Status is not (DeliveryStatus.PermanentFailure or
                                            DeliveryStatus.TemporaryFailure or
                                            DeliveryStatus.TechnicalFailure))
                {
                    return LogAndReturnApiResponse(LogLevel.Information,
                        this._responder.GetStandardized_Processing_ActionResult(ProcessingResult.Success, callbackDetails = GetCallbackDetails(callback)));
                }

                return LogAndReturnApiResponse(LogLevel.Error,
                    this._responder.GetStandardized_Processing_ActionResult(ProcessingResult.Failure, callbackDetails = GetCallbackDetails(callback)));
            }
            catch (Exception exception)
            {
                // NOTE: If callback.Id == Guid.Empty then to be suspected is exception during DeliveryReceipt deserialization
                callbackDetails = GetErrorDetails(callback, exception);

                return LogAndReturnApiResponse(LogLevel.Critical,
                    this._responder.GetStandardized_Exception_ActionResult(exception));
            }
            finally
            {
                await ReportAndLogStatusAsync(callback, callbackDetails);
            }
        }

        #region Helper methods
        private static string GetCallbackDetails(DeliveryReceipt callback)
        {
            return $"{Resources.Feedback_NotifyNL_SUCCESS_NotificationStatus} {callback.Id}: {callback.Status}.";
        }

        private static string GetErrorDetails(DeliveryReceipt callback, Exception exception)
        {
            return $"{Resources.Feedback_NotifyNL_ERROR_UnexpectedFailure} {callback.Id}: {exception.Message}.";
        }
        
        private async Task ReportAndLogStatusAsync(DeliveryReceipt callback, string callbackDetails)
        {
            if (callback.Reference != null)
            {
                string decodedNotification = callback.Reference.Base64Decode();
                NotificationEvent notification = this._serializer.Deserialize<NotificationEvent>(decodedNotification);
                NotifyMethods notificationMethod = callback.Type.ConvertToNotifyMethod();

                try
                {
                    string result = await this._telemetry.ReportCompletionAsync(notification, notificationMethod, callbackDetails);

                    LogApiResponse(LogLevel.Information, result);
                }
                catch (Exception exception)
                {
                    LogApiResponse(LogLevel.Critical, this._responder.GetStandardized_Exception_ActionResult(exception));
                }
            }
        }
        #endregion
    }
}