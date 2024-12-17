// © 2023, Worth Systems.

using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

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