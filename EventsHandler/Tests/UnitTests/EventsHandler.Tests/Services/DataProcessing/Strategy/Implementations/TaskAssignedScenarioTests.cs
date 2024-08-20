// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
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
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using Moq;
using System.Text.Json;

namespace EventsHandler.UnitTests.Services.DataProcessing.Strategy.Implementations
{
    [TestFixture]
    public sealed class TaskAssignedScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotificationEvent, NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

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

        [TearDown]
        public void ResetTests()
        {
            this._mockedDataQuery.Reset();
            this._mockedQueryContext.Reset();
            this._mockedNotifyService.Reset();
        }

        #region Test data
        private static readonly NotificationEvent s_invalidNotification = new();
        private static readonly NotificationEvent s_validNotification = new()
        {
            Attributes = new EventAttributes
            {
                ObjectTypeUri = new Uri($"http://www.domain.com/{ConfigurationHandler.TestTypeUuid}")
            }
        };

        private static readonly TaskObject s_taskClosed = new()
        {
            Record = new Record
            {
                Data = new Data
                {
                    Status = TaskStatuses.Closed
                }
            }
        };

        private static readonly TaskObject s_taskOpenNotAssignedToPerson = new()
        {
            Record = new Record
            {
                Data = new Data
                {
                    Status = TaskStatuses.Open,
                    Identification = new Identification
                    {
                        Type = IdTypes.Unknown
                    }
                }
            }
        };

        private const string TestTaskTitle = "Test title";
        private static readonly TaskObject s_taskOpenAssignedToPersonWithoutExpirationDate = new()
        {
            Record = new Record
            {
                Data = new Data
                {
                    Title = TestTaskTitle,
                    Status = TaskStatuses.Open,
                    // Missing "ExpirationDate" => default
                    Identification = new Identification
                    {
                        Type = IdTypes.Bsn
                    }
                }
            }
        };

        private static readonly TaskObject s_taskOpenAssignedToPersonWithExpirationDate = new()
        {
            Record = new Record
            {
                Data = new Data
                {
                    Title = TestTaskTitle,
                    Status = TaskStatuses.Open,
                    ExpirationDate = new DateTime(2024, 7, 24, 14, 10, 40, DateTimeKind.Utc),
                    Identification = new Identification
                    {
                        Type = IdTypes.Bsn
                    }
                }
            }
        };
        #endregion

