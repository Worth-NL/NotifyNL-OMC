// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Responding.Messages.Models.Details;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using EventsHandler.Services.Responding.Messages.Models.Errors;
using EventsHandler.Services.Responding.Messages.Models.Information;
using EventsHandler.Services.Responding.Messages.Models.Successes;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using EventsHandler.Services.Responding.Results.Enums;
using EventsHandler.Services.Responding.Results.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace EventsHandler.Services.Responding
{
    /// <inheritdoc cref="IRespondingService{TModel}"/>
    internal sealed class OmcResponder : IRespondingService<NotificationEvent>
    {
        private readonly IDetailsBuilder _detailsBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="OmcResponder"/> class.
        /// </summary>
        public OmcResponder(IDetailsBuilder builder)
        {
            this._detailsBuilder = builder;
        }

        #region IRespondingService
        /// <inheritdoc cref="IRespondingService.GetExceptionResponse(Exception)"/>
        ObjectResult IRespondingService.GetExceptionResponse(Exception exception)
        {
            if (exception is HttpRequestException)
            {
                return this._detailsBuilder.Get<ErrorDetails>(Reasons.HttpRequestError, exception.Message).AsResult_400();
            }

            return ((IRespondingService<NotificationEvent>)this).GetExceptionResponse(exception.Message);
        }

        /// <inheritdoc cref="IRespondingService.GetExceptionResponse(string)"/>
        ObjectResult IRespondingService.GetExceptionResponse(string errorMessage)
        {
            try
            {
                // JSON serialization issues
                if (errorMessage.StartsWith(DefaultValues.Validation.Deserialization_MissingProperty))
                {
                    return this._detailsBuilder.Get<ErrorDetails>(Reasons.MissingProperties_Notification, GetMissingPropertiesNames(errorMessage)).AsResult_422();
                }

                return errorMessage.StartsWith(DefaultValues.Validation.Deserialization_InvalidValue)
                    // JSON serialization issues
                    ? this._detailsBuilder.Get<ErrorDetails>(Reasons.InvalidProperties_Notification, errorMessage).AsResult_422()
                    // Invalid JSON structure
                    : this._detailsBuilder.Get<ErrorDetails>(Reasons.InvalidJson, errorMessage).AsResult_422();
            }
            catch
            {
                return this._detailsBuilder.Get<UnknownDetails>(Reasons.ValidationIssue).AsResult_500();
            }
        }

        /// <inheritdoc cref="IRespondingService.GetExceptionResponse(ResultExecutingContext, IDictionary{string, string[]})"/>
        ResultExecutingContext IRespondingService.GetExceptionResponse(ResultExecutingContext context, IDictionary<string, string[]> errorDetails)
        {
            if (((IRespondingService)this).ContainsErrorMessage(errorDetails, out string errorMessage))
            {
                context.Result = ((IRespondingService)this).GetExceptionResponse(errorMessage);
            }

            return context;
        }

        /// <inheritdoc cref="IRespondingService.ContainsErrorMessage(IDictionary{string, string[]}, out string)"/>
        bool IRespondingService.ContainsErrorMessage(IDictionary<string, string[]> errorDetails, out string errorMessage)
        {
            const int messageIndex = 0;     // NOTE: Under index 1 the source is stored (where the error encountered: property, object)
            const string messageKey = "$";  // NOTE: The data binding validation mechanism is storing error messages under this predefined key + .[source]

            // Known keys where error messages are present for sure
            if (errorDetails.TryGetValue(messageKey,                out string[]? errorMessages) ||
                errorDetails.TryGetValue($"{messageKey}.kanaal",    out           errorMessages) ||
                errorDetails.TryGetValue($"{messageKey}.kenmerken", out           errorMessages))
            {
                errorMessage = errorMessages[messageIndex];

                return true;
            }

            // Dynamic fallback strategy, to retrieve an error message anyway
            KeyValuePair<string, string[]>[] errorDetailsPairs = errorDetails.ToArray();

            for (int index = 0; index < errorDetails.Count; index++)
            {
                if (errorDetailsPairs[index].Key.StartsWith(messageKey))
                {
                    errorMessage = errorDetailsPairs[index].Value[messageIndex];

                    return true;
                }
            }

            errorMessage = Resources.Processing_ERROR_ExecutingContext_UnknownErrorDetails;

            return false;
        }
        #endregion

        #region IRespondingService<TResult, TDetails>
        /// <inheritdoc cref="IRespondingService{TResult, TDetails}.GetResponse(TResult, TDetails)"/>
        ObjectResult IRespondingService<(ProcessingResult, string), BaseEnhancedDetails>.GetResponse((ProcessingResult, string) result, BaseEnhancedDetails details)
        {
            return ((IRespondingService<NotificationEvent>)this).GetResponse(result, details);
        }
        #endregion

        #region Implementation
        /// <inheritdoc cref="IRespondingService{TModel}.GetResponse(ValueTuple{ProcessingResult, string}, BaseEnhancedDetails)"/>
        ObjectResult IRespondingService<NotificationEvent>.GetResponse((ProcessingResult Status, string Description) result, BaseEnhancedDetails details)
        {
            return result.Status switch
            {
                ProcessingResult.Success
                    => new ProcessingSucceeded(result.Description).AsResult_202(),

                ProcessingResult.Skipped or
                ProcessingResult.Aborted
                    => new ProcessingSkipped(result.Description).AsResult_206(),

                ProcessingResult.Failure
                    => details.Message.StartsWith(DefaultValues.Validation.HttpRequest_ErrorMessage)  // NOTE: HTTP Request error messages are always simplified
                        ? new HttpRequestFailed.Simplified(details).AsResult_400()
                        : details.Cases.IsNotEmpty() && details.Reasons.Any()
                            ? new ProcessingFailed.Detailed(HttpStatusCode.UnprocessableEntity, result.Description, details).AsResult_400()
                            : new ProcessingFailed.Simplified(HttpStatusCode.UnprocessableEntity, result.Description).AsResult_400(),

                ProcessingResult.NotPossible
                    => new DeserializationFailed(details).AsResult_422(),

                _ => ObjectResultExtensions.AsResult_501()
            };
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the names of missing JSON properties.
        /// </summary>
        /// <param name="errorMessage">The error message produced by validation framework during model binding.</param>
        /// <returns>
        ///   A single property or comma separated string of properties.
        /// </returns>
        private static string GetMissingPropertiesNames(string errorMessage)
        {
            const int spaceAndOffset = 2;

            int startIndexWhereMissingPropertiesAreListed = errorMessage.LastIndexOf(':') + spaceAndOffset;

            return errorMessage[startIndexWhereMissingPropertiesAreListed..];
        }
        #endregion
    }
}