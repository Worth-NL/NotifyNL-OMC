// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Implementations;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.Objecten;
using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.Objecten;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Moq;
using System.Text.Json;

namespace EventsHandler.UnitTests.Behaviors.Communication.Strategy.Implementations
{
    [TestFixture]
    public sealed class TaskAssignedScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);

        #region Test data
        private static readonly WebApiConfiguration s_emptyConfiguration = ConfigurationHandler.GetWebApiConfiguration();
        private static readonly WebApiConfiguration s_testConfiguration = ConfigurationHandler.GetValidEnvironmentConfiguration();

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

        [TearDown]
        public void ResetTests()
        {
            this._mockedDataQuery.Reset();
            this._mockedQueryContext.Reset();
        }

        #region GetAllNotifyDataAsync()
        [Test]
        public void GetAllNotifyDataAsync_ForInvalidTaskType_ThrowsAbortedNotifyingException()
        {
            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.IsValidType())
                .Returns(false);  // Invalid condition

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            INotifyScenario scenario = new TaskAssignedScenario(s_emptyConfiguration, this._mockedDataQuery.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message, Is.EqualTo(Resources.Processing_ABORT_DoNotSendNotification_TaskType));

                VerifyInvoke(getPartyDataAsyncInvokeCount: 0, getCaseAsyncInvokeCount: 0);
            });
        }

        [Test]
        public void GetAllNotifyDataAsync_ForClosedTask_ThrowsAbortedNotifyingException()
        {
            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.IsValidType())
                .Returns(true);
            this._mockedQueryContext
                .Setup(mock => mock.GetTaskAsync())
                .ReturnsAsync(s_taskClosed);  // Invalid condition

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            INotifyScenario scenario = new TaskAssignedScenario(s_emptyConfiguration, this._mockedDataQuery.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message, Is.EqualTo(Resources.Processing_ABORT_DoNotSendNotification_TaskClosed));

                VerifyInvoke(getPartyDataAsyncInvokeCount: 0, getCaseAsyncInvokeCount: 0);
            });
        }

        [Test]
        public void GetAllNotifyDataAsync_ForOpenTask_NotAssignedToPerson_ThrowsAbortedNotifyingException()
        {
            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.IsValidType())
                .Returns(true);
            this._mockedQueryContext
                .Setup(mock => mock.GetTaskAsync())
                .ReturnsAsync(s_taskOpenNotAssignedToPerson);  // Invalid condition

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            INotifyScenario scenario = new TaskAssignedScenario(s_emptyConfiguration, this._mockedDataQuery.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message, Is.EqualTo(Resources.Processing_ABORT_DoNotSendNotification_TaskNotPerson));

                VerifyInvoke(getPartyDataAsyncInvokeCount: 0, getCaseAsyncInvokeCount: 0);
            });
        }

        [TestCase(DistributionChannels.Email, 1, 1)]
        [TestCase(DistributionChannels.Sms, 1, 1)]
        [TestCase(DistributionChannels.Both, 2, 1)]
        [TestCase(DistributionChannels.None, 0, 0)]
        public async Task GetAllNotifyDataAsync_ForOpenTask_AssignedToPerson_WithValidDistChannels_ReturnsExpectedNotifyDataCount(
            DistributionChannels testDistributionChannel, int notifyDataCount, int getCaseAsyncInvokeCount)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario(testDistributionChannel, s_taskOpenAssignedToPersonWithoutExpirationDate);

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(notifyDataCount));

                VerifyInvoke(getPartyDataAsyncInvokeCount: 1, getCaseAsyncInvokeCount);
            });
        }

        [TestCase(DistributionChannels.Unknown)]
        [TestCase((DistributionChannels)(-1))]
        public void GetAllNotifyDataAsync_ForOpenTask_AssignedToPerson_WithInvalidDistChannels_ThrowsInvalidOperationException(
            DistributionChannels testDistributionChannel)
        {
            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.IsValidType())
                .Returns(true);
            this._mockedQueryContext
                .Setup(mock => mock.GetTaskAsync())
                .ReturnsAsync(s_taskOpenAssignedToPersonWithoutExpirationDate);
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new CommonPartyData
                {
                    DistributionChannel = testDistributionChannel,
                    Name = "Faye",
                    Surname = "Valentine",
                    EmailAddress = "cowboy@bebop.org"
                });

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            INotifyScenario scenario = new TaskAssignedScenario(s_testConfiguration, this._mockedDataQuery.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                InvalidOperationException? exception =
                    Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message, Is.EqualTo(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown));

                VerifyInvoke(getPartyDataAsyncInvokeCount: 1, getCaseAsyncInvokeCount: 0);
            });
        }
        #endregion

        #region GetPersonalizationAsync()
        [TestCase(DistributionChannels.Email, false, "-", "no")]
        [TestCase(DistributionChannels.Email, true, "woensdag 24 juli 2024 16:10", "yes")]
        [TestCase(DistributionChannels.Sms, false, "-", "no")]
        [TestCase(DistributionChannels.Sms, true, "woensdag 24 juli 2024 16:10", "yes")]
        public async Task GetPersonalizationAsync_ForSpecificDateTime_ReturnsExpectedPersonalization(
            DistributionChannels testDistributionChannel, bool isExpirationDateGiven, string testExpirationDate, string isExpirationDateGivenText)
        {
            TaskObject testTask = isExpirationDateGiven
                ? s_taskOpenAssignedToPersonWithExpirationDate
                : s_taskOpenAssignedToPersonWithoutExpirationDate;

            INotifyScenario scenario = ArrangeTaskScenario(testDistributionChannel, testTask);

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(1));

                string actualSerializedPersonalization = JsonSerializer.Serialize(actualResult[0].Personalization);
                string expectedSerializedPersonalization =
                    $"{{" +
                      $"\"taak.verloopdatum\":\"{testExpirationDate}\"," +
                      $"\"taak.heeft_verloopdatum\":\"{isExpirationDateGivenText}\"," +
                      $"\"taak.record.data.title\":\"{TestTaskTitle}\"," +
                      $"\"zaak.omschrijving\":\"\"," +
                      $"\"zaak.identificatie\":\"\"" +
                    $"}}";

                Assert.That(actualSerializedPersonalization, Is.EqualTo(expectedSerializedPersonalization));

                VerifyInvoke(getPartyDataAsyncInvokeCount: 1, getCaseAsyncInvokeCount: 1);
            });
        }
        #endregion

        #region Helper methods
        private INotifyScenario ArrangeTaskScenario(DistributionChannels testDistributionChannel, TaskObject testTask)
        {
            this._mockedQueryContext
                .Setup(mock => mock.IsValidType())
                .Returns(true);
            this._mockedQueryContext
                .Setup(mock => mock.GetTaskAsync())
                .ReturnsAsync(testTask);
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new CommonPartyData
                {
                    DistributionChannel = testDistributionChannel
                });
            this._mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new Case());

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            return new TaskAssignedScenario(s_testConfiguration, this._mockedDataQuery.Object);
        }
        #endregion

        #region Verify
        private void VerifyInvoke(int getPartyDataAsyncInvokeCount, int getCaseAsyncInvokeCount)
        {
            this._mockedDataQuery.Verify(mock =>
                    mock.From(It.IsAny<NotificationEvent>()),
                Times.Once);

            this._mockedQueryContext.Verify(
                mock => mock.GetPartyDataAsync(It.IsAny<string>()),
                Times.Exactly(getPartyDataAsyncInvokeCount));

            this._mockedQueryContext.Verify(
                mock => mock.GetCaseAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCaseAsyncInvokeCount));
        }
        #endregion
    }
}