// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Implementations;
using EventsHandler.Behaviors.Communication.Strategy.Implementations.Cases;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Manager;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventsHandler.UnitTests.Behaviors.Communication.Manager
{
    [TestFixture]
    public sealed class ScenariosResolverTests
    {
        private Mock<INotifyScenario> _mockedNotifyScenario = null!;
        private Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = null!;

        private ServiceProvider _serviceProvider = null!;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            // Mocked services
            this._mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            this._mockedNotifyScenario
                .Setup(mock => mock.GetAllNotifyDataAsync(It.IsAny<NotificationEvent>()))
                .ReturnsAsync(Array.Empty<NotifyData>());

            this._mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);

            // Service Provider (does not require mocking)
            var serviceCollection = new ServiceCollection();

            WebApiConfiguration webApiConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();

            serviceCollection.AddSingleton(webApiConfiguration);
            serviceCollection.AddSingleton(new CaseCreatedScenario(webApiConfiguration, this._mockedDataQuery.Object));
            serviceCollection.AddSingleton(new CaseCaseStatusUpdatedScenario(webApiConfiguration, this._mockedDataQuery.Object));
            serviceCollection.AddSingleton(new CaseCaseFinishedScenario(webApiConfiguration, this._mockedDataQuery.Object));
            serviceCollection.AddSingleton(new TaskAssignedScenario(webApiConfiguration, this._mockedDataQuery.Object));
            serviceCollection.AddSingleton(new DecisionMadeScenario(webApiConfiguration, this._mockedDataQuery.Object));
            serviceCollection.AddSingleton(new NotImplementedScenario(webApiConfiguration, this._mockedDataQuery.Object));

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
            this._serviceProvider.Dispose();
        }

        #region DetermineScenarioAsync()
        [Test]
        public async Task DetermineScenarioAsync_ForInvalidNotification_ReturnsNotImplementedScenario()
        {
            // Arrange
            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(default);

            // Assert
            Assert.That(actualResult, Is.TypeOf<NotImplementedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_ForCaseCreatedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            var testNotification = new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Cases,
                Resource = Resources.Status
            };

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync())
                .ReturnsAsync(new CaseStatuses { Count = 1 });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);

            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseCreatedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_ForCaseCaseStatusUpdatedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            var testNotification = new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Cases,
                Resource = Resources.Status
            };

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync())
                .ReturnsAsync(new CaseStatuses { Count = 2 });
            mockedQueryContext
                .Setup(mock => mock.GetLastCaseStatusTypeAsync(It.IsAny<CaseStatuses>()))
                .ReturnsAsync(new CaseStatusType { IsFinalStatus = false });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);

            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseCaseStatusUpdatedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_ForCaseCaseFinishedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            var testNotification = new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Cases,
                Resource = Resources.Status
            };

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync())
                .ReturnsAsync(new CaseStatuses { Count = 2 });
            mockedQueryContext
                .Setup(mock => mock.GetLastCaseStatusTypeAsync(It.IsAny<CaseStatuses>()))
                .ReturnsAsync(new CaseStatusType { IsFinalStatus = true });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);

            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseCaseFinishedScenario>());
        }

        [Test]
        public async Task DetermineScenarioAsync_ForTaskAssignedScenario_ReturnsExpectedScenario()
        {
            // Arrange
            var testNotification = new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Objects,
                Resource = Resources.Object
            };

            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<TaskAssignedScenario>());
        }

        [Test, Ignore("The decision is disabled currently")]  // TODO: Enable this test
        public async Task DetermineScenarioAsync_ForDecisionMadeScenario_ReturnsExpectedScenario()
        {
            // Arrange
            var testNotification = new NotificationEvent
            {
                Action = Actions.Create,
                Channel = Channels.Decisions,
                Resource = Resources.Decision
            };

            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<DecisionMadeScenario>());
        }
        #endregion
    }
}