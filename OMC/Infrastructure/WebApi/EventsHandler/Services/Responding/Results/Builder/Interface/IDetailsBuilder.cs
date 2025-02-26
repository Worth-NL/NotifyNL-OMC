﻿// © 2023, Worth Systems.

using Common.Models.Messages.Details.Base;
using EventsHandler.Services.Responding.Enums;

namespace EventsHandler.Services.Responding.Results.Builder.Interface
{
    /// <summary>
    /// The builder to create a specific details (enhanced or simple).
    /// </summary>
    public interface IDetailsBuilder
    {
        /// <summary>
        /// Gets the specific type of <see cref="BaseEnhancedDetails"/> with proper content.
        /// </summary>
        /// <typeparam name="TDetails">The type of the details.</typeparam>
        /// <param name="reason">
        ///   <inheritdoc cref="Reasons" path="/summary"/>
        /// </param>
        /// <param name="cases">The specific case of rule(s) violation to be included in details.</param>
        internal TDetails Get<TDetails>(Reasons reason, string cases)
            where TDetails : BaseEnhancedDetails;

        /// <summary>
        /// Gets the specific type of <see cref="BaseSimpleDetails"/> with proper content.
        /// </summary>
        /// <typeparam name="TDetails">The type of the details.</typeparam>
        /// <param name="reason">
        ///   <inheritdoc cref="Reasons" path="/summary"/>
        /// </param>
        internal TDetails Get<TDetails>(Reasons reason)
            where TDetails : BaseSimpleDetails;
    }
}