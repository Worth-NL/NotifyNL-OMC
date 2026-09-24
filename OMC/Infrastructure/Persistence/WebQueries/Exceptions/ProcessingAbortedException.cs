// © 2026, Worth Systems.

namespace WebQueries.Exceptions
{
    /// <summary>
    /// The flow deliberately stopped before sending anything, and the notification must not be redelivered.
    /// </summary>
    /// <remarks>
    ///   The counterpart of <c>EventsHandler.Exceptions.AbortedNotifyingException</c> for scenarios whose
    ///   work lives in this project, which cannot reference the Web API layer. <c>NotifyProcessor</c> maps
    ///   both onto the same aborted result, so both answer 206.
    ///   <para>
    ///     Use it for a decision, not for a defect: the resource is gone, a whitelist says no, a flag says
    ///     no. A service being unreachable is not this - that has to stay a failure so the sender retries.
    ///   </para>
    /// </remarks>
    public sealed class ProcessingAbortedException : Exception
    {
        /// <inheritdoc cref="ProcessingAbortedException"/>
        public ProcessingAbortedException(string message) : base(message)
        {
        }

        /// <inheritdoc cref="ProcessingAbortedException"/>
        public ProcessingAbortedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
