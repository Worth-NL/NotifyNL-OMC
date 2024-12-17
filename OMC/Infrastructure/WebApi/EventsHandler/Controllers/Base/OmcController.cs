// © 2024, Worth Systems.

using Common.Extensions;
using Common.Models.Messages.Base;
using EventsHandler.Attributes.Versioning;
using EventsHandler.Constants;
using EventsHandler.Properties;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.Controllers.Base
{
    /// <summary>
    /// Parent of all API Controllers in "Notify NL" OMC.
    /// </summary>
    [ApiController]
    [OmcApiVersion]
    [Route(ApiValues.Default.ApiController.Route)]
    [Consumes(ApiValues.Default.ApiController.ContentType)]
    [Produces(ApiValues.Default.ApiController.ContentType)]
    // Swagger UI
    [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(BaseEnhancedStandardResponseBody))]  // REASON: The HTTP Request wasn't successful
    [ProducesResponseType(StatusCodes.Status401Unauthorized,        Type = typeof(BaseStandardResponseBody))]          // REASON: JWT Token is invalid or expired
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseStandardResponseBody))]          // REASON: Unexpected internal error
    [ProducesResponseType(StatusCodes.Status501NotImplemented,      Type = typeof(BaseStandardResponseBody))]          // REASON: Something is not implemented
    public abstract class OmcController : Controller
    {
        /// <summary>
        /// Logs the message and returns the API response.
        /// </summary>
        /// <param name="logLevel">The severity of the log.</param>
        /// <param name="objectResult">The result to be analyzed before logging it.</param>
        protected internal static ObjectResult LogApiResponse(LogLevel logLevel, ObjectResult objectResult)
        {
            LogMessage(logLevel, DetermineResultMessage(objectResult));

            return objectResult;
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="logLevel">The severity of the log.</param>
        /// <param name="logMessage">The message to be logged.</param>
        protected internal static void LogApiResponse(LogLevel logLevel, string logMessage)
        {
            LogMessage(logLevel, logMessage);
        }

        /// <summary>
        /// Logs the exception and returns the API response.
        /// </summary>
        /// <param name="exception">The exception to be passed.</param>
        /// <param name="objectResult">The result to be analyzed before logging it.</param>
        protected internal static ObjectResult LogApiResponse(Exception exception, ObjectResult objectResult)
        {
            LogException(exception);

            return objectResult;
        }

        #region Sentry logging
        private static readonly Dictionary<LogLevel, SentryLevel> s_logMapping = new()
        {
            { LogLevel.Trace,       SentryLevel.Debug   },
            { LogLevel.Debug,       SentryLevel.Debug   },
            { LogLevel.Information, SentryLevel.Info    },
            { LogLevel.Warning,     SentryLevel.Warning },
            { LogLevel.Error,       SentryLevel.Error   },
            { LogLevel.Critical,    SentryLevel.Fatal   }
        };

        /// <inheritdoc cref="SentrySdk.CaptureMessage(string, SentryLevel)"/>
        internal static void LogMessage(LogLevel logLevel, string logMessage)
        {
            _ = SentrySdk.CaptureMessage(
                    message: string.Format(ApiResources.API_Response_STATUS_Logging, ApiResources.Application_Name, logLevel.GetEnumName(), logMessage),
                    level: s_logMapping[logLevel]);
        }

        /// <inheritdoc cref="SentrySdk.CaptureException(Exception)"/>
        private static void LogException(Exception exception)
        {
            _ = SentrySdk.CaptureException(exception);
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Determines the log message based on the received <see cref="ObjectResult"/>.
        /// <para>
        ///   The format:
        ///   <code>
        ///     HTTP Status Code | Description | Message (optional) | Cases (optional)
        ///   </code>
        /// </para>
        /// </summary>
        private static string DetermineResultMessage(ObjectResult objectResult)
        {
            return objectResult.Value switch
            {
                // Description with message
                BaseEnhancedStandardResponseBody enhancedResponse => enhancedResponse.ToString(),
                BaseSimpleStandardResponseBody simpleResponse => simpleResponse.ToString(),

                // Only description
                BaseStandardResponseBody baseResponse => baseResponse.ToString(),

                // Unknown object result
                _ => string.Format(ApiResources.API_Response_ERROR_UnspecifiedResponse, objectResult.StatusCode, $"The response type {nameof(objectResult.Value)}")
            };
        }
        #endregion
    }
}