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
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;
using WebQueries.MijnOverheid.Interfaces;
using WebQueries.MijnOverheid.Models;
using WebQueries.Tracing;
using WebQueries.Versioning;
using ZgwModels.Mapping.Events;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using CloudEvent = ZgwModels.Mapping.Events.CloudEvent;

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
        private readonly IVersionRegister _zgwRegister;
        private readonly IMijnOverheidForwarder _mijnOverheidForwarder;
        private readonly CloudEventNormalizer _normalizer;
        private readonly TraceEmitter _traceEmitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsController"/> class.
        /// </summary>
        /// <param name="processor">The input processing service (business logic).</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        /// <param name="omcRegister">The OMC version register.</param>
        /// <param name="zgwRegister">The ZGW version register.</param>
        /// <param name="mijnOverheidForwarder">The forwarder to MijnOverheid.</param>
        /// <param name="normalizer">The CloudEvent normalizer for incoming payloads.</param>
        /// <param name="traceEmitter">Broadcasts real-time processing steps to the dashboard.</param>
        public EventsController(
            IProcessingService processor,
            NotificationEventResponder responder,
            OmcVersionRegister omcRegister,
            ZgwVersionRegister zgwRegister,
            IMijnOverheidForwarder mijnOverheidForwarder,
            CloudEventNormalizer normalizer,
            TraceEmitter traceEmitter)
        {
            this._processor = processor;
            this._responder = responder;
            this._omcRegister = omcRegister;
            this._zgwRegister = zgwRegister;
            this._mijnOverheidForwarder = mijnOverheidForwarder;
            this._normalizer = normalizer;
            this._traceEmitter = traceEmitter;
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
        /// <param name="json">The incoming JSON payload (CloudEvent or NotificationEvent).</param>
        [HttpPost]
        [Route("MijnZaken")]
        [ApiAuthorization]
        [AspNetExceptionsHandler]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MOMZAsync([Required, FromBody] object json)
        {
            try
            {
                // 1. Normalize the incoming payload to a unified CloudEvent
                CloudEvent? cloudEvent = _normalizer.Normalize(JObject.Parse($"{json}"), out string? reason);
                if (cloudEvent == null)
                {
                    ObjectResult errorResponse = _responder.GetExceptionResponse(reason ?? "Unsupported payload format or missing required fields.");
                    return LogApiResponse(LogLevel.Warning, errorResponse);
                }

                // A real, understood event is entering the MijnZaken pipeline — start a trace for
                // the dashboard (no-op if nobody is watching; cleared in the finally block below
                // regardless). Which of the three flows this becomes is set inside
                // ForwardIfNeededAsync as soon as the CloudEvent type is parsed.
                TraceContext.Start(this._traceEmitter);
                TraceContext.Emit("oneground", "ok");
                TraceContext.Emit("output-patronen", "ok");

                // 2. Forward if needed
                MijnOverheidResponse? moResponse = await _mijnOverheidForwarder.ForwardIfNeededAsync((CloudEvent)cloudEvent);

                // 3. Return response — the trace's final leg: OMC's own HTTP response to this
                // request travels back to whichever system called /Events/MijnZaken (Oneground),
                // carrying whatever Logius said (or the "skipped" outcome, if a filter aborted
                // before Logius was ever called). The graph is undirected for pathfinding (see
                // tracePath.ts), so re-emitting "oneground" here — the same stage the trace
                // started on — is enough for the dashboard to animate the round trip back.
                if (moResponse == null)
                {
                    TraceContext.Emit("oneground", "ok", "Event was not forwarded (skipped) — reporting back to Oneground");
                    return LogApiResponse(LogLevel.Information, Ok("Event was not forwarded (skipped)."));
                }

                TraceContext.Emit("oneground", moResponse.IsSuccess ? "ok" : "fail",
                    $"Returning Logius response (HTTP {moResponse.StatusCode}) to Oneground");
                return LogApiResponse(LogLevel.Information, StatusCode(moResponse.StatusCode, moResponse.ResponseBody));
            }
            catch (Exception exception)
            {
                // Most call sites only emit "start" before a register/gate call, not a try/catch
                // around every single one — without this, an exception here would otherwise leave
                // the dashboard's trace hanging on that step forever instead of showing the failure.
                TraceContext.EmitPendingFailure(exception.Message);
                return LogApiResponse(exception, _responder.GetExceptionResponse(exception));
            }
            finally
            {
                // Never leak this trace's ambient state into unrelated work on a reused context.
                TraceContext.Clear();
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
                this._zgwRegister.GetVersion()));
        }
    }
}