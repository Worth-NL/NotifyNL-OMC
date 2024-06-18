// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataQuerying;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.IntegrationTests.Services.DataQuerying
{
    [TestFixture]
    public sealed class DataQueryServiceTests
    {
        private IDataQueryService<NotificationEvent>? _dataQuery;
        private NotificationEvent _notification;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            // Mocked IQueryContext
            IQueryContext queryContext = new Mock<IQueryContext>().Object;  // TODO: MockBehavior.Strict

            // Larger services
            this._dataQuery = new DataQueryService(queryContext);

            // Notification
            this._notification = NotificationEventHandler.GetNotification_Test_EmptyAttributes_WithOrphans_ManuallyCreated();
        }

        // TODO: Complete tests
    }
}