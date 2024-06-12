// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Manager;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoqExt;

namespace EventsHandler.UnitTests.Behaviors.Communication.Manager
{
    [TestFixture]
    public sealed class ScenariosResolverTests
    {
        private Mock<INotifyScenario>? _mockedNotifyScenario;
        private Mock<IDataQueryService<NotificationEvent>>? _mockedDataQuery;

        private ServiceProvider? _serviceProvider;
        private IScenariosResolver? _scenariosResolver;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            WebApiConfiguration webApiConfiguration = new(new MockingContext());

            // Mocked services
            this._mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            this._mockedNotifyScenario?.Setup(mock => mock.GetAllNotifyDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(Array.Empty<NotifyData>());

            this._mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            var defaultScenario = new NotImplementedScenario(webApiConfiguration, this._mockedDataQuery.Object);

            // Service Provider (does not require mocking)
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(webApiConfiguration);
            serviceCollection.AddSingleton(defaultScenario);
            this._serviceProvider = serviceCollection.BuildServiceProvider();

            // Scenarios Manager
            this._scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            this._mockedDataQuery?.Reset();
        }

        [OneTimeTearDown]
        public void CleanupTests()
        {
            // Dispose service provider with registered services
            this._serviceProvider?.Dispose();
        }

        [Test]
        public async Task DetermineScenarioAsync_ForInvalidNotification_ReturnsNotImplementedScenario()
        {
            // Arrange
            var testNotification = new NotificationEvent();

            // Act
            INotifyScenario actualResult = await this._scenariosResolver!.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<NotImplementedScenario>());
        }

        [Test, Ignore("QueryContext class is not yet ready for mocking => tech debt")]
        public async Task DetermineScenarioAsync_ForCaseCreatedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetTestNotificationEvent();

            // TODO: Finish unit testing by introducing IQueryContext interface first
            //this._mockedDataQuery?.Setup(mock => mock.From(testNotification))
            //    .Returns(new Mock<DataQueryService.QueryContext>());

            // Act
            INotifyScenario actualResult = await this._scenariosResolver!.DetermineScenarioAsync(testNotification);
        }

        #region Helper methods
        private static NotificationEvent GetTestNotificationEvent()
        {
            return new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Cases,
                Resource = Resources.Status
            };
        }
        #endregion
    }
}