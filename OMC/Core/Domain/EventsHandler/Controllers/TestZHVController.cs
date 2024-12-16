// © 2024, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Responses;
using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Controllers.Base;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller used to test other Web API services from which "Notify NL" OMC is dependent.
    /// </summary>
    /// <seealso cref="OmcController"/>
    // Swagger UI
    [ProducesResponseType(StatusCodes.Status202Accepted,
        Type = typeof(BaseStandardResponseBody))] // REASON: The API service is up and running
    [ProducesResponseType(StatusCodes.Status403Forbidden,
        Type = typeof(BaseStandardResponseBody))] // REASON: Incorrect URL or API key to "Notify NL" API service
    public sealed class TestZHVController : OmcController // Swagger UI requires this class to be public
    {
        private readonly IQueryContext _queryContext;
        private readonly IRespondingService<ProcessingResult> _responder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestZHVController"/> class.
        /// </summary>
        /// <param name="queryContext">The adapter containing external Web API queries.</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        public TestZHVController(
            IQueryContext queryContext,
            NotifyResponder responder)
        {
            this._queryContext = queryContext;
            this._responder = responder;
        }

        #region ZHV endpoints

        /// <summary>
        /// Checks the state of the "Open Zaak" Web API service health.
        /// </summary>
        [HttpGet]
        [Route("OpenZaakHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public async Task<IActionResult> OpenZaakHealthCheckAsync()
        {
            try
            {
                // Request
                HttpRequestResponse result = await this._queryContext.GetZaakHealthCheckAsync();

                string healthCheckState = HealthCheckResponse.Get(result.IsSuccess);

                // Response
                return result.IsSuccess
                    // HttpStatus Code: 202 Accepted
                    ? LogApiResponse(LogLevel.Information,
                        this._responder.GetResponse(ProcessingResult.Success(healthCheckState)))
                    // HttpStatus Code: 400 Bad Request
                    : LogApiResponse(LogLevel.Error,
                        this._responder.GetResponse(ProcessingResult.Failure(healthCheckState)));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        /// <summary>
        /// Checks the state of the "Open Klant" Web API service health.
        /// </summary>
        [HttpGet]
        [Route("OpenKlantHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public async Task<IActionResult> OpenKlantHealthCheckAsync()
        {
            try
            {
                // Request
                HttpRequestResponse result = await this._queryContext.GetKlantHealthCheckAsync();

                string healthCheckState = HealthCheckResponse.Get(result.IsSuccess);

                // Response
                return result.IsSuccess
                    // HttpStatus Code: 202 Accepted
                    ? LogApiResponse(LogLevel.Information,
                        this._responder.GetResponse(ProcessingResult.Success(healthCheckState)))
                    // HttpStatus Code: 400 Bad Request
                    : LogApiResponse(LogLevel.Error,
                        this._responder.GetResponse(ProcessingResult.Failure(healthCheckState)));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        /// <summary>
        /// Checks the state of the "Open Besluiten" Web API service health.
        /// </summary>
        [HttpGet]
        [Route("OpenBesluitenHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public async Task<IActionResult> OpenBesluitenHealthCheckAsync()
        {
            try
            {
                // Request
                HttpRequestResponse result = await this._queryContext.GetBesluitenHealthCheckAsync();

                string healthCheckState = HealthCheckResponse.Get(result.IsSuccess);

                // Response
                return result.IsSuccess
                    // HttpStatus Code: 202 Accepted
                    ? LogApiResponse(LogLevel.Information,
                        this._responder.GetResponse(ProcessingResult.Success(healthCheckState)))
                    // HttpStatus Code: 400 Bad Request
                    : LogApiResponse(LogLevel.Error,
                        this._responder.GetResponse(ProcessingResult.Failure(healthCheckState)));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        /// <summary>
        /// Checks the state of the "Open Objecten" Web API service health.
        /// </summary>
        [HttpGet]
        [Route("OpenObjectenHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public async Task<IActionResult> OpenObjectenHealthCheckAsync()
        {
            try
            {
                // Request
                HttpRequestResponse result = await this._queryContext.GetObjectenHealthCheckAsync();

                string healthCheckState = HealthCheckResponse.Get(result.IsSuccess);

                // Response
                return result.IsSuccess
                    // HttpStatus Code: 202 Accepted
                    ? LogApiResponse(LogLevel.Information,
                        this._responder.GetResponse(ProcessingResult.Success(healthCheckState)))
                    // HttpStatus Code: 400 Bad Request
                    : LogApiResponse(LogLevel.Error,
                        this._responder.GetResponse(ProcessingResult.Failure(healthCheckState)));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        /// <summary>
        /// Checks the state of the "Open ObjectTypen" Web API service health.
        /// </summary>
        [HttpGet]
        [Route("OpenObjectTypenHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public async Task<IActionResult> OpenObjectTypenHealthCheckAsync()
        {
            try
            {
                // Request
                HttpRequestResponse result = await this._queryContext.GetObjectTypenHealthCheckAsync();

                string healthCheckState = HealthCheckResponse.Get(result.IsSuccess);

                // Response
                return result.IsSuccess
                    // HttpStatus Code: 202 Accepted
                    ? LogApiResponse(LogLevel.Information,
                        this._responder.GetResponse(ProcessingResult.Success(healthCheckState)))
                    // HttpStatus Code: 400 Bad Request
                    : LogApiResponse(LogLevel.Error,
                        this._responder.GetResponse(ProcessingResult.Failure(healthCheckState)));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }
        #endregion
    }
}