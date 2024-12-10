// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Manager;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Tests.Unit.Services.DataProcessing.Strategy.Manager
{
    [TestFixture]
    public sealed class NotifyScenariosResolverTests
    {
        private Mock<INotifyScenario> _mockedNotifyScenario = null!;
        private Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = null!;
        private Mock<INotifyService<NotifyData>> _mockedNotifyService = null!;

        private WebApiConfiguration _webApiConfiguration = null!;
        private ServiceProvider _serviceProvider = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            // Mocked services
            this._mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            this._mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(It.IsAny<NotificationEvent>()))
                .ReturnsAsync(QueryingDataResponse.Failure());

            this._mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            this._mockedNotifyService = new Mock<INotifyService<NotifyData>>(MockBehavior.Strict);

            // Service Provider (does not require mocking)
            var serviceCollection = new ServiceCollection();

            this._webApiConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.BothValid_v1);

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
        public void TestsReset()
        {
            // NOTE: This mock is object of tests setup (arrange)
            this._mockedDataQuery.Reset();
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._webApiConfiguration.Dispose();
            this._serviceProvider.Dispose();
        }

        #region DetermineScenarioAsync()
        [Test]
        public async Task DetermineScenarioAsync_InvalidNotification_ReturnsNotImplementedScenario()
        {
            // Arrange
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

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
            
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

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
            
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

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
            
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

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
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

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
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

            // Act
            INotifyScenario actualResult = await scenariosResolver.DetermineScenarioAsync(testNotification);

            // Assert
            Assert.That(actualResult, Is.TypeOf<MessageReceivedScenario>());
        }

        [Test]
        public void DetermineScenarioAsync_MessageReceivedScenario_ThrowsAbortedNotifyingException()
        {
            // Arrange
            NotificationEvent testNotification = GetObjectNotification(Guid.Empty.ToString());
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception = Assert.ThrowsAsync<AbortedNotifyingException>(() => scenariosResolver.DetermineScenarioAsync(testNotification));
                Assert.That(exception?.Message.StartsWith(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_GenObjectTypeGuid
                                              .Replace("{0}", $"{testNotification.Attributes.ObjectTypeUri.GetGuid()}")
                                              .Replace("{1}", "ZGW_VARIABLE_OBJECTTYPE_...OBJECTTYPE_UUID")), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);
            });
        }

        [Test]
        public async Task DetermineScenarioAsync_DecisionMadeScenario_ReturnsExpectedScenario()
        {
            // Arrange
            NotificationEvent testNotification = GetDecisionNotification();
            IScenariosResolver<INotifyScenario, NotificationEvent> scenariosResolver = GetScenariosResolver();

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

        private NotifyScenariosResolver GetScenariosResolver()
        {
            return new NotifyScenariosResolver(this._webApiConfiguration, this._serviceProvider, this._mockedDataQuery.Object);
        }
        #endregion
    }
}