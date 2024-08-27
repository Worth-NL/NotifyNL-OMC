﻿// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Manager;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventsHandler.UnitTests.Services.DataProcessing.Strategy.Manager
{
    [TestFixture]
    public sealed class ScenariosResolverTests
    {
        private Mock<INotifyScenario> _mockedNotifyScenario = null!;
        private Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = null!;
        private Mock<INotifyService<NotificationEvent, NotifyData>> _mockedNotifyService = null!;

        private WebApiConfiguration _webApiConfiguration = null!;
        private ServiceProvider _serviceProvider = null!;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            // Mocked services
            this._mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            this._mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Failure());

            this._mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            this._mockedNotifyService = new Mock<INotifyService<NotificationEvent, NotifyData>>(MockBehavior.Strict);

            // Service Provider (does not require mocking)
            var serviceCollection = new ServiceCollection();

            this._webApiConfiguration = ConfigurationHandler.GetValidBothConfigurations();

            serviceCollection.AddSingleton(this._webApiConfiguration);
            serviceCollection.AddSingleton(new CaseCreatedScenario(this._webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new CaseStatusUpdatedScenario(this._webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new CaseClosedScenario(this._webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new TaskAssignedScenario(this._webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new DecisionMadeScenario(this._webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new MessageReceivedScenario(this._webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new NotImplementedScenario(this._webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));

            this._serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [SetUp]
        public void ResetTests()
        {
            // NOTE: This mock is object of tests setup (arrange)
            this._mockedDataQuery.Reset();
        }

        [OneTimeTearDown]
        public void CleanupTests()
        {
            this._webApiConfiguration.Dispose();
            this._serviceProvider.Dispose();
        }

        #region DetermineScenarioAsync()
        [Test]
        public async Task DetermineScenarioAsync_InvalidNotification_ReturnsNotImplementedScenario()
        {
            // Arrange
            IScenariosResolver scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(default);

            // Assert
            Assert.That(actualResult, Is.TypeOf<NotImplementedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_CaseCreatedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetCaseNotification();

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses { Count = 1 });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);
            
            IScenariosResolver scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseCreatedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_CaseCaseStatusUpdatedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetCaseNotification();

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses { Count = 2 });
            mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses>()))
                .ReturnsAsync(new CaseType { IsFinalStatus = false });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);
            
            IScenariosResolver scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseStatusUpdatedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_CaseCaseFinishedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetCaseNotification();

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses { Count = 2 });
            mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses>()))
                .ReturnsAsync(new CaseType { IsFinalStatus = true });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);
            
            IScenariosResolver scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseClosedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_TaskAssignedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetObjectNotification(ConfigurationHandler.TestTaskObjectTypeUuid);
            IScenariosResolver scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<TaskAssignedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_MessageReceivedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetObjectNotification(ConfigurationHandler.TestMessageObjectTypeUuid);
            IScenariosResolver scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<MessageReceivedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_DecisionMadeScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetDecisionNotification();
            IScenariosResolver scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<DecisionMadeScenario>());
        }
        #endregion

        #region Helper methods
        private static NotificationEvent GetCaseNotification()
        {
            return new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Cases,
                Resource = Resources.Status
            };
        }

        private static NotificationEvent GetObjectNotification(string testGuid)
        {
            return new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Objects,
                Resource = Resources.Object,
                Attributes = new EventAttributes
                {
                    ObjectTypeUri = new Uri($"https://objecttypen.test.denhaag.opengem.nl/api/v2/objecttypes/{testGuid}")
                }
            };
        }

        private static NotificationEvent GetDecisionNotification()
        {
            return new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Decisions,
                Resource = Resources.Decision
            };
        }

        private ScenariosResolver GetScenariosResolver()
        {
            return new ScenariosResolver(this._webApiConfiguration, this._serviceProvider, this._mockedDataQuery.Object);
        }
        #endregion
    }
}