// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using Moq;

namespace EventsHandler.Tests.Unit.Services.DataProcessing.Strategy.Implementations
{
    [TestFixture]
    public sealed class MessageReceivedScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

        [TearDown]
        public void TestsReset()
        {
            this._mockedDataQuery.Reset();
            this._mockedQueryContext.Reset();
            this._mockedNotifyService.Reset();

            this._getDataVerified = false;
            this._processDataVerified = false;
        }

        #region Test data
        private static readonly NotificationEvent s_validNotification = new()
        {
            Attributes = new EventAttributes
            {
                ObjectTypeUri = new Uri($"http://www.domain.com/{ConfigurationHandler.TestInfoObjectTypeUuid1}")
            }
        };

        private const string TestEmailAddress = "test@email.com";
        private const string TestPhoneNumber = "911";
        #endregion

        #region TryGetDataAsync()
        [Test]
        public void TryGetDataAsync_NotAllowedMessages_ThrowsAbortedNotifyingException()
        {
            // Arrange
            using WebApiConfiguration configuration = GetConfiguration(isMessageAllowed: false);
            INotifyScenario scenario = ArrangeMessageScenario_TryGetData(configuration);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));

                string expectedMessage = ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_MessagesForbidden
                    .Replace("{0}", "ZGW_WHITELIST_MESSAGE_ALLOWED");

                Assert.That(exception?.Message.StartsWith(expectedMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(0, 0, 0);
            });
        }
        
        [TestCase(DistributionChannels.None)]
        [TestCase(DistributionChannels.Unknown)]
        [TestCase((DistributionChannels)(-1))]
        public async Task TryGetDataAsync_AllowedMessages_WithInvalidDistChannel_ReturnsFailure(
            DistributionChannels invalidDistributionChannel)
        {
            // Arrange
            using WebApiConfiguration configuration = GetConfiguration(isMessageAllowed: true);
            INotifyScenario scenario = ArrangeMessageScenario_TryGetData(configuration, invalidDistributionChannel);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }
        
        [TestCase(DistributionChannels.Email, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(DistributionChannels.Sms, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(DistributionChannels.Both, null, 2, TestEmailAddress + TestPhoneNumber)]
        public async Task TryGetDataAsync_AllowedMessages_WithValidDistChannels_ReturnsSuccess(
            DistributionChannels testDistributionChannel, NotifyMethods? expectedNotificationMethod, int notifyDataCount, string expectedContactDetails)
        {
            // Arrange
            using WebApiConfiguration configuration = GetConfiguration(isMessageAllowed: true);
            INotifyScenario scenario = ArrangeMessageScenario_TryGetData(configuration, testDistributionChannel);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(notifyDataCount));

                string contactDetails;

                if (testDistributionChannel == DistributionChannels.Both)
                {
                    NotifyData firstResult = actualResult.Content.First();
                    Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                    Assert.That(firstResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(firstResult.NotificationMethod, configuration)));

                    NotifyData secondResult = actualResult.Content.Last();
                    Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                    Assert.That(secondResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(firstResult.NotificationMethod, configuration)));

                    contactDetails = firstResult.ContactDetails + secondResult.ContactDetails;
                }
                else
                {
                    NotifyData onlyResult = actualResult.Content.First();
                    Assert.That(onlyResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod!.Value));
                    Assert.That(onlyResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(onlyResult.NotificationMethod, configuration)));

                    contactDetails = onlyResult.ContactDetails;
                }

                Assert.That(contactDetails, Is.EqualTo(expectedContactDetails));
                
                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }
        #endregion

        #region ProcessDataAsync()
        [Test]
        public async Task ProcessDataAsync_EmptyNotifyData_ReturnsFailure()
        {
            // Arrange
            using WebApiConfiguration configuration = GetConfiguration(isMessageAllowed: true);
            INotifyScenario scenario = ArrangeMessageScenario_ProcessData(
                configuration,
                isSendingSuccessful: true,
                new NotifyData(NotifyMethods.Email));

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsFailure, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(ApiResources.Processing_ERROR_Scenario_MissingNotifyData));

                VerifyProcessDataMethodCalls(0, 0);
            });
        }

        [TestCase(NotifyMethods.None)]
        [TestCase((NotifyMethods)(-1))]
        public async Task ProcessDataAsync_ValidNotifyData_InvalidNotifyMethod_ReturnsFailure(NotifyMethods invalidNotifyMethod)
        {
            // Arrange
            NotifyData testData = new(invalidNotifyMethod);

            using WebApiConfiguration configuration = GetConfiguration(isMessageAllowed: true);
            INotifyScenario scenario = ArrangeMessageScenario_ProcessData(
                configuration,
                isSendingSuccessful: true,
                testData);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, [testData]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsFailure, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(ApiResources.Processing_ERROR_Notification_DeliveryMethodUnknown));

                VerifyProcessDataMethodCalls(0, 0);
            });
        }

        [TestCase(NotifyMethods.Email, 1, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingFailed_ReturnsFailure(
            NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            // Arrange
            NotifyData testData = new(testNotifyMethod);
            
            using WebApiConfiguration configuration = GetConfiguration(isMessageAllowed: true);
            INotifyScenario scenario = ArrangeMessageScenario_ProcessData(
                configuration,
                isSendingSuccessful: false,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, [testData]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsFailure, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(SimulatedNotifyExceptionMessage));

                VerifyProcessDataMethodCalls(sendEmailInvokeCount, sendSmsInvokeCount);
            });
        }

        [TestCase(NotifyMethods.Email, 1, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingSuccessful_ReturnsSuccess(
            NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            // Arrange
            NotifyData testData = new(testNotifyMethod);
            
            using WebApiConfiguration configuration = GetConfiguration(isMessageAllowed: true);
            INotifyScenario scenario = ArrangeMessageScenario_ProcessData(
                configuration,
                isSendingSuccessful: true,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, [testData]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsSuccess, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(ApiResources.Processing_SUCCESS_Scenario_DataProcessed));
                
                VerifyProcessDataMethodCalls(sendEmailInvokeCount, sendSmsInvokeCount);
            });
        }
        #endregion

        #region Setup
        private static WebApiConfiguration GetConfiguration(bool isMessageAllowed)
        {
            return isMessageAllowed
                ? ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1)
                : ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v1);
        }

        private MessageReceivedScenario ArrangeMessageScenario_TryGetData(
            WebApiConfiguration configuration, DistributionChannels testDistributionChannel = DistributionChannels.Email)
        {
            // IQueryContext
            this._mockedQueryContext
                .Setup(mock => mock.GetMessageAsync())
                .ReturnsAsync(new MessageObject());

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(
                    It.IsAny<Uri?>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(new CommonPartyData
                {
                    Name = "Jackie",
                    Surname = "Chan",
                    DistributionChannel = testDistributionChannel,
                    EmailAddress = TestEmailAddress,
                    TelephoneNumber = TestPhoneNumber
                });

            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(
                    It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Decision Scenario
            return new MessageReceivedScenario(configuration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }

        private const string SimulatedNotifyExceptionMessage = "Some NotifyClientException";

        private TaskAssignedScenario ArrangeMessageScenario_ProcessData(
            WebApiConfiguration configuration, bool isSendingSuccessful,
            NotifyData? emailNotifyData = default, NotifyData? smsNotifyData = default)
        {
            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(
                    It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // INotifyService
            this._mockedNotifyService.Setup(mock => mock.SendEmailAsync(emailNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            this._mockedNotifyService.Setup(mock => mock.SendSmsAsync(smsNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            // Task Scenario
            return new TaskAssignedScenario(configuration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }

        private static Guid DetermineTemplateId(NotifyMethods notifyMethod, WebApiConfiguration configuration)
        {
            return notifyMethod switch
            {
                NotifyMethods.Email => configuration.Notify.TemplateId.Email.MessageReceived(),
                NotifyMethods.Sms => configuration.Notify.TemplateId.Sms.MessageReceived(),
                _ => Guid.Empty
            };
        }
        #endregion

        #region Verify
        private bool _getDataVerified;
        private bool _processDataVerified;

        private void VerifyGetDataMethodCalls(int fromInvokeCount, int getMessageInvokeCount, int getPartyDataInvokeCount)
        {
            if (this._getDataVerified)
            {
                return;
            }

            // IDataQueryService
            this._mockedDataQuery
                .Verify(mock => mock.From(
                    It.IsAny<NotificationEvent>()),
                Times.Exactly(fromInvokeCount));

            // IQueryContext
            this._mockedQueryContext
                .Verify(mock => mock.GetMessageAsync(),
                Times.Exactly(getMessageInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(
                    It.IsAny<Uri?>(),
                    It.IsAny<string?>()),
                Times.Exactly(getPartyDataInvokeCount));

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
                    It.IsAny<NotifyData>()),
                Times.Exactly(sendEmailInvokeCount));

            this._mockedNotifyService
                .Verify(mock => mock.SendSmsAsync(
                    It.IsAny<NotifyData>()),
                Times.Exactly(sendSmsInvokeCount));

            this._processDataVerified = true;

            VerifyGetDataMethodCalls(0, 0, 0);
        }
        #endregion
    }
}