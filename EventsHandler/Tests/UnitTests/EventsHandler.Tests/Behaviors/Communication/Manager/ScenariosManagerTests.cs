﻿// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Manager;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Configuration;
using EventsHandler.Services.DataLoading.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace EventsHandler.UnitTests.Behaviors.Communication.Manager
{
    [TestFixture]
    public sealed class ScenariosManagerTests
    {
        private Mock<INotifyScenario>? _mockedNotifyScenario;
        private Mock<IDataQueryService<NotificationEvent>>? _mockedDataQuery;

        private ServiceProvider? _serviceProvider;
        private IScenariosManager? _scenariosManager;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            var mockedConfiguration = new Mock<IConfiguration>(MockBehavior.Loose);
            var webApiConfiguration = new WebApiConfiguration(mockedConfiguration.Object, new Mock<ILoadingService>().Object);

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
            this._scenariosManager = new ScenariosManager(this._serviceProvider, this._mockedDataQuery.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            this._mockedDataQuery?.Reset();
        }

        [OneTimeTearDown]
        public void CleanupTests()
        {
            this._serviceProvider?.Dispose();
        }

        [Test]
        public async Task DetermineScenarioAsync_ForInvalidNotification_ReturnsNotImplementedScenario()
        {
            // Arrange
            var testNotification = new NotificationEvent();

            // Act
            INotifyScenario actualResult = await this._scenariosManager!.DetermineScenarioAsync(testNotification);

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
            //    .Returns(new Mock<ApiDataQuery.QueryContext>());

            // Act
            INotifyScenario actualResult = await this._scenariosManager!.DetermineScenarioAsync(testNotification);
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