        #region TryGetDataAsync()
        [Test]
        public void TryGetDataAsync_InvalidTaskType_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                s_taskOpenAssignedToPersonWithExpirationDate,
                true,
                true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_invalidNotification));  // Notification doesn't have matching GUID in task type
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_TaskType), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidTaskType_Closed_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                s_taskClosed,
                true,
                true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_TaskClosed), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidTaskType_Open_NotAssignedToPerson_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                s_taskOpenNotAssignedToPerson,
                true,
                true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_TaskNotPerson), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidTaskType_Open_AssignedToPerson_NotWhitelisted_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                s_taskOpenAssignedToPersonWithExpirationDate,
                isCaseIdWhitelisted: false,
                isNotificationExpected: true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));

                string expectedErrorMessage = Resources.Processing_ABORT_DoNotSendNotification_CaseIdWhitelisted
                    .Replace("{0}", "4")
                    .Replace("{1}", "USER_WHITELIST_TASKASSIGNED_IDS");

                Assert.That(exception?.Message.StartsWith(expectedErrorMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidTaskType_Open_AssignedToPerson_Whitelisted_WithInformSetToFalse_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                s_taskOpenAssignedToPersonWithExpirationDate,
                isCaseIdWhitelisted: true,
                isNotificationExpected: false);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_Informeren), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 1, 0);
            });
        }

        [Test]
        public async Task TryGetDataAsync_ValidTaskType_Open_AssignedToPerson_Whitelisted_WithInformSetToTrue_WithoutNotifyMethod_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.None,
                s_taskOpenAssignedToPersonWithExpirationDate,
                true,
                true);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.False);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }

        [TestCase(DistributionChannels.Unknown)]
        [TestCase((DistributionChannels)(-1))]
        public async Task TryGetDataAsync_ValidTaskType_Open_AssignedToPerson_Whitelisted_WithInformSetToTrue_WithUnknownNotifyMethod_ReturnsFailure(
            DistributionChannels invalidDistributionChannel)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                invalidDistributionChannel,
                s_taskOpenAssignedToPersonWithExpirationDate,
                true,
                true);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.False);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }

        // Single
        [TestCase(DistributionChannels.Email, 1)]
        [TestCase(DistributionChannels.Sms, 1)]
        // Both
        [TestCase(DistributionChannels.Both, 2)]
        public async Task TryGetDataAsync_ValidTaskType_Open_AssignedToPerson_Whitelisted_WithInformSetToTrue_WithValidDistChannels_ReturnsSuccess(
            DistributionChannels testDistributionChannel, int notifyDataCount)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                testDistributionChannel,
                s_taskOpenAssignedToPersonWithExpirationDate,
                isCaseIdWhitelisted: true,
                isNotificationExpected: true);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(notifyDataCount));

                if (testDistributionChannel == DistributionChannels.Both)
                {
                    NotifyData firstResult = actualResult.Content.First();
                    Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));

                    NotifyData secondResult = actualResult.Content.Last();
                    Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                }
                
                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }
        #endregion

        #region GetPersonalizationAsync()
        [TestCase(DistributionChannels.Email, false, "-", "no")]
        [TestCase(DistributionChannels.Email, true, "woensdag 24 juli 2024 16:10", "yes")]
        [TestCase(DistributionChannels.Sms, false, "-", "no")]
        [TestCase(DistributionChannels.Sms, true, "woensdag 24 juli 2024 16:10", "yes")]
        public async Task GetPersonalizationAsync_SpecificDateTime_ReturnsExpectedPersonalization(
            DistributionChannels testDistributionChannel, bool isExpirationDateGiven, string testExpirationDate, string isExpirationDateGivenText)
        {
            TaskObject testTask = isExpirationDateGiven
                ? s_taskOpenAssignedToPersonWithExpirationDate
                : s_taskOpenAssignedToPersonWithoutExpirationDate;

            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                testDistributionChannel,
                testTask,
                isCaseIdWhitelisted: true,
                isNotificationExpected: true);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(1));

                string actualSerializedPersonalization = JsonSerializer.Serialize(actualResult.Content.First().Personalization);
                string expectedSerializedPersonalization =
                    $"{{" +
                      $"\"klant.voornaam\":\"Jackie\"," +
                      $"\"klant.voorvoegselAchternaam\":null," +
                      $"\"klant.achternaam\":\"Chan\"," +

                      $"\"taak.verloopdatum\":\"{testExpirationDate}\"," +
                      $"\"taak.heeft_verloopdatum\":\"{isExpirationDateGivenText}\"," +
                      $"\"taak.record.data.title\":\"{TestTaskTitle}\"," +

                      $"\"zaak.identificatie\":\"1\"," +
                      $"\"zaak.omschrijving\":\"\"" +
                    $"}}";

                Assert.That(actualSerializedPersonalization, Is.EqualTo(expectedSerializedPersonalization));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }
        #endregion

        #region ProcessDataAsync()
        [Test]
        public async Task ProcessDataAsync_InvalidNotifyData_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(true);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, Array.Empty<NotifyData>());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsSuccess, Is.False);
                Assert.That(actualResponse.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_MissingData));

                VerifyProcessDataMethodCalls(0, 0);
            });
        }

        [TestCase(NotifyMethods.None)]
        [TestCase((NotifyMethods)(-1))]
        public async Task ProcessDataAsync_ValidNotifyData_InvalidNotifyMethod_ReturnsFailure(NotifyMethods invalidNotifyMethod)
        {
            // Arrange
            NotifyData testData = GetNotifyData(invalidNotifyMethod);

            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(
                isSendingSuccessful: false,
                testData);

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

        [TestCase(NotifyMethods.Email, 1, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingFailed_ReturnsFailure(
            NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            // Arrange
            NotifyData testData = GetNotifyData(testNotifyMethod);

            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(
                isSendingSuccessful: false,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null);

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

        [TestCase(NotifyMethods.Email, 1, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingSuccessful_ReturnsSuccess(
            NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            // Arrange
            NotifyData testData = GetNotifyData(testNotifyMethod);

            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(
                isSendingSuccessful: true,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null);

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
        private INotifyScenario ArrangeTaskScenario_TryGetData(
            DistributionChannels testDistributionChannel, TaskObject testTask, bool isCaseIdWhitelisted, bool isNotificationExpected)
        {
            // IQueryContext
            this._mockedQueryContext
                .Setup(mock => mock.GetTaskAsync())
                .ReturnsAsync(testTask);

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<object?>()))
                .ReturnsAsync(new Case
                {
                    Identification = isCaseIdWhitelisted ? "1" : "4"
                });

            this._mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()))
                .ReturnsAsync(new CaseType
                {
                    IsNotificationExpected = isNotificationExpected
                });

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses());

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new CommonPartyData
                {
                    Name = "Jackie",
                    Surname = "Chan",
                    DistributionChannel = testDistributionChannel
                });

            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Task Scenario
            return new TaskAssignedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }

        private const string SimulatedNotifyExceptionMessage = "Some NotifyClientException";

        private INotifyScenario ArrangeTaskScenario_ProcessData(
            bool isSendingSuccessful, NotifyData? emailNotifyData = default, NotifyData? smsNotifyData = default)
        {
            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // INotifyService
            this._mockedNotifyService.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(), emailNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            this._mockedNotifyService.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(), smsNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            // Task Scenario
            return new TaskAssignedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
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
        private void VerifyGetDataMethodCalls(int getCaseAsyncInvokeCount, int getCaseTypeInvokeCount, int getPartyDataAsyncInvokeCount)
        {
            this._mockedDataQuery
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()),
                Times.Once);

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<object?>()),
                Times.Exactly(getCaseAsyncInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()),
                Times.Exactly(getCaseTypeInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCaseTypeInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(It.IsAny<string>()),
                    Times.Exactly(getPartyDataAsyncInvokeCount));
        }

        private void VerifyProcessDataMethodCalls(int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
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
        }
        #endregion
    }
}