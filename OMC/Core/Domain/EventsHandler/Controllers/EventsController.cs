// © 2023, Worth Systems.

using Common.Extensions;
using Common.Models.Messages.Base;
using Common.Models.Responses;
using Common.Versioning.Interfaces;
using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Controllers.Base;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Utilities.Swagger.Examples;
using EventsHandler.Versioning;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;
using WebQueries.Versioning;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller handling events workflow between "Notificatie API" events queue, services with citizens personal
    /// data from the municipalities in The Netherlands ("OpenZaak" and "OpenKlaant"), and "Notify NL" API service.
    /// </summary>
    /// <seealso cref="OmcController"/>
    public sealed class EventsController : OmcController  // Swagger UI requires this class to be public
    {
        private readonly IProcessingService _processor;
        private readonly IRespondingService<ProcessingResult> _responder;
        private readonly IVersionRegister _omcRegister;
        private readonly IVersionRegister _zhvRegister;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsController"/> class.
        /// </summary>
        /// <param name="processor">The input processing service (business logic).</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        /// <param name="omcRegister">The OMC version register.</param>
        /// <param name="zhvRegister">The ZHV version register.</param>
        public EventsController(IProcessingService processor, OmcResponder responder, OmcVersionRegister omcRegister, ZhvVersionRegister zhvRegister)
        {
            this._processor = processor;
            this._responder = responder;
            this._omcRegister = omcRegister;
            this._zhvRegister = zhvRegister;
        }

        /// <summary>
        /// Callback URL listening to notifications from subscribed channels sent by "Open Notificaties" Web API service.
        /// </summary>
        /// <remarks>
        ///   NOTE: This endpoint will start processing business logic after receiving initial notification from "Open Notificaties" Web API service.
        /// </remarks>
        /// <param name="json">The notification from "OpenNotificaties" Web API service (as a plain JSON object).</param>
        [HttpPost]
        [Route("Listen")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(NotificationEvent), typeof(NotificationEventExample))]  // NOTE: Documentation of expected JSON schema with sample and valid payload values
        [ProducesResponseType(StatusCodes.Status202Accepted,           Type = typeof(BaseStandardResponseBody))]          // REASON: The notification was sent to "Notify NL" Web API service
        [ProducesResponseType(StatusCodes.Status206PartialContent,     Type = typeof(BaseEnhancedStandardResponseBody))]  // REASON: Test ping notification was received, serialization failed
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed, Type = typeof(BaseEnhancedStandardResponseBody))]  // REASON: Some conditions predeceasing the request were not met
        public async Task<IActionResult> ListenAsync([Required, FromBody] object json)
        {
            /* The validation of JSON payload structure and model-binding of [Required] properties are
             * happening on the level of [FromBody] annotation. The attribute [StandardizeApiResponses]
             * is meant to intercept native framework errors, raised immediately by ASP.NET Core validation
             * mechanism, and to re-pack them ("beautify") into user-friendly standardized API responses */
            try
            {
                // Try to process the received notification
                ProcessingResult result = await this._processor.ProcessAsync(json);

                return LogApiResponse(result.Status.ConvertToLogLevel(),  // LogLevel
                    this._responder.GetResponse(result));
            }
            catch (Exception exception)
            {
                // Unhandled problems occurred during the attempt to process the notification
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        /// <summary>
        /// Gets the current version and setup of the OMC (Output Management Component).
        /// </summary>
        [HttpGet]
        [Route("Version")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public IActionResult Version()
        {
            LogApiResponse(LogLevel.Trace, ApiResources.Endpoint_Events_Version_INFO_ApiVersionRequested);

            return Ok(this._omcRegister.GetVersion(
                      this._zhvRegister.GetVersion()));
        }
    }
}