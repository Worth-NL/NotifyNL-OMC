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
            GeneralResponder responder)
        {
            this._queryContext = queryContext;
            this._responder = responder;
        }

        #region ZHV Test endpoints
        private const string Test = nameof(Test);
        private const string ZHV = nameof(ZHV);
        private const string UrlStart = $"/{Test}/{ZHV}/";

        /// <summary>
        /// Checks the state of the "Open Zaak" Web API service health.
        /// </summary>
        [HttpGet]
        [Route($"{UrlStart}OpenZaakHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public Task<IActionResult> OpenZaakHealthCheckAsync()
        {
            return ExecuteHealthCheckAsync(() => this._queryContext.GetZaakHealthCheckAsync());
        }

        /// <summary>
        /// Checks the state of the "Open Klant" Web API service health.
        /// </summary>
        [HttpGet]
        [Route($"{UrlStart}OpenKlantHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public Task<IActionResult> OpenKlantHealthCheckAsync()
        {
            return ExecuteHealthCheckAsync(() => this._queryContext.GetKlantHealthCheckAsync());
        }

        /// <summary>
        /// Checks the state of the "Open Besluiten" Web API service health.
        /// </summary>
        [HttpGet]
        [Route($"{UrlStart}OpenBesluitenHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public Task<IActionResult> OpenBesluitenHealthCheckAsync()
        {
            return ExecuteHealthCheckAsync(() => this._queryContext.GetBesluitenHealthCheckAsync());
        }

        /// <summary>
        /// Checks the state of the "Open Objecten" Web API service health.
        /// </summary>
        [HttpGet]
        [Route($"{UrlStart}OpenObjectenHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public Task<IActionResult> OpenObjectenHealthCheckAsync()
        {
            return ExecuteHealthCheckAsync(() => this._queryContext.GetObjectenHealthCheckAsync());
        }

        /// <summary>
        /// Checks the state of the "Open ObjectTypen" Web API service health.
        /// </summary>
        [HttpGet]
        [Route($"{UrlStart}OpenObjectTypenHealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public Task<IActionResult> OpenObjectTypenHealthCheckAsync()
        {
            return ExecuteHealthCheckAsync(() => this._queryContext.GetObjectTypenHealthCheckAsync());
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Generic method for health checks.
        /// </summary>
        /// <param name="healthCheckFunc">The function to call for the service health check.</param>
        /// <returns>An IActionResult representing the health check status.</returns>
        private async Task<IActionResult> ExecuteHealthCheckAsync(Func<Task<HttpRequestResponse>> healthCheckFunc)
        {
            try
            {
                // Execute the health check
                HttpRequestResponse result = await healthCheckFunc();
                return DetermineHealthCheckResponse(result);
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception, this._responder.GetExceptionResponse(exception));
            }
        }

        /// <summary>
        /// Handles HttpRequestResponse and constructs an appropriate response.
        /// </summary>
        /// <param name="result">The HTTP request response to evaluate.</param>
        /// <returns>An ObjectResult with the appropriate response.</returns>
        private ObjectResult DetermineHealthCheckResponse(HttpRequestResponse result)
        {
            string healthCheckState = HealthCheckResponse.Get(result.IsSuccess);

            // Determine response based on the success of the result
            return result.IsSuccess
                ? LogApiResponse(LogLevel.Information,
                    this._responder.GetResponse(ProcessingResult.Success(healthCheckState))) // 202 Accepted
                : LogApiResponse(LogLevel.Error,
                    this._responder.GetResponse(ProcessingResult.Failure(healthCheckState))); // 400 Bad Request
        }
        #endregion
    }
}