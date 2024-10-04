// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of <see cref="NotificationEvent"/> to be used in Swagger UI.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage(Justification = "This is example model used by Swagger UI; testing how third-party dependency is dealing with it is unnecessary.")]
    internal sealed class NotifyReferenceExample : IExamplesProvider<NotifyReference>
    {
        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public NotifyReference GetExamples()
        {
            return new NotifyReference
            {
                Notification = new NotificationEventExample().GetExamples(),
                CaseId = Guid.Empty,
                PartyId = Guid.Empty
            };
        }
    }
}