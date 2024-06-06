// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Responding.Results.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.UserCommunication.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Notify.Exceptions;
using System.Text.RegularExpressions;

namespace EventsHandler.Services.UserCommunication
{
    /// <inheritdoc cref="IRespondingService{TResult, TDetails}"/>
    internal sealed partial class NotifyResponder : IRespondingService<ProcessingResult, string>  // NOTE: "partial" is introduced by the new RegEx generation approach
    {
        #region RegEx patterns
        // -------
        // API Key
        // -------
        [GeneratedRegex("Invalid token: service not found", RegexOptions.Compiled)]
        private static partial Regex InvalidApiKeyPattern();

        // -----
        // Email        
        // -----
        [GeneratedRegex("Address field is required", RegexOptions.Compiled)]
        private static partial Regex MissingEmailAddressPattern();

        [GeneratedRegex("Not a valid email address", RegexOptions.Compiled)]
        private static partial Regex InvalidEmailSymbolsPattern();

        // ------------
        // Phone number
        // ------------
        [GeneratedRegex("Number field is required", RegexOptions.Compiled)]
        private static partial Regex MissingPhoneNumberPattern();

        [GeneratedRegex("Must not contain letters or symbols", RegexOptions.Compiled)]
        private static partial Regex InvalidPhoneSymbolsPattern();

        [GeneratedRegex("Not enough digits", RegexOptions.Compiled)]
        private static partial Regex InvalidPhoneTooShortPattern();

        [GeneratedRegex("Too many digits", RegexOptions.Compiled)]
        private static partial Regex InvalidPhoneTooLongPattern();

        [GeneratedRegex("Please enter mobile number according to the expected format", RegexOptions.Compiled)]
        private static partial Regex InvalidPhoneFormatPattern();

        // ------------------
        // Template or its ID
        // ------------------
        [GeneratedRegex("not a valid UUID", RegexOptions.Compiled)]
        private static partial Regex InvalidTemplateIdFormatPattern();
        
        [GeneratedRegex("Template not found", RegexOptions.Compiled)]
        private static partial Regex NotFoundTemplatePattern();

        // ---------------
        // Personalization
        // ---------------
        [GeneratedRegex("Missing personalisation\\:[a-z.,\\ ]+", RegexOptions.Compiled)]  // NOTE: This is not a typo, "personalization" is written this way in UK English
        private static partial Regex MissingPersonalizationPattern();
        #endregion

        #region IRespondingService
        /// <inheritdoc cref="IRespondingService.Get_Exception_ActionResult(Exception)"/>
        ObjectResult IRespondingService.Get_Exception_ActionResult(Exception exception)
        {
            switch (exception)
            {
                // NOTE: Issues reported by the Notify API .NET client: 400 BadRequest, 403 Forbidden
                case NotifyClientException:
                {
                    string message;
                    Match match;

                    // HttpStatus Code: 403 Forbidden
                    if ((match = InvalidApiKeyPattern().Match(exception.Message)).Success)  // NOTE: The API key is invalid (access to "Notify NL" API service denied)
                    {
                        message = match.Value;

                        // NOTE: This specific error message is inconsistently returned from Notify .NET client with 403 Forbidden status code (unlike others - with 400 BadRequest code)
                        return ObjectResultExtensions.AsResult_403(message);
                    }
                
                    // HttpStatus Code: 400 BadRequest
                    if ((match = InvalidEmailSymbolsPattern().Match(exception.Message)).Success)  // NOTE: The email address is invalid
                    {
                        message = GetEmailErrorMessage(match);
                    }
                    else if ((match = InvalidPhoneSymbolsPattern().Match(exception.Message)).Success  ||  // NOTE: The phone number contains letters or illegal symbols
                             (match = InvalidPhoneTooShortPattern().Match(exception.Message)).Success ||  // NOTE: The phone number contains not enough digits
                             (match = InvalidPhoneTooLongPattern().Match(exception.Message)).Success  ||  // NOTE: The phone number contains too many digits
                             (match = InvalidPhoneFormatPattern().Match(exception.Message)).Success)      // NOTE: The phone number format is invalid: e.g., the country code is unsupported
                    {
                        message = GetPhoneErrorMessage(match);
                    }
                    else if (InvalidTemplateIdFormatPattern().Match(exception.Message).Success)  // NOTE: The template ID is not in UUID (Universal Unique Identifier) format
                    {
                        message = GetTemplateErrorMessage(match);
                    }
                    else if ((match = NotFoundTemplatePattern().Match(exception.Message)).Success ||      // NOTE: The message template could not be find based on provided template ID
                             (match = MissingPersonalizationPattern().Match(exception.Message)).Success)  // NOTE: Personalization was required by message template but wasn't provided
                    {
                        message = match.Value;
                    }
                    else
                    {
                        // Everything else that is throw as NotifyClientException (NOTE: for better UX experience it should be also handled and formatted appropriately)
                        message = exception.Message;
                    }

                    return ((IRespondingService)this).Get_Exception_ActionResult(message);
                }

                // NOTE: Authorization issues wrapped around 403 Forbidden status code
                case NotifyAuthException:
                    return ObjectResultExtensions.AsResult_403(exception.Message);

                // NOTE: Unexpected issues wrapped around 500 Internal Server Error status code
                default:
                    return ObjectResultExtensions.AsResult_500(exception.Message);
            }
        }

