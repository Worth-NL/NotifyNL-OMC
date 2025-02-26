﻿// © 2024, Worth Systems.

using EventsHandler.Properties;

namespace EventsHandler.Exceptions
{
    /// <summary>
    /// The custom exception used to abort further processing of the notification.
    /// </summary>
    /// <seealso cref="Exception"/>
    internal sealed class AbortedNotifyingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortedNotifyingException"/> class.
        /// </summary>
        internal AbortedNotifyingException(string message)
            : base($"{message} {ApiResources.Processing_ABORT}")
        {
        }
    }
}