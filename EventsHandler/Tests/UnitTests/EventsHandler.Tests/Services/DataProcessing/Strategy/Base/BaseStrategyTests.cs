// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using Moq;

namespace EventsHandler.UnitTests.Services.DataProcessing.Strategy.Base
{
    [TestFixture]
    public sealed class BaseStrategyTests
    {
        private readonly Mock<INotifyService<NotificationEvent, NotifyData>> _emptyMockedNotifyService = new(MockBehavior.Strict);

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

        #region Test data
        private const string TestEmailAddress = "test@gmail.com";
        private const string TestPhoneNumber = "+310123456789";
        #endregion

        #region TryGetDataAsync()
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms)]
        public void TryGetDataAsync_NotWhitelisted_ThrowsAbortedNotifyingException(
            Type scenarioType, DistributionChannels testDistributionChannel)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = GetMockedQueryContext(
                testDistributionChannel, isCaseIdWhitelisted: false, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType, mockedQueryService, this._emptyMockedNotifyService);

            // TODO: Rewrite to check failures
            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));

                string expectedErrorMessage = Resources.Processing_ABORT_DoNotSendNotification_CaseIdWhitelisted
                    .Replace("{0}", "4")
                    // Get substring
                    [..(Resources.Processing_ABORT_DoNotSendNotification_CaseIdWhitelisted.Length -
                        Resources.Processing_ABORT_DoNotSendNotification_CaseIdWhitelisted.IndexOf("{1}", StringComparison.Ordinal))];

                Assert.That(exception?.Message.StartsWith(expectedErrorMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(mockedQueryContext, mockedQueryService,
                    1, 1, 0, 0);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms)]
        public void TryGetDataAsync_Whitelisted_WithInformSetToFalse_ThrowsAbortedNotifyingException(
            Type scenarioType, DistributionChannels testDistributionChannel)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = GetMockedQueryContext(
                testDistributionChannel, isCaseIdWhitelisted: true, isNotificationExpected: false);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType, mockedQueryService, this._emptyMockedNotifyService);
            
            // TODO: Rewrite to check failures
            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_Informeren), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(mockedQueryContext, mockedQueryService,
                    1, 1, 1, 0);
            });
        }

        [TestCase(typeof(CaseCreatedScenario))]
        [TestCase(typeof(CaseStatusUpdatedScenario))]
        [TestCase(typeof(CaseClosedScenario))]
        public async Task TryGetDataAsync_Whitelisted_InformSetToTrue_WithoutNotifyMethod_ReturnsFailure(Type scenarioType)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = GetMockedQueryContext(
                DistributionChannels.None, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType, mockedQueryService, this._emptyMockedNotifyService);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.False);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(mockedQueryContext, mockedQueryService,
                    1, 1, 1, 1);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseCreatedScenario), (DistributionChannels)(-1))]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseStatusUpdatedScenario), (DistributionChannels)(-1))]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseClosedScenario), (DistributionChannels)(-1))]
        public async Task TryGetDataAsync_Whitelisted_InformSetToTrue_WithUnknownNotifyMethod_ReturnsFailure(
            Type scenarioType, DistributionChannels testDistributionChannel)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = GetMockedQueryContext(
                testDistributionChannel, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType, mockedQueryService, this._emptyMockedNotifyService);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.False);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(mockedQueryContext, mockedQueryService,
                    1, 1, 1, 1);
            });
        }
        
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email, NotifyMethods.Email, "test@gmail.com")]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms, NotifyMethods.Sms, "+310123456789")]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email, NotifyMethods.Email, "test@gmail.com")]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms, NotifyMethods.Sms, "+310123456789")]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email, NotifyMethods.Email, "test@gmail.com")]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms, NotifyMethods.Sms, "+310123456789")]
        public async Task TryGetDataAsync_Whitelisted_InformSetToTrue_WithValidNotifyMethod_Single_ReturnsSuccess(
            Type scenarioType, DistributionChannels testDistributionChannel, NotifyMethods expectedNotificationMethod, string expectedContactDetails)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = GetMockedQueryContext(
                testDistributionChannel, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType, mockedQueryService, this._emptyMockedNotifyService);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(1));

                NotifyData firstResult = actualResult.Content.First();
                Assert.That(firstResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod));
                Assert.That(firstResult.ContactDetails, Is.EqualTo(expectedContactDetails));
                Assert.That(firstResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, firstResult.NotificationMethod, this._testConfiguration)));

                VerifyGetDataMethodCalls(mockedQueryContext, mockedQueryService,
                    1, 1, 1, 1);
            });
        }

        [TestCase(typeof(CaseCreatedScenario))]
        [TestCase(typeof(CaseStatusUpdatedScenario))]
        [TestCase(typeof(CaseClosedScenario))]
        public async Task TryGetDataAsync_Whitelisted_InformSetToTrue_WithValidNotifyMethods_Both_ReturnsSuccess(Type scenarioType)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = GetMockedQueryContext(
                DistributionChannels.Both, isCaseIdWhitelisted: true, isNotificationExpected: true);
            
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType, mockedQueryService, this._emptyMockedNotifyService);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(2));

                NotifyData firstResult = actualResult.Content.First();
                Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                Assert.That(firstResult.ContactDetails, Is.EqualTo(TestEmailAddress));
                Assert.That(firstResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, firstResult.NotificationMethod, this._testConfiguration)));

                NotifyData secondResult = actualResult.Content.Last();
                Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                Assert.That(secondResult.ContactDetails, Is.EqualTo(TestPhoneNumber));
                Assert.That(secondResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, secondResult.NotificationMethod, this._testConfiguration)));

                VerifyGetDataMethodCalls(mockedQueryContext, mockedQueryService,
                    1, 1, 1, 1);
            });
        }

        [Test]
        public void TryGetDataAsync_ForNotImplementedScenario_ThrowsNotImplementedException()
        {
            // Arrange
            var mockedQueryService = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);

            INotifyScenario scenario = ArrangeSpecificScenario<NotImplementedScenario>(mockedQueryService, this._emptyMockedNotifyService);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.ThrowsAsync<NotImplementedException>(() => scenario.TryGetDataAsync(default));

                VerifyGetDataMethodCalls(new Mock<IQueryContext>(),
                    new Mock<IDataQueryService<NotificationEvent>>(),
                    0, 0, 0, 0);
            });
        }
        #endregion

        // TODO: Add GetPersonalization tests

        // TODO: Add ProcessData tests

        #region Setup
        private INotifyScenario ArrangeSpecificScenario<TScenario>(
            IMock<IDataQueryService<NotificationEvent>> mockedQueryService,
            IMock<INotifyService<NotificationEvent, NotifyData>> mockedNotifyService)
            where TScenario : class
        {
            return ArrangeSpecificScenario(typeof(TScenario), mockedQueryService, mockedNotifyService);
        }

        private INotifyScenario ArrangeSpecificScenario(
            Type scenarioType,
            IMock<IDataQueryService<NotificationEvent>> mockedQueryService,
            IMock<INotifyService<NotificationEvent, NotifyData>> mockedNotifyService)
        {
            return (BaseScenario)Activator.CreateInstance(scenarioType,
                this._testConfiguration, mockedQueryService.Object, mockedNotifyService.Object)!;
        }

        private static Mock<IQueryContext> GetMockedQueryContext(
            DistributionChannels testDistributionChannel, bool isCaseIdWhitelisted, bool isNotificationExpected)
        {
            // TryGetCaseAsync()
            var testCase = new Case
            {
                Name = "Test case",
                Identification = isCaseIdWhitelisted ? "1" : "4"
            };

            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<object?>()))
                .ReturnsAsync(testCase);

            mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()))
                .ReturnsAsync(new CaseType
                {
                    IsNotificationExpected = isNotificationExpected
                });

            mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses());

            // TryGetPartyDataAsync(string)
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
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string>()))
                .ReturnsAsync(testParty);

            return mockedQueryContext;
        }

        private static Mock<IDataQueryService<NotificationEvent>> GetMockedQueryService(IMock<IQueryContext> mockedQueryContext)
        {
            var mockedQueryService = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            mockedQueryService
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(mockedQueryContext.Object);

            return mockedQueryService;
        }

        private static Mock<INotifyService<NotificationEvent, NotifyData>> GetMockedNotifyService(
            NotifyData? emailNotifyData = default, NotifyData? smsNotifyData = default)
        {
            var mockedNotifyService = new Mock<INotifyService<NotificationEvent, NotifyData>>(MockBehavior.Strict);
            mockedNotifyService.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(), emailNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(NotifySendResponse.Success("Test Email body"));

            mockedNotifyService.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(), smsNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(NotifySendResponse.Success("Test SMS body"));

            return mockedNotifyService;
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
        #endregion

        #region Verify
        private static void VerifyGetDataMethodCalls(
            Mock<IQueryContext> mockedQueryContext,
            Mock<IDataQueryService<NotificationEvent>> mockedQueryService,
            int fromInvokeCount, int getCaseInvokeCount,
            int getCaseTypeInvokeCount, int getPartyInvokeCount)
        {
            mockedQueryService
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()),
                Times.Exactly(fromInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<object?>()),
                Times.Exactly(getCaseInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()),
                Times.Exactly(getCaseTypeInvokeCount));
            mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCaseTypeInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(It.IsAny<string>()),
                Times.Exactly(getPartyInvokeCount));
        }

        private static void VerifyProcessDataMethodCalls(
            Mock<INotifyService<NotificationEvent, NotifyData>> mockedNotifyService,
            int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            mockedNotifyService
                .Verify(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyData>()),
                Times.Exactly(sendEmailInvokeCount));
            
            mockedNotifyService
                .Verify(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyData>()),
                Times.Exactly(sendSmsInvokeCount));
        }
        #endregion
    }
}