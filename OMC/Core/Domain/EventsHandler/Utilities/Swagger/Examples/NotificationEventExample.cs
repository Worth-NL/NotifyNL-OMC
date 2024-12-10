// © 2023, Worth Systems.

using Common.Constants;
using EventsHandler.Constants;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using ZhvModels.Constants;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of <see cref="NotificationEvent"/> to be used in Swagger UI.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage(Justification = "This is example model used by Swagger UI; testing how third-party dependency is dealing with it is unnecessary.")]
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
                    CaseTypeUri = CommonValues.Default.Models.EmptyUri,
                    SourceOrganization = ZhvValues.Default.Models.DefaultOrganization,
                    ConfidentialityNotice = PrivacyNotices.NonConfidential,
                    // Objects
                    ObjectTypeUri = CommonValues.Default.Models.EmptyUri,
                    // Decisions
                    DecisionTypeUri = CommonValues.Default.Models.EmptyUri,
                    ResponsibleOrganization = ZhvValues.Default.Models.DefaultOrganization
                },
                MainObjectUri = CommonValues.Default.Models.EmptyUri,
                ResourceUri = CommonValues.Default.Models.EmptyUri,
                CreateDate = DateTime.UtcNow
            };
        }
    }
}