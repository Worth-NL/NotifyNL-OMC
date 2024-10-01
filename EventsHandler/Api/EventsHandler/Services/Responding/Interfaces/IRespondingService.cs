// © 2023, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventsHandler.Services.Responding.Interfaces
{
    /// <summary>
    /// The service producing human user-friendly <see cref="IActionResult"/> API responses.
    /// </summary>
    public interface IRespondingService
    {
        /// <summary>
        /// Gets the standardized <see cref="IActionResult"/> based on the received <see cref="Exception"/>.
        /// </summary>
        /// <param name="exception">The handled program exception.</param>
        internal ObjectResult GetExceptionResponse(Exception exception);

        /// <summary>
        /// Gets the standardized <see cref="IActionResult"/> based on the received error message.
        /// </summary>
        /// <param name="errorMessage">
        ///   The message (it can be a handled <see cref="Exception"/> message or intercepted validation error message).
        /// </param>
        internal ObjectResult GetExceptionResponse(string errorMessage);

        /// <summary>
        /// Gets the standardized <see cref="IActionResult"/> based on received error details (e.g., handled by <seealso cref="ActionFilterAttribute"/>).
        /// </summary>
        /// <param name="context">The intercepted result of API executing context.</param>
        /// <param name="errorDetails">The encountered error details to be processed.</param>
        internal ResultExecutingContext GetExceptionResponse(ResultExecutingContext context, IDictionary<string, string[]> errorDetails);

        /// <summary>
        /// Tries to get valid and meaningful error message from the collection of encountered errors.
        /// </summary>
        /// <param name="errorDetails">The error details to be reviewed.</param>
        /// <param name="errorMessage">The error message to be returned.</param>
        /// <returns>
        ///   <see langword="true"/> if an error message was found; otherwise, <see langword="false"/>.
        /// </returns>
        internal bool ContainsErrorMessage(IDictionary<string, string[]> errorDetails, out string errorMessage);
    }

    /// <summary>
    /// <inheritdoc cref="IRespondingService"/>
    /// <para>
    ///   Specialized in processing generic <typeparamref name="TResult"/> and <typeparamref name="TDetails"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The generic type of the processing result.</typeparam>
    /// <typeparam name="TDetails">The more insightful details about the processing outcome.</typeparam>
    /// <seealso cref="IRespondingService"/>
    public interface IRespondingService<in TResult, in TDetails> : IRespondingService
    {
        /// <summary>
        /// Gets standardized <see cref="IActionResult"/> based on the received generic <typeparamref name="TResult"/> and <typeparamref name="TDetails"/>.
        /// </summary>
        /// <param name="result">
        ///   <inheritdoc cref="IRespondingService{TResult, TDetails}" path="/typeparam[@name='TResult']"/>
        /// </param>
        /// <param name="details">
        ///   <inheritdoc cref="IRespondingService{TResult, TDetails}" path="/typeparam[@name='TDetails']"/>
        /// </param>
        internal ObjectResult GetResponse(TResult result, TDetails details);
    }

    /// <summary>
    /// <inheritdoc cref="IRespondingService{TResult, TDetails}"/>
    /// <para>
    ///   Dedicated to be used with <typeparamref name="TModel"/> objects.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <seealso cref="IRespondingService{TResult, TDetails}"/>
    public interface IRespondingService<TModel> : IRespondingService<(ProcessingResult, string), BaseEnhancedDetails>  // NOTE: This interface is implicitly following Adapter Design Pattern
        where TModel : IJsonSerializable
    {
        /// <inheritdoc cref="IRespondingService{TResult, TDetails}.GetResponse(TResult, TDetails)"/>
        internal new ObjectResult GetResponse((ProcessingResult Status, string Description) result, BaseEnhancedDetails details);
    }
}