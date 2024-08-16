// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
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
            this._mockedNotifyService = new Mock<INotifyService<NotificationEvent, NotifyData>>(MockBehavior.Strict);

            // Service Provider (does not require mocking)
            var serviceCollection = new ServiceCollection();

            WebApiConfiguration webApiConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();

            serviceCollection.AddSingleton(webApiConfiguration);
            serviceCollection.AddSingleton(new CaseCreatedScenario(webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new CaseStatusUpdatedScenario(webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new CaseClosedScenario(webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new TaskAssignedScenario(webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new DecisionMadeScenario(webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));
            serviceCollection.AddSingleton(new NotImplementedScenario(webApiConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object));

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
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
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
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses { Count = 2 });
            mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses>()))
                .ReturnsAsync(new CaseType { IsFinalStatus = false });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);

            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseStatusUpdatedScenario>());
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
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses { Count = 2 });
            mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses>()))
                .ReturnsAsync(new CaseType { IsFinalStatus = true });

            this._mockedDataQuery
                .Setup(mock => mock.From(testNotification))
                .Returns(mockedQueryContext.Object);

            IScenariosResolver scenariosResolver = new ScenariosResolver(this._serviceProvider, this._mockedDataQuery.Object);

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<CaseClosedScenario>());
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

        [Test]
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