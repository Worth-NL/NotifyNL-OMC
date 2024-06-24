// © 2023, Worth Systems.

namespace EventsHandler.Exceptions
{
    /// <summary>
    /// The custom exception to report communication issues with external telemetry API service.
    /// </summary>
    /// <seealso cref="Exception"/>
    internal sealed class TelemetryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryException"/> class.
        /// </summary>
        public TelemetryException(string message)
            : base(message)
        {
        }
    }
}