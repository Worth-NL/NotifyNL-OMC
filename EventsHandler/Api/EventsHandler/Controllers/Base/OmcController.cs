// © 2024, Worth Systems.

using Asp.Versioning;
using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.Controllers.Base
{
    /// <summary>
    /// Parent of all API Controllers in "NotifyNL" OMC.
    /// </summary>
    [ApiController]
    [Route(DefaultValues.ApiController.Route)]
    [Consumes(DefaultValues.Request.ContentType)]
    [Produces(DefaultValues.Request.ContentType)]
    [ApiVersion(DefaultValues.ApiController.Version)]
    // Swagger UI
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]  // REASON: JWT Token is invalid or expired
    public abstract class OmcController : Controller
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OmcController"/> class.
        /// </summary>
        /// <param name="logger">The generic logger to be used.</param>
        protected OmcController(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Logs the API response.
        /// </summary>
        protected void LogApiResponse(LogLevel logLevel, ObjectResult objectResult)
        {
            _ = LogAndReturnApiResponse(logLevel, objectResult);
        }

        /// <summary>
        /// Logs and returns the API response.
        /// </summary>
        protected ObjectResult LogAndReturnApiResponse(LogLevel logLevel, ObjectResult objectResult)
        {
            // Determine log message based on the received ObjectResult
            string logMessage = objectResult.Value switch
            {
                // Description with message
                BaseEnhancedStandardResponseBody enhancedResponse => enhancedResponse.ToString(),
                BaseSimpleStandardResponseBody simpleResponse => simpleResponse.ToString(),
                
                // Only description
                BaseApiStandardResponseBody baseResponse => baseResponse.ToString(),
                
                // Unknown object result
                _ => $"Not standardized API response | {objectResult.StatusCode} | {nameof(objectResult.Value)}"
            };

            LogApiResponse(logLevel, logMessage);

            return objectResult;
        }

        /// <summary>
        /// Logs the API response.
        /// </summary>
        protected void LogApiResponse(LogLevel logLevel, string logMessage)
        {
            this._logger.Log(logLevel, $"OMC: {logMessage} | {DateTime.Now}");
        }
    }
}