// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using Moq;

namespace EventsHandler.UnitTests.Services.DataProcessing.Strategy.Base
{
    [TestFixture]
    public sealed class BaseStrategyTests
    {
        private const string TestEmailAddress = "test@gmail.com";
        private const string TestPhoneNumber = "+310123456789";

        private WebApiConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetValidEnvironmentConfiguration();
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._testConfiguration.Dispose();
        }

        #region GetAllNotifyDataAsync()
        [Test]
        public void GetAllNotifyDataAsync_ForNullCachedCommonPartyData_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Loose);  // NOTE: GetPartyDataAsync() is not mocked => returning null
            var mockedQueryService = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            mockedQueryService
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(mockedQueryContext.Object);

            INotifyScenario scenario = new CaseCreatedScenario(this._testConfiguration, mockedQueryService.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                InvalidOperationException? exception =
                    Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message, Is.EqualTo(Resources.HttpRequest_ERROR_NoPartyData));

                VerifyMethodCalls(mockedQueryContext, mockedQueryService,
                    fromInvokeCount: 1, partyInvokeCount: 1, caseInvokeCount: 0);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email, 1, NotifyMethods.Email, "test@gmail.com", 1, 1, 1)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms, 1, NotifyMethods.Sms, "+310123456789", 1, 1, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email, 1, NotifyMethods.Email, "test@gmail.com", 1, 1, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms, 1, NotifyMethods.Sms, "+310123456789", 1, 1, 1)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email, 1, NotifyMethods.Email, "test@gmail.com", 1, 1, 1)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms, 1, NotifyMethods.Sms, "+310123456789", 1, 1, 1)]
        public async Task GetAllNotifyDataAsync_ForValidNotification_WithSingleNotifyMethod_ReturnsExpectedData(
            Type scenarioType, DistributionChannels testDistributionChannel, int expectedResultsCount,
            NotifyMethods expectedNotificationMethod, string expectedContactDetails,
            int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(
                testDistributionChannel, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedQueryService.Object)!;

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(expectedResultsCount));

                NotifyData firstResult = actualResult[0];
                Assert.That(firstResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod));
                Assert.That(firstResult.ContactDetails, Is.EqualTo(expectedContactDetails));
                Assert.That(firstResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, firstResult.NotificationMethod, this._testConfiguration)));

                VerifyMethodCalls(mockedQueryContext, mockedQueryService,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email, 1, 1, 1)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms, 1, 1, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email, 1, 1, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms, 1, 1, 1)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email, 1, 1, 1)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms, 1, 1, 1)]
        public void GetAllNotifyDataAsync_WithoutCaseIdWhitelisted_ThrowsAbortedNotifyingException(
            Type scenarioType, DistributionChannels testDistributionChannel, int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(
                testDistributionChannel, isCaseIdWhitelisted: false, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedQueryService.Object)!;

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));

                string expectedErrorMessage = Resources.Processing_ABORT_DoNotSendNotification_CaseIdWhitelisted
                    .Replace("{0}", "4")
                    // Get substring
                    [..(Resources.Processing_ABORT_DoNotSendNotification_CaseIdWhitelisted.Length -
                        Resources.Processing_ABORT_DoNotSendNotification_CaseIdWhitelisted.IndexOf("{1}", StringComparison.Ordinal))];

                Assert.That(exception?.Message.StartsWith(expectedErrorMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyMethodCalls(mockedQueryContext, mockedQueryService,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email, 1, 1, 1)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms, 1, 1, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email, 1, 1, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms, 1, 1, 1)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email, 1, 1, 1)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms, 1, 1, 1)]
        public void GetAllNotifyDataAsync_WithInformSetToFalse_ThrowsAbortedNotifyingException(
            Type scenarioType, DistributionChannels testDistributionChannel, int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(
                testDistributionChannel, isCaseIdWhitelisted: true, isNotificationExpected: false);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedQueryService.Object)!;

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_Informeren), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyMethodCalls(mockedQueryContext, mockedQueryService,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), 1, 1, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), 1, 1, 1)]
        [TestCase(typeof(CaseClosedScenario), 1, 1, 1)]
        public async Task GetAllNotifyDataAsync_ForValidNotification_WithBothNotifyMethods_ReturnsBothExpectedData(
            Type scenarioType, int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(
                DistributionChannels.Both, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedQueryService.Object)!;

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(2));

                NotifyData firstResult = actualResult[0];
                Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                Assert.That(firstResult.ContactDetails, Is.EqualTo(TestEmailAddress));
                Assert.That(firstResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, firstResult.NotificationMethod, this._testConfiguration)));

                NotifyData secondResult = actualResult[1];
                Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                Assert.That(secondResult.ContactDetails, Is.EqualTo(TestPhoneNumber));
                Assert.That(secondResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, secondResult.NotificationMethod, this._testConfiguration)));

                VerifyMethodCalls(mockedQueryContext, mockedQueryService,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), 1, 1, 0)]
        [TestCase(typeof(CaseStatusUpdatedScenario), 1, 1, 0)]
        [TestCase(typeof(CaseClosedScenario), 1, 1, 0)]
        public async Task GetAllNotifyDataAsync_ForNotification_WithoutNotifyMethod_ReturnsEmptyData_DoesNotThrowException(
            Type scenarioType, int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(
                DistributionChannels.None, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedQueryService.Object)!;

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(0));

                VerifyMethodCalls(mockedQueryContext, mockedQueryService,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseCreatedScenario), (DistributionChannels)(-1))]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseStatusUpdatedScenario), (DistributionChannels)(-1))]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseClosedScenario), (DistributionChannels)(-1))]
        public void GetAllNotifyDataAsync_ForNotification_WithUnknownNotifyMethod_ThrowsInvalidOperationException(
            Type scenarioType, DistributionChannels testDistributionChannel)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(
                testDistributionChannel, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedQueryService.Object)!;

            // Act & Assert
            Assert.Multiple(() =>
            {
                InvalidOperationException? exception =
                    Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message, Is.EqualTo(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown));

                VerifyMethodCalls(mockedQueryContext, mockedQueryService,
                    fromInvokeCount: 1, partyInvokeCount: 1, caseInvokeCount: 0);
            });
        }

        [Test]
        public void GetAllNotifyDataAsync_ForNotImplementedScenario_ThrowsNotImplementedException()
        {
            // Arrange
            var mockedQueryService = new Mock<IDataQueryService<NotificationEvent>>();

            INotifyScenario scenario = new NotImplementedScenario(this._testConfiguration, mockedQueryService.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.ThrowsAsync<NotImplementedException>(() => scenario.GetAllNotifyDataAsync(default));

                VerifyMethodCalls(new Mock<IQueryContext>(),
                    new Mock<IDataQueryService<NotificationEvent>>(),
                    fromInvokeCount: 0, partyInvokeCount: 0, caseInvokeCount: 0);
            });
        }
        #endregion

        #region Helper methods
        private static Mock<IQueryContext> MockQueryContextMethods(
            DistributionChannels testDistributionChannel, bool isCaseIdWhitelisted, bool isNotificationExpected)
        {
            // GetCaseAsync()
            var testCase = new Case
            {
                Name = "Test case",
                Identification = isCaseIdWhitelisted ? "1" : "4"
            };

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(
                    It.IsAny<object?>()))
                .ReturnsAsync(testCase);

            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync())
                .ReturnsAsync(new CaseStatuses());

            mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(
                    It.IsAny<CaseStatuses>()))
                .ReturnsAsync(new CaseType
                {
                    IsNotificationExpected = isNotificationExpected
                });

            // GetPartyDataAsync(string)
            var testParty = new CommonPartyData
            {
                Name = "Alice",
                SurnamePrefix = "van",
                Surname = "Wonderland",
                DistributionChannel = testDistributionChannel,
                EmailAddress = TestEmailAddress,
                TelephoneNumber = TestPhoneNumber
            };

            mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(
                    It.IsAny<string>()))
                .ReturnsAsync(testParty);

            return mockedQueryContext;
        }

        private static Mock<IDataQueryService<NotificationEvent>> GetMockedQueryService(IMock<IQueryContext> mockedQueryContext)
        {
            var mockedQueryService = new Mock<IDataQueryService<NotificationEvent>>();
            mockedQueryService
                .Setup(mock => mock.From(
                    It.IsAny<NotificationEvent>()))
                .Returns(mockedQueryContext.Object);

            return mockedQueryService;
        }

        private static Guid DetermineTemplateId<TStrategy>(TStrategy strategy, NotifyMethods notifyMethod, WebApiConfiguration configuration)
            where TStrategy : Type
        {
            return strategy.Name switch
            {
                nameof(CaseCreatedScenario) =>
                    notifyMethod switch
                    {
                        NotifyMethods.Email => configuration.User.TemplateIds.Email.ZaakCreate(),
                        NotifyMethods.Sms => configuration.User.TemplateIds.Sms.ZaakCreate(),
                        _ => Guid.Empty
                    },

                nameof(CaseStatusUpdatedScenario) =>
                    notifyMethod switch
                    {
                        NotifyMethods.Email => configuration.User.TemplateIds.Email.ZaakUpdate(),
                        NotifyMethods.Sms => configuration.User.TemplateIds.Sms.ZaakUpdate(),
                        _ => Guid.Empty
                    },

                nameof(CaseClosedScenario) =>
                    notifyMethod switch
                    {
                        NotifyMethods.Email => configuration.User.TemplateIds.Email.ZaakClose(),
                        NotifyMethods.Sms => configuration.User.TemplateIds.Sms.ZaakClose(),
                        _ => Guid.Empty
                    },

                _ => Guid.Empty
            };
        }

        private static void VerifyMethodCalls(Mock<IQueryContext> mockedQueryContext, Mock<IDataQueryService<NotificationEvent>> mockedQueryService,
            int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            mockedQueryService
                .Verify(mock => mock.From(
                        It.IsAny<NotificationEvent>()),
                    Times.Exactly(fromInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(
                        It.IsAny<string>()),
                    Times.Exactly(partyInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(
                        It.IsAny<object?>()),
                    Times.Exactly(caseInvokeCount));
        }
        #endregion
    }
}