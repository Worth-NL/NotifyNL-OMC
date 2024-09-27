// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.NotificatieApi;
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
                Notification = new NotificationEvent
                {
                    Action = Actions.Create,
                    Channel = Channels.Cases,
                    Resource = Resources.Status,
                    Attributes = new EventAttributes
                    {
                        // Cases
                        CaseTypeUri = DefaultValues.Models.EmptyUri,
                        SourceOrganization = DefaultValues.Models.DefaultOrganization,
                        ConfidentialityNotice = PrivacyNotices.NonConfidential,
                        // Objects
                        ObjectTypeUri = DefaultValues.Models.EmptyUri,
                        // Decisions
                        DecisionTypeUri = DefaultValues.Models.EmptyUri,
                        ResponsibleOrganization = DefaultValues.Models.DefaultOrganization
                    },
                    MainObjectUri = DefaultValues.Models.EmptyUri,
                    ResourceUri = DefaultValues.Models.EmptyUri,
                    CreateDate = DateTime.UtcNow
                },
                CaseUri = DefaultValues.Models.EmptyUri,
                PartyUri = DefaultValues.Models.EmptyUri
            };
        }
    }
}