// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataReceiving;
using EventsHandler.Services.DataReceiving.Factories;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataReceiving.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using MoqExt;
using SecretsManager.Services.Authentication.Encryptions.Strategy;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;

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

            // Mocked IHttpSupplier
            WebApiConfiguration configuration = new(new MockingContext());
            EncryptionContext encryptionContext = new(new SymmetricEncryptionStrategy());
            IHttpClientFactory<HttpClient, (string, string)[]> httpClientFactory = new RegularHttpClientFactory(configuration);
            IHttpNetworkService network = new HttpNetworkService(configuration, encryptionContext, httpClientFactory);

            // Larger services
            this._dataQuery = new DataQueryService(queryContext, network);

            // Notification
            this._notification = NotificationEventHandler.GetNotification_Test_WithOrphans_ManuallyCreated();
        }

        // TODO: Complete tests
    }
}