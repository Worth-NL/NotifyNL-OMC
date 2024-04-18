// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Errors;
using EventsHandler.Behaviors.Responding.Messages.Models.Information;
using EventsHandler.Behaviors.Responding.Messages.Models.Successes;
using EventsHandler.Behaviors.Responding.Results.Builder.Interface;
using EventsHandler.Behaviors.Responding.Results.Enums;
using EventsHandler.Behaviors.Responding.Results.Extensions;
using EventsHandler.Constants;
using EventsHandler.Properties;
using EventsHandler.Services.UserCommunication.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace EventsHandler.Services.UserCommunication
{
    /// <inheritdoc cref="IRespondingService{TModel}"/>
    internal sealed class NotificationResponder : IRespondingService<NotificationEvent>
    {
        private readonly IDetailsBuilder _detailsBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationResponder"/> class.
        /// </summary>
        public NotificationResponder(IDetailsBuilder builder)
        {
            this._detailsBuilder = builder;
        }

        #region IRespondingService
        /// <inheritdoc cref="IRespondingService.Get_Exception_ActionResult(Exception)"/>
        ObjectResult IRespondingService.Get_Exception_ActionResult(Exception exception)
        {
            if (exception is HttpRequestException)
            {
                return this._detailsBuilder.Get<ErrorDetails>(Reasons.HttpRequestError, exception.Message).AsResult_400();
            }

            return ((IRespondingService<NotificationEvent>)this).Get_Exception_ActionResult(exception.Message);
        }

        /// <inheritdoc cref="IRespondingService.Get_Exception_ActionResult(string)"/>
        ObjectResult IRespondingService.Get_Exception_ActionResult(string errorMessage)
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

        /// <inheritdoc cref="IRespondingService.Get_Exception_ActionResult(Microsoft.AspNetCore.Mvc.Filters.ResultExecutingContext,System.Collections.Generic.IDictionary{string,string[]})"/>
        ResultExecutingContext IRespondingService.Get_Exception_ActionResult(ResultExecutingContext context, IDictionary<string, string[]> errorDetails)
        {
            if (((IRespondingService)this).ContainsErrorMessage(errorDetails, out string errorMessage))
            {
                context.Result = ((IRespondingService)this).Get_Exception_ActionResult(errorMessage);
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
        /// <inheritdoc cref="IRespondingService{TResult,TDetails}.Get_Processing_Status_ActionResult"/>
        ObjectResult IRespondingService<(ProcessingResult, string), BaseEnhancedDetails>.Get_Processing_Status_ActionResult((ProcessingResult, string) result, BaseEnhancedDetails details)
        {
            return ((IRespondingService<NotificationEvent>)this).Get_Processing_Status_ActionResult(result, details);
        }

        /// <inheritdoc cref="IRespondingService{TResult,TDetails}.Get_Processing_Failed_ActionResult"/>
        ObjectResult IRespondingService<(ProcessingResult, string), BaseEnhancedDetails>.Get_Processing_Failed_ActionResult(BaseEnhancedDetails details)
        {
            return ((IRespondingService<NotificationEvent>)this).Get_Processing_Failed_ActionResult(details);
        }
        #endregion

        #region Implementation
        /// <inheritdoc cref="IRespondingService{TModel}.Get_Processing_Status_ActionResult"/>
        ObjectResult IRespondingService<NotificationEvent>.Get_Processing_Status_ActionResult((ProcessingResult Status, string Description) result, BaseEnhancedDetails details)
        {
            return result.Status switch
            {
                ProcessingResult.Success
                    => new ProcessingSucceeded(result.Description, details).AsResult_202(),

                ProcessingResult.Skipped
                    => new ProcessingSkipped(result.Description).AsResult_206(),

                ProcessingResult.Failure
                    => string.Equals(details.Message, Resources.Operation_RESULT_Deserialization_Failure) ||
                       string.Equals(details.Message, Resources.Deserialization_INFO_UnexpectedData_Notification_Message)
                        ? new ProcessingFailed.Detailed(HttpStatusCode.UnprocessableEntity, result.Description, details).AsResult_422()
                        : new ProcessingFailed.Simplified(HttpStatusCode.BadRequest, result.Description).AsResult_400(),

                _ => ObjectResultExtensions.AsResult_501()
            };
        }

        /// <inheritdoc cref="IRespondingService{TModel}.Get_Processing_Failed_ActionResult"/>
        ObjectResult IRespondingService<NotificationEvent>.Get_Processing_Failed_ActionResult(BaseEnhancedDetails details)
        {
            return ((IRespondingService<NotificationEvent>)this).Get_Processing_Status_ActionResult(
                (ProcessingResult.Failure, Resources.Processing_ERROR_Scenario_NotificationNotSent),
                details);
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