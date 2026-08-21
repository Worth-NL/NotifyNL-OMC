// © 2024, Worth Systems.

using Common.Enums.Responses;
using Common.Extensions;
using Common.Models.Messages.Errors;
using Common.Models.Messages.Errors.Specific;
using Common.Models.Messages.Successes;
using Common.Models.Responses;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Extensions;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Notify.Exceptions;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.MOBB.Models;
using WebQueries.Print.Models;
using ZgwModels.Enums;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.NotifyNL;
using ZgwModels.Serialization.Interfaces;

namespace EventsHandler.Services.Responding
{
    /// <inheritdoc cref="IRespondingService{TResult}"/>
    public abstract partial class GeneralResponder : IRespondingService<ProcessingResult>  // NOTE: "partial" is introduced by the new RegEx generation approach
    {
        /// <inheritdoc cref="ISerializationService"/>
        protected ISerializationService Serializer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralResponder"/> class.
        /// </summary>
        /// <param name="serializer">The input de(serializing) service.</param>
        protected GeneralResponder(ISerializationService serializer)
        {
            this.Serializer = serializer;
        }

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
        [GeneratedRegex("not a valid UUID", RegexOptions.Compiled)]  // NOTE: "is" is not capitalized; skip it
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
        /// <inheritdoc cref="IRespondingService.GetExceptionResponse(Exception)"/>
        ObjectResult IRespondingService.GetExceptionResponse(Exception exception)
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
                        return new ProcessingFailed.Simplified(HttpStatusCode.Forbidden, ProcessingResult.Failure(message)).AsResult_403();
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

                    return ((IRespondingService)this).GetExceptionResponse(message);
                }

                // NOTE: Authorization issues wrapped around 403 Forbidden status code
                case NotifyAuthException:
                    return new Forbidden.Simplified(ProcessingResult.Failure(exception.Message)).AsResult_403();

