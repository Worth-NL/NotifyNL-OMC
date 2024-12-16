// © 2024, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Responses;
using Common.Settings.Configuration;
using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Controllers.Base;
using EventsHandler.Properties;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller used to test other Web API services from which "Notify NL" OMC is dependent.
    /// </summary>
    /// <seealso cref="OmcController"/>
    // Swagger UI
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(BaseStandardResponseBody))]  // REASON: The API service is up and running
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(BaseStandardResponseBody))]  // REASON: Incorrect URL or API key to "Notify NL" API service
    public sealed class TestOMCController : OmcController  // Swagger UI requires this class to be public
    {
        private readonly WebApiConfiguration _configuration;
        private readonly IRespondingService<ProcessingResult> _responder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestOMCController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        public TestOMCController(
            WebApiConfiguration configuration,
            NotifyResponder responder)
        {
            this._configuration = configuration;
            this._responder = responder;
        }

        #region OMC endpoints
        /// <summary>
        /// Tests if all the configurations (from "appsettings.json" and environment variables) are present and contains non-empty values (if required).
        /// </summary>
        [HttpGet]
        [Route("TestConfigs")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]
        public async Task<IActionResult> TestConfigsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _ = WebApiConfiguration.TestAppSettingsConfigs(this._configuration);
                    _ = WebApiConfiguration.TestEnvVariablesConfigs(this._configuration);

                    // HttpStatus Code: 202 Accepted
                    return LogApiResponse(LogLevel.Information, this._responder.GetResponse(ProcessingResult.Success(ApiResources.Endpoint_Test_OMC_TestConfigs_SUCCESS_ConfigurationsValid)));
                }
                catch (Exception exception)
                {
                    // HttpStatus Code: 500 Internal Server Error
                    return this._responder.GetExceptionResponse(exception);
                }
            });
        }
        #endregion
    }
}