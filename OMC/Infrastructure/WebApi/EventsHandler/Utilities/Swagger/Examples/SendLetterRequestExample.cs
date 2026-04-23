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
                Extras = new Dictionary<string, object>
                {
                    { "aanhefAan", new[] { "heffer" } },
                    { "aantalBijlagen", 0 },
                    { "afdeling", new[] { "afdeling" } },
                    { "classificatie", "VERTROUWELIJK" },
                    { "contactpersoon", new[] { "contactpersoon" } },
                    { "datum", "01-januari-2025" },
                    { "dienstCode", 123 },
                    { "dienstNaam", new[] { "dienstNaam" } },
                    { "emailadres", "my@big.com" },
                    { "geadresseerde", "persoonbos" },
                    { "locatie", new[] { "locatie" } },
                    { "ondertekening", new[] { "Pier-Angelo Gaetani" } },
                    { "onderwerp", new[] { "its an onderwerp" } },
                    { "onsKenmerk", "itsakenmerk" },               // Capitalized to match typical C# naming (but dictionary keys are case‑sensitive)
                    { "secundaireAfzender", new[] { "secundaireAfzender", "asdasd", "asdasd" } },
                    { "telefoonnummer", "telefoonnummer" },
                    { "uwBriefVan", "01-januari-2025" },
                    { "uwKenmerk", "uwKenmerk" },
                    { "retourAdres", "WEIGELIAPARK 14 2724RK ZOETERMEER" }
                },
                Reference = "your-client-reference-123"
            };
        }
    }
}