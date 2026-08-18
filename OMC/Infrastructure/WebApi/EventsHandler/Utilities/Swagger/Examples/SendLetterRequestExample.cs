// © 2024, Worth Systems.

using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using EventsHandler.Controllers;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of <see cref="SendLetterRequest"/> for Swagger UI.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage(Justification = "This is an example model used by Swagger UI; testing how third-party dependency is dealing with it is unnecessary.")]
    internal sealed class SendLetterRequestExample : IExamplesProvider<SendLetterRequest>
    {
        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public SendLetterRequest GetExamples()
        {
            return new SendLetterRequest
            {
                Personalization = new Dictionary<string, object>
                {
                    { "placeholder1", "value1" },
                    { "placeholder2", "value2" }
                },
                Reference = "your-client-reference-123"
            };
        }
    }
}