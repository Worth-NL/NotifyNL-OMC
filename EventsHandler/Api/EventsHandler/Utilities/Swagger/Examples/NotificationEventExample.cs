// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Constants;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of <see cref="NotificationEvent"/> to be used in Swagger UI.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage]
    internal sealed class NotificationEventExample : IExamplesProvider<NotificationEvent>
    {
        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public NotificationEvent GetExamples()
        {
            return new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Cases,
                Resource = Resources.Status,
                Attributes = new EventAttributes
                {
                    // Cases
                    CaseType = DefaultValues.Models.EmptyUri,
                    SourceOrganization = DefaultValues.Models.DefaultOrganization,
                    ConfidentialityNotice = PrivacyNotices.NonConfidential,
                    // Objects
                    ObjectType = DefaultValues.Models.EmptyUri,
                    // Decisions
                    DecisionType = DefaultValues.Models.EmptyUri,
                    ResponsibleOrganization = DefaultValues.Models.DefaultOrganization
                },
                MainObject = DefaultValues.Models.EmptyUri,
                ResourceUrl = DefaultValues.Models.EmptyUri,
                CreateDate = DateTime.UtcNow
            };
        }
    }
}