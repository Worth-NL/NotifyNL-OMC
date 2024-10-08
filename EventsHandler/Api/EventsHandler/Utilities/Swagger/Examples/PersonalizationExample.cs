﻿// © 2024, Worth Systems.

using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of personalization map for "Notify NL" message template to be used in Swagger UI.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage(Justification = "This is example model used by Swagger UI; testing how third-party dependency is dealing with it is unnecessary.")]
    internal sealed class PersonalizationExample : IExamplesProvider<Dictionary<string, object>>
    {
        internal const string Key = "key";
        internal const string Value = "value";

        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public Dictionary<string, object> GetExamples()
        {
            return new Dictionary<string, object>
            {
                { Key, Value }
            };
        }
    }
}