// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventsHandler.Services.UserCommunication.Interfaces
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
        internal ObjectResult Get_Exception_ActionResult(Exception exception);

        /// <summary>
        /// Gets the standardized <see cref="IActionResult"/> based on the received error message.
        /// </summary>
        /// <param name="errorMessage">
        ///   The message (it can be a handled <see cref="Exception"/> message or intercepted validation error message).
        /// </param>
        internal ObjectResult Get_Exception_ActionResult(string errorMessage);

        /// <summary>
        /// Gets the standardized <see cref="IActionResult"/> based on received error details (e.g., handled by <seealso cref="ActionFilterAttribute"/>).
        /// </summary>
        /// <param name="context">The intercepted result of API executing context.</param>
        /// <param name="errorDetails">The encountered error details to be processed.</param>
        internal ResultExecutingContext Get_Exception_ActionResult(ResultExecutingContext context, IDictionary<string, string[]> errorDetails);

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
        internal ObjectResult Get_Processing_Status_ActionResult(TResult result, TDetails details);

        /// <summary>
        /// Gets standardized failure <see cref="IActionResult"/> based on the received <typeparamref name="TDetails"/>.
        /// </summary>
        /// <param name="details">
        ///   <inheritdoc cref="Get_Processing_Status_ActionResult"
        ///               path="/param[@name='details']"/>
        /// </param>
        internal ObjectResult Get_Processing_Failed_ActionResult(TDetails details);
    }

    /// <summary>
    /// <inheritdoc cref="IRespondingService{TResult, TDetails}"/>
    /// <para>
    ///   Dedicated to be used with <typeparamref name="TModel"/> objects.
    /// </para>
    /// </summary>
    /// <seealso cref="IRespondingService{TResult, TDetails}"/>
    public interface IRespondingService<TModel> : IRespondingService<(ProcessingResult, string), BaseEnhancedDetails>  // NOTE: This interface is implicitly following Adapter Design Pattern
        where TModel : IJsonSerializable
    {
        /// <inheritdoc cref="IRespondingService{TResult,TDetails}.Get_Processing_Status_ActionResult"/>
        internal new ObjectResult Get_Processing_Status_ActionResult((ProcessingResult Status, string Description) result, BaseEnhancedDetails details);

        /// <inheritdoc cref="IRespondingService{TResult,TDetails}.Get_Processing_Failed_ActionResult"/>
        internal new ObjectResult Get_Processing_Failed_ActionResult(BaseEnhancedDetails details);
    }
}