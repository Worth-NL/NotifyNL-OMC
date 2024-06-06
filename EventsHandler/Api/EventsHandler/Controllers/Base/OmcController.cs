// © 2024, Worth Systems.

using Asp.Versioning;
using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Constants;
using EventsHandler.Properties;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.Controllers.Base
{
    /// <summary>
    /// Parent of all API Controllers in "Notify NL" OMC.
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
        // ReSharper disable MemberCanBeMadeStatic.Global

        /// <summary>
        /// Logs the message and returns the API response.
        /// </summary>
        /// <param name="logLevel">The severity of the log.</param>
        /// <param name="objectResult">The result to be analyzed before logging it.</param>
        protected ObjectResult LogApiResponse(LogLevel logLevel, ObjectResult objectResult)
        {
            LogMessage(logLevel, DetermineResultMessage(objectResult));

            return objectResult;
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="logLevel">The severity of the log.</param>
        /// <param name="logMessage">The message to be logged.</param>
        protected void LogApiResponse(LogLevel logLevel, string logMessage)
        {
            LogMessage(logLevel, logMessage);
        }

        /// <summary>
        /// Logs the exception and returns the API response.
        /// </summary>
        /// <param name="exception">The exception to be passed.</param>
        /// <param name="objectResult">The result to be analyzed before logging it.</param>
        protected ObjectResult LogApiResponse(Exception exception, ObjectResult objectResult)
        {
            LogException(exception);

            return objectResult;
        }
        
        // ReSharper enable MemberCanBeMadeStatic.Global

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
            _ = SentrySdk.CaptureMessage($"{Resources.Application_Name} | {logLevel:G} | {logMessage}", s_logMapping[logLevel]);
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
        /// </summary>
        private static string DetermineResultMessage(ObjectResult objectResult)
        {
            return objectResult.Value switch
            {
                // Description with message
                BaseEnhancedStandardResponseBody enhancedResponse => enhancedResponse.ToString(),
                BaseSimpleStandardResponseBody simpleResponse => simpleResponse.ToString(),
                
                // Only description
                BaseApiStandardResponseBody baseResponse => baseResponse.ToString(),

                // Unknown object result
                _ => $"{Resources.Processing_ERROR_UnspecifiedResponse} | {objectResult.StatusCode} | {nameof(objectResult.Value)}"
            };
        }
        #endregion
    }
}