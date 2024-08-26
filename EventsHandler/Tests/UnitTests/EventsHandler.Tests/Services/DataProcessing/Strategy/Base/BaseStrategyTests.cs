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
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedQueryService = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotificationEvent, NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

        private WebApiConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetValidEnvironmentConfiguration();
        }

        [TearDown]
        public void TestsReset()
        {
            this._mockedQueryContext.Reset();
            this._mockedQueryService.Reset();
            this._mockedNotifyService.Reset();

            this._getDataVerified = false;
            this._processDataVerified = false;
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._testConfiguration.Dispose();
        }

        #region Test data
        private const string TestEmailAddress = "test@email.com";
        private const string TestPhoneNumber = "911";
        #endregion

        #region TryGetDataAsync()
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms)]
        public void TryGetDataAsync_NotWhitelisted_ThrowsAbortedNotifyingException(
            Type scenarioType, DistributionChannels testNotifyMethod)
        {
            // Arrange
            GetMockedServices_TryGetData(testNotifyMethod, false, true);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);

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
                
                VerifyGetDataMethodCalls(1, 1, 0, 0);
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
            GetMockedServices_TryGetData(
                testDistributionChannel, true, false);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);
            
            // TODO: Rewrite to check failures
            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_Informeren), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 1, 1, 0);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.None)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseCreatedScenario), (DistributionChannels)(-1))]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.None)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseStatusUpdatedScenario), (DistributionChannels)(-1))]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.None)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Unknown)]
        [TestCase(typeof(CaseClosedScenario), (DistributionChannels)(-1))]
        public async Task TryGetDataAsync_Whitelisted_InformSetToTrue_WithUnknownNotifyMethod_ReturnsFailure(
            Type scenarioType, DistributionChannels invalidDistributionChannel)
        {
            // Arrange
            GetMockedServices_TryGetData(
                invalidDistributionChannel, true, true);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.False);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1, 1);
            });
        }
        
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Both, null, 2, TestEmailAddress + TestPhoneNumber)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Email, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Sms, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(typeof(CaseStatusUpdatedScenario), DistributionChannels.Both, null, 2, TestEmailAddress + TestPhoneNumber)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Email, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Sms, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(typeof(CaseClosedScenario), DistributionChannels.Both, null, 2, TestEmailAddress + TestPhoneNumber)]
        public async Task TryGetDataAsync_Whitelisted_InformSetToTrue_WithValidNotifyMethod_Single_ReturnsSuccess(
            Type scenarioType, DistributionChannels testDistributionChannel, NotifyMethods? expectedNotificationMethod, int notifyDataCount, string expectedContactDetails)
        {
            // Arrange
            GetMockedServices_TryGetData(
                testDistributionChannel, true, true);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(notifyDataCount));

                string contactDetails;

                if (testDistributionChannel == DistributionChannels.Both)
                {
                    NotifyData firstResult = actualResult.Content.First();
                    Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                    Assert.That(firstResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(scenarioType, firstResult.NotificationMethod, this._testConfiguration)));

                    NotifyData secondResult = actualResult.Content.Last();
                    Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                    Assert.That(secondResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(scenarioType, secondResult.NotificationMethod, this._testConfiguration)));

                    contactDetails = firstResult.ContactDetails + secondResult.ContactDetails;
                }
                else
                {
                    NotifyData onlyResult = actualResult.Content.First();
                    Assert.That(onlyResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod!.Value));
                    Assert.That(onlyResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(scenarioType, onlyResult.NotificationMethod, this._testConfiguration)));

                    contactDetails = onlyResult.ContactDetails;
                }

                Assert.That(contactDetails, Is.EqualTo(expectedContactDetails));

                VerifyGetDataMethodCalls(1, 1, 1, 1);
            });
        }

        [Test]
        public void TryGetDataAsync_ForNotImplementedScenario_ThrowsNotImplementedException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeSpecificScenario<NotImplementedScenario>();

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.ThrowsAsync<NotImplementedException>(() => scenario.TryGetDataAsync(default));

                VerifyGetDataMethodCalls(0, 0, 0, 0);
            });
        }
        #endregion

        // TODO: Add GetPersonalization tests

        #region ProcessDataAsync()
        [TestCase(typeof(CaseCreatedScenario))]
        [TestCase(typeof(CaseStatusUpdatedScenario))]
        [TestCase(typeof(CaseClosedScenario))]
        public async Task ProcessDataAsync_EmptyNotifyData_ReturnsFailure(Type scenarioType)
        {
            // Arrange
            GetMockedServices_ProcessData(
                isSendingSuccessful: true);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, Array.Empty<NotifyData>());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsSuccess, Is.False);
                Assert.That(actualResponse.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_MissingNotifyData));

                VerifyProcessDataMethodCalls(0, 0);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), NotifyMethods.None)]
        [TestCase(typeof(CaseCreatedScenario), (NotifyMethods)(-1))]
        [TestCase(typeof(CaseStatusUpdatedScenario), NotifyMethods.None)]
        [TestCase(typeof(CaseStatusUpdatedScenario), (NotifyMethods)(-1))]
        [TestCase(typeof(CaseClosedScenario), NotifyMethods.None)]
        [TestCase(typeof(CaseClosedScenario), (NotifyMethods)(-1))]
        public async Task ProcessDataAsync_ValidNotifyData_InvalidNotifyMethod_ReturnsFailure(
            Type scenarioType, NotifyMethods invalidNotifyMethod)
        {
            // Arrange
            NotifyData testData = GetNotifyData(invalidNotifyMethod);

            GetMockedServices_ProcessData(
                isSendingSuccessful: true,
                testData);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, new[] { testData });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsSuccess, Is.False);
                Assert.That(actualResponse.Message, Is.EqualTo(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown));

                VerifyProcessDataMethodCalls(0, 0);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), NotifyMethods.Email, 1, 0)]
        [TestCase(typeof(CaseCreatedScenario), NotifyMethods.Sms, 0, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), NotifyMethods.Email, 1, 0)]
        [TestCase(typeof(CaseStatusUpdatedScenario), NotifyMethods.Sms, 0, 1)]
        [TestCase(typeof(CaseClosedScenario), NotifyMethods.Email, 1, 0)]
        [TestCase(typeof(CaseClosedScenario), NotifyMethods.Sms, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingFailed_ReturnsFailure(
            Type scenarioType, NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            // Arrange
            NotifyData testData = GetNotifyData(testNotifyMethod);

            GetMockedServices_ProcessData(
                isSendingSuccessful: false,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, new[] { testData });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsSuccess, Is.False);
                Assert.That(actualResponse.Message, Is.EqualTo(SimulatedNotifyExceptionMessage));

                VerifyProcessDataMethodCalls(sendEmailInvokeCount, sendSmsInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), NotifyMethods.Email, 1, 0)]
        [TestCase(typeof(CaseCreatedScenario), NotifyMethods.Sms, 0, 1)]
        [TestCase(typeof(CaseStatusUpdatedScenario), NotifyMethods.Email, 1, 0)]
        [TestCase(typeof(CaseStatusUpdatedScenario), NotifyMethods.Sms, 0, 1)]
        [TestCase(typeof(CaseClosedScenario), NotifyMethods.Email, 1, 0)]
        [TestCase(typeof(CaseClosedScenario), NotifyMethods.Sms, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingSuccessful_ReturnsSuccess(
            Type scenarioType, NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            // Arrange
            NotifyData testData = GetNotifyData(testNotifyMethod);

            GetMockedServices_ProcessData(
                isSendingSuccessful: true,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null);

            INotifyScenario scenario = ArrangeSpecificScenario(scenarioType);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, new[] { testData });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsSuccess, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(Resources.Processing_SUCCESS_Scenario_DataProcessed));

                VerifyProcessDataMethodCalls(sendEmailInvokeCount, sendSmsInvokeCount);
            });
        }
        #endregion

        #region Setup
        private INotifyScenario ArrangeSpecificScenario<TScenario>()
            where TScenario : class
        {
            return ArrangeSpecificScenario(typeof(TScenario));
        }

        private INotifyScenario ArrangeSpecificScenario(Type scenarioType)
        {
            return (BaseScenario)Activator.CreateInstance(scenarioType,
                this._testConfiguration, this._mockedQueryService.Object, this._mockedNotifyService.Object)!;
        }

        private void GetMockedServices_TryGetData(
            DistributionChannels testDistributionChannel, bool isCaseIdWhitelisted, bool isNotificationExpected)
        {
            // IQueryContext
            // TryGetCaseAsync()
            var testCase = new Case
            {
                Name = "Test case",
                Identification = isCaseIdWhitelisted ? "1" : "4"
            };

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<object?>()))
                .ReturnsAsync(testCase);
            
            this._mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()))
                .ReturnsAsync(new CaseType
                {
                    IsNotificationExpected = isNotificationExpected
                });
            
            this._mockedQueryContext
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
            
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string>()))
                .ReturnsAsync(testParty);
            
            // IDataQueryService
            this._mockedQueryService
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);
        }

        private const string SimulatedNotifyExceptionMessage = "Some NotifyClientException";

        private void GetMockedServices_ProcessData(
            bool isSendingSuccessful, NotifyData? emailNotifyData = default, NotifyData? smsNotifyData = default)
        {
            // INotifyService
            this._mockedNotifyService.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(), emailNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            this._mockedNotifyService.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(), smsNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));
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

        private static NotifyData GetNotifyData(NotifyMethods method)
        {
            return new NotifyData(method,
                string.Empty,
                Guid.Empty,
                new Dictionary<string, object>());
        }
        #endregion

        #region Verify
        private bool _getDataVerified;
        private bool _processDataVerified;

        private void VerifyGetDataMethodCalls(int fromInvokeCount, int getCaseInvokeCount, int getCaseTypeInvokeCount, int getPartyInvokeCount)
        {
            if (this._getDataVerified)
            {
                return;
            }
            
            // IDataQueryService
            this._mockedQueryService
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()),
                Times.Exactly(fromInvokeCount));

            // IQueryContext
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<object?>()),
                Times.Exactly(getCaseInvokeCount));

            this._mockedQueryContext  // Dependent queries
                .Verify(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()),
                Times.Exactly(getCaseTypeInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCaseTypeInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(It.IsAny<string>()),
                Times.Exactly(getPartyInvokeCount));

            this._getDataVerified = true;

            VerifyProcessDataMethodCalls(0, 0);
        }

        private void VerifyProcessDataMethodCalls(int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            if (this._processDataVerified)
            {
                return;
            }
            
            // INotifyService
            this._mockedNotifyService
                .Verify(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyData>()),
                Times.Exactly(sendEmailInvokeCount));
            
            this._mockedNotifyService
                .Verify(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyData>()),
                Times.Exactly(sendSmsInvokeCount));

            this._processDataVerified = true;

            VerifyGetDataMethodCalls(0, 0, 0, 0);
        }
        #endregion
    }
}