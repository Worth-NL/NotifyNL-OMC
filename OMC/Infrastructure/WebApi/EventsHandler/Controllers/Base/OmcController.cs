// © 2024, Worth Systems.

using Common.Extensions;
using Common.Models.Messages.Base;
using EventsHandler.Attributes.Versioning;
using EventsHandler.Constants;
using EventsHandler.Properties;
using Microsoft.AspNetCore.Mvc;
using Sentry;

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
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseEnhancedStandardResponseBody))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BaseStandardResponseBody))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseStandardResponseBody))]
    [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(BaseStandardResponseBody))]
    public abstract class OmcController : Controller
    {
        /// <summary>
        /// Logs the message and returns the API response.
        /// </summary>
        /// <param name="logLevel">The severity of the log.</param>
        /// <param name="objectResult">The HTTP response object to be analyzed and logged.</param>
        /// <returns>The same <see cref="ObjectResult"/> passed in, after logging.</returns>
        protected internal static ObjectResult LogApiResponse(
            LogLevel logLevel,
            ObjectResult? objectResult)
        {
            if (objectResult == null)
            {
                LogMessage(logLevel, "Null ObjectResult returned from controller");

                return new ObjectResult(null)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            var message = DetermineResultMessage(objectResult);

            // Extract structured details for logging (focus on validation/missing fields)
            var data = objectResult.Value switch
            {
                BaseEnhancedStandardResponseBody enhanced => enhanced.Details,
                BaseSimpleStandardResponseBody simple => simple,
                BaseStandardResponseBody baseResp => baseResp,
                _ => objectResult.Value
            };

            LogMessage(logLevel, message, data);

            return objectResult;
        }

        /// <summary>
        /// Logs a simple message without an HTTP response object.
        /// </summary>
        /// <param name="logLevel">The severity of the log.</param>
        /// <param name="logMessage">The message to log.</param>
        protected internal static void LogApiResponse(LogLevel logLevel, string logMessage)
        {
            LogMessage(logLevel, logMessage);
        }

        /// <summary>
        /// Logs an exception and returns the HTTP response object.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="objectResult">The HTTP response object to return (if available).</param>
        /// <returns>The same or fallback <see cref="ObjectResult"/>.</returns>
        protected internal static ObjectResult LogApiResponse(Exception exception, ObjectResult? objectResult)
        {
            LogException(exception);

            return objectResult ?? new ObjectResult(null)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        #region Sentry logging

        /// <summary>
        /// Maps application log levels to Sentry severity levels.
        /// </summary>
        private static readonly Dictionary<LogLevel, SentryLevel> s_logMapping = new()
        {
            { LogLevel.Trace,       SentryLevel.Debug   },
            { LogLevel.Debug,       SentryLevel.Debug   },
            { LogLevel.Information, SentryLevel.Info    },
            { LogLevel.Warning,     SentryLevel.Warning },
            { LogLevel.Error,       SentryLevel.Error   },
            { LogLevel.Critical,    SentryLevel.Fatal   }
        };

        /// <summary>
        /// Logs a message to Sentry with optional structured data.
        /// </summary>
        /// <param name="logLevel">Application log level.</param>
        /// <param name="logMessage">Message to log.</param>
        /// <param name="data">Optional structured payload (e.g. validation details).</param>
        internal static void LogMessage(LogLevel logLevel, string logMessage, object? data = null)
        {
            using (SentrySdk.PushScope())
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.SetTag("log.level", logLevel.GetEnumName());

                    if (data != null)
                    {
                        scope.SetExtra("response.details", data);
                    }
                });

                SentrySdk.CaptureMessage(
                    string.Format(
                        ApiResources.API_Response_STATUS_Logging,
                        ApiResources.Application_Name,
                        logLevel.GetEnumName(),
                        logMessage),
                    s_logMapping[logLevel]);
            }
        }

        /// <summary>
        /// Logs an exception to Sentry.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        private static void LogException(Exception exception)
        {
            using (SentrySdk.PushScope())
            {
                SentrySdk.CaptureException(exception);
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Determines the log message based on the received <see cref="ObjectResult"/>.
        /// <para>
        /// Format: HTTP Status Code | Description | Message (optional)
        /// </para>
        /// </summary>
        /// <param name="objectResult">The response object to analyze.</param>
        /// <returns>A formatted log message.</returns>
        private static string DetermineResultMessage(ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode?.ToString() ?? "unknown";

            return objectResult.Value switch
            {
                BaseEnhancedStandardResponseBody enhancedResponse => enhancedResponse.ToString(),
                BaseSimpleStandardResponseBody simpleResponse => simpleResponse.ToString(),
                BaseStandardResponseBody baseResponse => baseResponse.ToString(),

                _ => string.Format(
                    ApiResources.API_Response_ERROR_UnspecifiedResponse,
                    statusCode,
                    $"The response type {nameof(objectResult.Value)}")
            };
        }

        #endregion
    }
}