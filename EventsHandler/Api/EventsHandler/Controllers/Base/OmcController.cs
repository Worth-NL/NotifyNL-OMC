// © 2024, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.Controllers.Base
{
    /// <summary>
    /// Parent of all API Controllers in NotifyNL OMC.
    /// </summary>
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
        /// Logs and returns the API response.
        /// </summary>
        protected ObjectResult LogAndReturnApiResponse(LogLevel logLevel, ObjectResult objectResult)
        {
            DateTime logTime = DateTime.Now;
            string logMessage = objectResult.Value switch
            {
                // Description with message
                BaseEnhancedStandardResponseBody detailedResponseBody
                    => $"{detailedResponseBody.StatusDescription} | {detailedResponseBody.Details.Message}",
                
                // Only description
                BaseApiStandardResponseBody shortResponseBody
                    => shortResponseBody.StatusDescription,
                
                // Unknown object result
                _ => $"Not standardized API response | {objectResult.StatusCode} | {nameof(objectResult.Value)}"
            };

            #pragma warning disable CA2254  // The logged message will be dynamic by its nature in this case
            this._logger.Log(logLevel, $"OMC: {logMessage} | {logTime}");
            #pragma warning restore CA2254

            return objectResult;
        }
    }
}
