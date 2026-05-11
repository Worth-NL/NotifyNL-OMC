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
using WebQueries.MijnOverheid.Interfaces;
using WebQueries.MijnOverheid.Models;
using WebQueries.Versioning;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller handling events workflow between "Notificatie API" events queue, services with citizens personal
    /// data from the municipalities in The Netherlands ("OpenZaak" and "OpenKlaant"), and "Notify NL" API service.
    /// </summary>
    /// <seealso cref="OmcController"/>
    public sealed class EventsController : OmcController // Swagger UI requires this class to be public
    {
        private readonly IProcessingService _processor;
        private readonly IRespondingService<ProcessingResult> _responder;
        private readonly IVersionRegister _omcRegister;
        private readonly IVersionRegister _zhvRegister;
        private readonly IMijnOverheidForwarder _mijnOverheidForwarder;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsController"/> class.
        /// </summary>
        /// <param name="processor">The input processing service (business logic).</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        /// <param name="omcRegister">The OMC version register.</param>
        /// <param name="zhvRegister">The ZHV version register.</param>
        /// <param name="mijnOverheidForwarder"></param>
        public EventsController(
            IProcessingService processor, 
            NotificationEventResponder responder,
            OmcVersionRegister omcRegister, 
            ZhvVersionRegister zhvRegister,
            IMijnOverheidForwarder mijnOverheidForwarder)
        {
            this._processor = processor;
            this._responder = responder;
            this._omcRegister = omcRegister;
            this._zhvRegister = zhvRegister;
            this._mijnOverheidForwarder = mijnOverheidForwarder;
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
        [AspNetExceptionsHandler] // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(NotificationEvent),
            typeof(NotificationEventExample))] // NOTE: Documentation of expected JSON schema with sample and valid payload values
        [ProducesResponseType(StatusCodes.Status202Accepted,
            Type = typeof(BaseStandardResponseBody))] // REASON: The notification was sent to "Notify NL" Web API service
        [ProducesResponseType(StatusCodes.Status206PartialContent,
            Type =
                typeof(BaseEnhancedStandardResponseBody))] // REASON: Test ping notification was received, serialization failed
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed,
            Type =
                typeof(BaseEnhancedStandardResponseBody))] // REASON: Some conditions predeceasing the request were not met
        public async Task<IActionResult> ListenAsync([Required, FromBody] object json)
        {
            /* The validation of JSON payload structure and model-binding of [Required] properties are
             * happening on the level of [FromBody] annotation. The attribute [AspNetExceptionsHandler]
             * is meant to intercept native framework errors, raised immediately by ASP.NET Core validation
             * mechanism, and to re-pack them ("beautify") into user-friendly standardized API responses */
            try
            {
                // Try to process the received notification
                ProcessingResult result = await this._processor.ProcessAsync(json);

                return LogApiResponse(result.Status.ConvertToLogLevel(), // LogLevel
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
        /// Callback URL listening to CloudEvents from ZGW Web API service.
        /// Forwards the event to MijnOverheid after validation and returns the response.
        /// </summary>
        /// <remarks>
        ///   This endpoint receives CloudEvents (e.g., zaak-gemuteerd, zaak-geopend, zaak-verwijderd),
        ///   checks whitelist and notification permissions (for gemuteerd events), and forwards to MijnOverheid.
        /// </remarks>
        /// <param name="cloudEvent">The CloudEvent from ZGW API (application/cloudevents+json).</param>
        [HttpPost]
        [Route("cloudevents")]
        [ApiAuthorization]
        [AspNetExceptionsHandler]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReceiveCloudEventAsync([Required, FromBody] CloudEvent cloudEvent)
        {
            try
            {
                if (cloudEvent == null || string.IsNullOrEmpty(cloudEvent.Type))
                {
                    ObjectResult errorResponse = _responder.GetExceptionResponse("CloudEvent is missing or has no 'type' property.");
                    return LogApiResponse(LogLevel.Warning, errorResponse);
                }

                MijnOverheidResponse? moResponse = await _mijnOverheidForwarder.ForwardIfNeededAsync(cloudEvent);

                // Skipped – return 200 OK with a simple message
                return LogApiResponse(LogLevel.Information, moResponse == null ? Ok("Event was not forwarded (skipped).") : StatusCode(moResponse.StatusCode, moResponse.ResponseBody));

                // Return exactly what MijnOverheid returned: status code and response body
            }
            catch (Exception exception)
            {
                return LogApiResponse(exception, _responder.GetExceptionResponse(exception));
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
        [AspNetExceptionsHandler] // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
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