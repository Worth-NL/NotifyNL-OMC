// © 2024, Worth Systems.

using Asp.Versioning;
using EventsHandler.Attributes.Authorization;
using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotifyNL;
using EventsHandler.Behaviors.Responding.Messages.Models.Errors;
using EventsHandler.Constants;
using EventsHandler.Controllers.Base;
using EventsHandler.Services.Serialization.Interfaces;
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
    /// <seealso cref="Controller"/>
    [ApiController]
    [Route(DefaultValues.ApiController.Route)]
    [Consumes(DefaultValues.Request.ContentType)]
    [Produces(DefaultValues.Request.ContentType)]
    [ApiVersion(DefaultValues.ApiController.Version)]
    public sealed class NotifyController : OmcController
    {
        private readonly ISerializationService _serializer;
        private readonly IRespondingService<ProcessingResult, string> _responder;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsController"/> class.
        /// </summary>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        /// <param name="logger">The logging service.</param>
        public NotifyController(
            ISerializationService serializer,
            IRespondingService<ProcessingResult, string> responder,
            ILogger<NotifyController> logger)
            : base(logger)
        {
            this._serializer = serializer;
            this._responder = responder;
        }

        /// <summary>
        /// Callback URL listening to delivery receipt send by "NotifyNL" API web service.
        /// </summary>
        /// <param name="json">The delivery receipt from "NotifyNL" Web service (as a plain JSON object).</param>
        [HttpPost]
        [Route("Confirm")]
        // Security
        [ApiAuthorization]
        [SwaggerRequestExample(typeof(DeliveryReceipt), typeof(DeliveryReceiptExample))]  // NOTE: Documentation of expected JSON schema with sample and valid payload values
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                // REASON: The delivery receipt with successful status
        [ProducesResponseType(StatusCodes.Status400BadRequest,   Type = typeof(ProcessingFailed.Detailed))]  // REASON: The delivery receipt with failure status
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]                     // REASON: JWT Token is invalid or expired
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]                                     // REASON: Internal server error (if-else / try-catch-finally handle)
        public IActionResult Confirm([Required, FromBody] object json)
        {
            try
            {
                // Deserialize received JSON payload
                DeliveryReceipt callback = this._serializer.Deserialize<DeliveryReceipt>(json);

                if (callback.Status is not (DeliveryStatus.PermanentFailure or
                                            DeliveryStatus.TemporaryFailure or
                                            DeliveryStatus.TechnicalFailure))
                {
                    return LogAndReturnApiResponse(LogLevel.Information,
                        this._responder.GetStandardized_Processing_ActionResult(ProcessingResult.Success, GetCallbackDetails(callback)));
                }

                return LogAndReturnApiResponse(LogLevel.Error,
                    this._responder.GetStandardized_Processing_ActionResult(ProcessingResult.Failure, GetCallbackDetails(callback)));
            }
            catch (Exception exception)
            {
                return LogAndReturnApiResponse(LogLevel.Critical,
                    this._responder.GetStandardized_Exception_ActionResult(exception));
            }
        }

        private static string GetCallbackDetails(DeliveryReceipt callback)
        {
            return $"The status of notification with ID {callback.Id} is: {callback.Status}.";
        }
    }
}