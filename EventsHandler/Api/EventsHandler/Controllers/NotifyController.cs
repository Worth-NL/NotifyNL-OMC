// © 2024, Worth Systems.

using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Controllers.Base;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Models.POCOs.NotifyNL;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Responding.Messages.Models.Errors;
using EventsHandler.Utilities.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller used to get feedback from "Notify NL" Web API service.
    /// </summary>
    /// <seealso cref="OmcController"/>
    public sealed class NotifyController : OmcController
    {
        private readonly NotifyResponder _responder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyController"/> class.
        /// </summary>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        public NotifyController(IRespondingService<ProcessingResult, string> responder)
        {
            this._responder = (NotifyResponder)responder;
        }

        /// <summary>
        /// Callback URL listening to delivery receipt (callback) sent by "Notify NL" Web API service.
        /// </summary>
        /// <param name="json">The delivery receipt from "Notify NL" Web API service (as a plain JSON object).</param>
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
            return await this._responder.HandleNotifyCallbackAsync(json);
        }
    }
}