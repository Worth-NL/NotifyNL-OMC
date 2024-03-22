// © 2024, Worth Systems.

using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of personalization map for "NotifyNL" message template to be used in Swagger UI.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage]
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