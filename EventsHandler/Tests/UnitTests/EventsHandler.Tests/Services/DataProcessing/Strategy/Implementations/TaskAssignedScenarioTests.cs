// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
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
        public void GetAllNotifyDataAsync_InvalidTaskType_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(
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
        public void GetAllNotifyDataAsync_ValidTaskType_Closed_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(
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
        public void GetAllNotifyDataAsync_ValidTaskType_Open_NotAssignedToPerson_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(
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
        public void GetAllNotifyDataAsync_ValidTaskType_Open_AssignedToPerson_NotWhitelisted_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(
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
        public void GetAllNotifyDataAsync_ValidTaskType_Open_AssignedToPerson_Whitelisted_WithInformSetToFalse_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(
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

        [TestCase(DistributionChannels.Unknown)]
        [TestCase((DistributionChannels)(-1))]
        public void GetAllNotifyDataAsync_ValidTaskType_Open_AssignedToPerson_Whitelisted_WithInformSetToTrue_WithInvalidDistChannels_ThrowsInvalidOperationException(
            DistributionChannels invalidDistributionChannel)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(
                invalidDistributionChannel,
                s_taskOpenAssignedToPersonWithExpirationDate,
                true,
                true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                InvalidOperationException? exception =
                    Assert.ThrowsAsync<InvalidOperationException>(() => scenario.TryGetDataAsync(s_validNotification));
                Assert.That(exception?.Message, Is.EqualTo(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }

        // Single
        [TestCase(DistributionChannels.Email, 1)]
        [TestCase(DistributionChannels.Sms, 1)]
        // Both
        [TestCase(DistributionChannels.Both, 2)]
        // Unspecified
        [TestCase(DistributionChannels.None, 0)]
        public async Task GetAllNotifyDataAsync_ValidTaskType_Open_AssignedToPerson_Whitelisted_WithInformSetToTrue_WithValidDistChannels_ReturnsExpectedNotifyDataCount(
            DistributionChannels testDistributionChannel, int notifyDataCount)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(
                testDistributionChannel,
                s_taskOpenAssignedToPersonWithExpirationDate,
                isCaseIdWhitelisted: true,
                isNotificationExpected: true);

            // Act
            IReadOnlyCollection<NotifyData> actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Count.EqualTo(notifyDataCount));
                
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

            INotifyScenario scenario = ArrangeTaskScenario(
                testDistributionChannel,
                testTask,
                isCaseIdWhitelisted: true,
                isNotificationExpected: true);

            // Act
            IReadOnlyCollection<NotifyData> actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Count.EqualTo(1));

                string actualSerializedPersonalization = JsonSerializer.Serialize(actualResult.First().Personalization);
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

        // TODO: Add ProcessData tests

        #region Setup
        private INotifyScenario ArrangeTaskScenario(
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

            // INotifyService
            this._mockedNotifyService.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyData>()))
                .ReturnsAsync(new NotifySendResponse(true, "Test Email body"));

            this._mockedNotifyService.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyData>()))
                .ReturnsAsync(new NotifySendResponse(true, "Test SMS body"));

            // Task Scenario
            return new TaskAssignedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
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