        /// <inheritdoc cref="IRespondingService.Get_Exception_ActionResult(string)"/>
        ObjectResult IRespondingService.Get_Exception_ActionResult(string errorMessage)
        {
            string? message = null;
            Match match;

            if ((match = MissingEmailAddressPattern().Match(errorMessage)).Success)  // NOTE: The email address is empty (whitespaces only).
            {
                message = GetEmailErrorMessage(match);
            }
            else if ((match = MissingPhoneNumberPattern().Match(errorMessage)).Success)  // NOTE: The phone number is empty (whitespaces only)
            {
                message = GetPhoneErrorMessage(match);
            }
            
            // HttpStatus Code: 400 BadRequest
            return ObjectResultExtensions.AsResult_400(message ?? errorMessage);
        }

        /// <inheritdoc cref="IRespondingService.Get_Exception_ActionResult(Microsoft.AspNetCore.Mvc.Filters.ResultExecutingContext,System.Collections.Generic.IDictionary{string,string[]})"/>
        ResultExecutingContext IRespondingService.Get_Exception_ActionResult(ResultExecutingContext context, IDictionary<string, string[]> errorDetails)
        {
            if (((IRespondingService)this).ContainsErrorMessage(errorDetails, out string errorMessage))
            {
                // HttpStatus Code: 400 BadRequest
                context.Result = ((IRespondingService)this).Get_Exception_ActionResult(errorMessage);
            }

            return context;
        }

        /// <inheritdoc cref="IRespondingService.ContainsErrorMessage(IDictionary{string, string[]}, out string)"/>
        bool IRespondingService.ContainsErrorMessage(IDictionary<string, string[]> errorDetails, out string errorMessage)
        {
            // 1. Dictionary of all possible details (with potentially multiple errors each)
            if (errorDetails.Count > 0)
            {
                // 2. First details with errors
                KeyValuePair<string, string[]> firstErrorDetails = errorDetails.First();

                if (firstErrorDetails.Value.Length > 0)
                {
                    // 3. First error
                    errorMessage = firstErrorDetails.Value[0];

                    return true;
                }
            }

            errorMessage = Resources.Processing_ERROR_ExecutingContext_UnknownErrorDetails;

            return false;
        }
        #endregion

        #region IRespondingService<TResult, TDetails>
        /// <inheritdoc cref="IRespondingService{TResult,TDetails}.Get_Processing_Status_ActionResult"/>
        ObjectResult IRespondingService<ProcessingResult, string>.Get_Processing_Status_ActionResult(ProcessingResult result, string details)
        {
            return result switch
            {
                // HttpStatus Code: 202 Accepted
                ProcessingResult.Success => ObjectResultExtensions.AsResult_202(details),
                
                // HttpStatus Code: 400 BadRequest
                ProcessingResult.Failure => ((IRespondingService)this).Get_Exception_ActionResult(details),
                
                // HttpStatus Code: 501 Not Implemented
                _ => ObjectResultExtensions.AsResult_501()
            };
        }
        #endregion

        #region Helper methods
        private static string GetEmailErrorMessage(Capture match)
        {
            const string email = "Email: ";

            return $"{email}{match.Value}";
        }
        
        private static string GetPhoneErrorMessage(Capture match)
        {
            const string phone = "Phone: ";

            return $"{phone}{match.Value}";
        }
        
        private static string GetTemplateErrorMessage(Capture match)
        {
            const string template = "Template: Is ";  // "is" is not capitalized in the original error message

            return $"{template}{match.Value}";
        }
        #endregion
    }
}