                // NOTE: Unexpected issues wrapped around 500 Internal Server Error status code
                default:
                    return new InternalError.Simplified(ProcessingResult.Failure(exception.InnerException?.Message ?? exception.Message)).AsResult_500();
            }
        }

        /// <inheritdoc cref="IRespondingService.GetExceptionResponse(string)"/>
        ObjectResult IRespondingService.GetExceptionResponse(string errorMessage)
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
            return new BadRequest.Simplified(ProcessingResult.Failure(message ?? errorMessage)).AsResult_400();
        }

        /// <inheritdoc cref="IRespondingService.GetExceptionResponse(ResultExecutingContext, IDictionary{string, string[]})"/>
        ResultExecutingContext IRespondingService.GetExceptionResponse(ResultExecutingContext context, IDictionary<string, string[]> errorDetails)
        {
            if (((IRespondingService)this).ContainsErrorMessage(errorDetails, out string errorMessage))
            {
                // HttpStatus Code: 400 BadRequest
                context.Result = ((IRespondingService)this).GetExceptionResponse(errorMessage);
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

            errorMessage = ApiResources.Processing_ERROR_ExecutingContext_UnknownErrorDetails;

            return false;
        }
        #endregion

        #region IRespondingService<TResult>
        /// <inheritdoc cref="IRespondingService{TResult}.GetResponse(TResult)"/>
        ObjectResult IRespondingService<ProcessingResult>.GetResponse(ProcessingResult result)
        {
            return result.Status switch
            {
                // HttpStatus Code: 202 Accepted
                ProcessingStatus.Success => new ProcessingSucceeded(result).AsResult_202(),

                // HttpStatus Code: 400 BadRequest
                ProcessingStatus.Failure => ((IRespondingService)this).GetExceptionResponse(result.Description),

                // HttpStatus Code: 501 Not Implemented
                _ => new NotImplemented().AsResult_501()
            };
        }
        #endregion

        #region Abstract        
        /// <summary>
        /// Handles the callbacks from "Notify NL" Web API service (accordingly to the used "OMC workflow" version).
        /// </summary>
        internal abstract Task<IActionResult> HandleNotifyCallbackAsync(object json);
        #endregion

        #region Parent
        /// <summary>
        /// Extracts the notification data from received <see cref="DeliveryReceipt"/> callback.
        /// </summary>
        /// <param name="callback">The callback to be analyzed.</param>
        /// <returns>
        ///   The notification data required for further processing.
        /// </returns>
        internal async Task<(NotifyReference, NotifyMethods)> ExtractCallbackDataAsync(DeliveryReceipt callback)
        {
            // Decompress and decode the string
            string decodedReference = await (callback.Reference ?? string.Empty).DecompressGZipAsync(CancellationToken.None);

            // Deserialize into object
            NotifyReference notification = this.Serializer.Deserialize<NotifyReference>(decodedReference);

            // Convert enums
            NotifyMethods notificationMethod = callback.Type.ConvertToNotifyMethod();

            return (notification, notificationMethod);
        }

        /// <summary>
        /// Determines whether the given callback's reference was built for the MOBB / Berichtenbox
        /// scenario (<see cref="MessageBoxNotifyReference"/>) rather than the standard <see cref="NotifyReference"/>.
        /// </summary>
        /// <remarks>
        ///   First-version draft: distinguishes the two by peeking at the decoded reference for a
        ///   "MessageId" property, which only <see cref="MessageBoxNotifyReference"/> has. This means the
        ///   reference gets decompressed/parsed twice for a MOBB callback (once here, once by whichever
        ///   Extract...CallbackDataAsync is subsequently called) - acceptable for a first version, but
        ///   worth collapsing into a single decode if this needs tightening up later.
        /// </remarks>
        internal async Task<bool> IsMessageBoxCallbackAsync(DeliveryReceipt callback)
        {
            string decodedReference = await (callback.Reference ?? string.Empty).DecompressGZipAsync(CancellationToken.None);

            if (decodedReference.IsNullOrEmpty())
            {
                return false;
            }

            using JsonDocument document = JsonDocument.Parse(decodedReference);

            return document.RootElement.TryGetProperty(nameof(MessageBoxNotifyReference.MessageId), out _);
        }

        /// <summary>
        /// Determines whether the given callback's reference was built for the print (printstraat)
        /// scenario (<see cref="PrintNotifyReference"/>) rather than the standard <see cref="NotifyReference"/>.
        /// </summary>
        /// <remarks>
        ///   Same raw-JSON peek as <see cref="IsMessageBoxCallbackAsync"/>, on the "ObjectId" property that
        ///   only <see cref="PrintNotifyReference"/> has.
        /// </remarks>
        internal async Task<bool> IsPrintCallbackAsync(DeliveryReceipt callback)
        {
            string decodedReference = await (callback.Reference ?? string.Empty).DecompressGZipAsync(CancellationToken.None);

            if (decodedReference.IsNullOrEmpty())
            {
                return false;
            }

            using JsonDocument document = JsonDocument.Parse(decodedReference);

            return document.RootElement.TryGetProperty(nameof(PrintNotifyReference.ObjectId), out _);
        }

        /// <summary>
        /// Extracts the notification data from received <see cref="DeliveryReceipt"/> callback, for the
        /// print (printstraat) scenario reference shape.
        /// </summary>
        /// <param name="callback">The callback to be analyzed.</param>
        /// <returns>
        ///   The notification data required for further processing.
        /// </returns>
        internal async Task<(PrintNotifyReference, NotifyMethods)> ExtractPrintCallbackDataAsync(DeliveryReceipt callback)
        {
            string decodedReference = await (callback.Reference ?? string.Empty).DecompressGZipAsync(CancellationToken.None);

            PrintNotifyReference reference = this.Serializer.Deserialize<PrintNotifyReference>(decodedReference);

            NotifyMethods notificationMethod = callback.Type.ConvertToNotifyMethod();

            return (reference, notificationMethod);
        }

        /// <summary>
        /// Extracts the notification data from received <see cref="DeliveryReceipt"/> callback, for the
        /// MOBB / Berichtenbox scenario reference shape.
        /// </summary>
        /// <param name="callback">The callback to be analyzed.</param>
        /// <returns>
        ///   The notification data required for further processing.
        /// </returns>
        internal async Task<(MessageBoxNotifyReference, NotifyMethods)> ExtractMessageBoxCallbackDataAsync(DeliveryReceipt callback)
        {
            string decodedReference = await (callback.Reference ?? string.Empty).DecompressGZipAsync(CancellationToken.None);

            MessageBoxNotifyReference reference = this.Serializer.Deserialize<MessageBoxNotifyReference>(decodedReference);

            NotifyMethods notificationMethod = callback.Type.ConvertToNotifyMethod();

            return (reference, notificationMethod);
        }

        /// <summary>
        /// Gets the text confirming successful delivery receipt.
        /// </summary>
        /// <param name="callback">The callback to be parsed.</param>
        internal static string GetDeliveryStatusLogMessage(DeliveryReceipt callback)
            => string.Format(ApiResources.Endpoint_Notify_Confirm_INFO_NotificationStatus, callback.Id, callback.Status);

        /// <summary>
        /// Gets the text confirming failed delivery receipt.
        /// </summary>
        /// <param name="callback">The callback to be parsed.</param>
        /// <param name="exception">The exception that occurred during processing the callback.</param>
        internal static string GetDeliveryErrorLogMessage(DeliveryReceipt callback, Exception exception)
            => string.Format(ApiResources.Endpoint_Notify_Confirm_ERROR_UnexpectedFailure, callback.Id, exception.Message);
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