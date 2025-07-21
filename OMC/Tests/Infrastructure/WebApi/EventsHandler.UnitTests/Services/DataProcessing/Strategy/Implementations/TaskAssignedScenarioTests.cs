﻿// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Models.Responses;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using Moq;
using System.Text.Json;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Properties;
using ZhvModels.Enums;
using ZhvModels.Mapping.Enums.Objecten;
using ZhvModels.Mapping.Enums.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Tests.Unit.Services.DataProcessing.Strategy.Implementations
{
    [TestFixture]
    public sealed class TaskAssignedScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

        private OmcConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);
        }

        [TearDown]
        public void TestsReset()
        {
            this._mockedDataQuery.Reset();
            this._mockedQueryContext.Reset();
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
        private static readonly NotificationEvent s_validNotification = new()
        {
            Attributes = new EventAttributes
            {
                ObjectTypeUri = new Uri($"http://www.domain.com/{ConfigurationHandler.TestTaskObjectTypeUuid}")
            }
        };

        private static readonly CommonTaskData s_taskClosed = new()
        {
            Status = TaskStatuses.Closed
        };

        private static readonly CommonTaskData s_taskOpenNotAssignedToAnyone = new()
        {
            Status = TaskStatuses.Open,
            Identification = new Identification
            {
                Type = IdTypes.Unknown
            }
        };

        private const string TestTaskTitle = "Test title";

        // Tasks assigned to someone with expiration date
        private static readonly CommonTaskData s_taskOpenAssignedToPersonWithoutExpirationDate = new()
        {
            Title = TestTaskTitle,
            Status = TaskStatuses.Open,
            // Missing "ExpirationDate" => default
            Identification = new Identification
            {
                Type = IdTypes.Bsn
            }
        };
        private static readonly CommonTaskData s_taskOpenAssignedToOrganizationWithoutExpirationDate = new()
        {
            Title = TestTaskTitle,
            Status = TaskStatuses.Open,
            // Missing "ExpirationDate" => default
            Identification = new Identification
            {
                Type = IdTypes.Kvk
            }
        };

        // Tasks assigned to someone without expiration date
        private static readonly CommonTaskData s_taskOpenAssignedToPersonWithExpirationDate = new()
        {
            Title = TestTaskTitle,
            Status = TaskStatuses.Open,
            ExpirationDate = new DateTime(2024, 7, 24, 14, 10, 40, DateTimeKind.Utc),
            Identification = new Identification
            {
                Type = IdTypes.Bsn
            }
        };
        private static readonly CommonTaskData s_taskOpenAssignedToOrganizationWithExpirationDate = new()
        {
            Title = TestTaskTitle,
            Status = TaskStatuses.Open,
            ExpirationDate = new DateTime(2024, 7, 24, 14, 10, 40, DateTimeKind.Utc),
            Identification = new Identification
            {
                Type = IdTypes.Kvk
            }
        };

        private const string TestEmailAddress = "test@email.com";
        private const string TestPhoneNumber = "911";
        private const string CaseId = "ZAAK-2024-00000000001";
        #endregion

        #region TryGetDataAsync()
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
                Assert.That(exception?.Message.StartsWith(ApiResources.Processing_ABORT_DoNotSendNotification_TaskClosed), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 1, 0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidTaskType_Open_NotAssignedToAnyone_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                s_taskOpenNotAssignedToAnyone,
                true,
                true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));
                Assert.That(exception?.Message.StartsWith(ApiResources.Processing_ABORT_DoNotSendNotification_TaskIdTypeNotSupported), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(1, 1, 0, 0, 0);
            });
        }

        [TestCase(IdTypes.Bsn)]  // Person
        [TestCase(IdTypes.Kvk)]  // Organization
        public void TryGetDataAsync_ValidTaskType_Open_AssignedToEntity_NotWhitelisted_ThrowsAbortedNotifyingException(IdTypes idType)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                testTask: GetOpenAssignedTaskWithExpirationDate(idType),
                isCaseTypeIdWhitelisted: false,
                isNotificationExpected: true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));

                string expectedErrorMessage = ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_CaseTypeId
                    .Replace("{0}", "4")
                    .Replace("{1}", "ZGW_WHITELIST_TASKASSIGNED_IDS");

                Assert.That(exception?.Message.StartsWith(expectedErrorMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 1, 1, 0, 0);
            });
        }
        
        [TestCase(IdTypes.Bsn)]  // Person
        [TestCase(IdTypes.Kvk)]  // Organization
        public void TryGetDataAsync_ValidTaskType_Open_AssignedToEntity_Whitelisted_InformSetToFalse_ThrowsAbortedNotifyingException(IdTypes idType)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                DistributionChannels.Email,
                testTask: GetOpenAssignedTaskWithExpirationDate(idType),
                isCaseTypeIdWhitelisted: true,
                isNotificationExpected: false);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));
                Assert.That(exception?.Message.StartsWith(ApiResources.Processing_ABORT_DoNotSendNotification_Informeren), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 1, 1, 0, 0);
            });
        }

        // Person
        [TestCase(DistributionChannels.None, IdTypes.Bsn)]
        [TestCase(DistributionChannels.Unknown, IdTypes.Bsn)]
        [TestCase((DistributionChannels)(-1), IdTypes.Bsn)]
        // Organization
        [TestCase(DistributionChannels.None, IdTypes.Kvk)]
        [TestCase(DistributionChannels.Unknown, IdTypes.Kvk)]
        [TestCase((DistributionChannels)(-1), IdTypes.Kvk)]
        public async Task TryGetDataAsync_ValidTaskType_Open_AssignedToEntity_Whitelisted_InformSetToTrue_WithInvalidDistChannel_ReturnsFailure(
            DistributionChannels invalidDistributionChannel, IdTypes idType)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                invalidDistributionChannel,
                testTask: GetOpenAssignedTaskWithExpirationDate(idType),
                true,
                true);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(QueryResources.Response_QueryingData_ERROR_NotificationMethodMissing));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1);
            });
        }

        // Person
        [TestCase(DistributionChannels.Email, IdTypes.Bsn, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(DistributionChannels.Sms, IdTypes.Bsn, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(DistributionChannels.Letter, IdTypes.Bsn, NotifyMethods.Letter, 1, "")]
        [TestCase(DistributionChannels.Both, IdTypes.Bsn, null, 2, TestEmailAddress + TestPhoneNumber)]
        // Organization
        [TestCase(DistributionChannels.Email, IdTypes.Kvk, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(DistributionChannels.Sms, IdTypes.Kvk, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(DistributionChannels.Letter, IdTypes.Kvk, NotifyMethods.Letter, 1, "")]
        [TestCase(DistributionChannels.Both, IdTypes.Kvk, null, 2, TestEmailAddress + TestPhoneNumber)]
        public async Task TryGetDataAsync_ValidTaskType_Open_AssignedToEntity_Whitelisted_InformSetToTrue_WithValidDistChannels_ReturnsSuccess(
            DistributionChannels testDistributionChannel, IdTypes idType, NotifyMethods? expectedNotificationMethod, int notifyDataCount, string expectedContactDetails)
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                testDistributionChannel,
                testTask: GetOpenAssignedTaskWithExpirationDate(idType),
                isCaseTypeIdWhitelisted: true,
                isNotificationExpected: true);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(QueryResources.Response_QueryingData_SUCCESS_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(notifyDataCount));

                string contactDetails;

                if (testDistributionChannel == DistributionChannels.Both)
                {
                    NotifyData firstResult = actualResult.Content.First();
                    Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                    Assert.That(firstResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(firstResult.NotificationMethod, this._testConfiguration)));

                    NotifyData secondResult = actualResult.Content.Last();
                    Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                    Assert.That(secondResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(firstResult.NotificationMethod, this._testConfiguration)));

                    contactDetails = firstResult.ContactDetails + secondResult.ContactDetails;
                }
                else
                {
                    NotifyData onlyResult = actualResult.Content.First();
                    Assert.That(onlyResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod!.Value));
                    Assert.That(onlyResult.TemplateId, Is.EqualTo(
                        DetermineTemplateId(onlyResult.NotificationMethod, this._testConfiguration)));

                    contactDetails = onlyResult.ContactDetails;
                }

                Assert.That(contactDetails, Is.EqualTo(expectedContactDetails));

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1);
            });
        }
        #endregion

        #region GetPersonalizationAsync()
        // Person
        [TestCase(DistributionChannels.Email, IdTypes.Bsn, false, "-", "no")]
        [TestCase(DistributionChannels.Email, IdTypes.Bsn, true, "24-07-2024", "yes")]
        [TestCase(DistributionChannels.Sms, IdTypes.Bsn, false, "-", "no")]
        [TestCase(DistributionChannels.Sms, IdTypes.Bsn, true, "24-07-2024", "yes")]
        [TestCase(DistributionChannels.Letter, IdTypes.Bsn, false, "-", "no")]
        [TestCase(DistributionChannels.Letter, IdTypes.Bsn, true, "24-07-2024", "yes")]
        // Organization
        [TestCase(DistributionChannels.Email, IdTypes.Kvk, false, "-", "no")]
        [TestCase(DistributionChannels.Email, IdTypes.Kvk, true, "24-07-2024", "yes")]
        [TestCase(DistributionChannels.Sms, IdTypes.Kvk, false, "-", "no")]
        [TestCase(DistributionChannels.Sms, IdTypes.Kvk, true, "24-07-2024", "yes")]
        [TestCase(DistributionChannels.Letter, IdTypes.Kvk, false, "-", "no")]
        [TestCase(DistributionChannels.Letter, IdTypes.Kvk, true, "24-07-2024", "yes")]
        public async Task GetPersonalizationAsync_SpecificDateTime_ReturnsExpectedPersonalization(
            DistributionChannels testDistributionChannel, IdTypes idType, bool isExpirationDateGiven, string testExpirationDate, string isExpirationDateGivenText)
        {
            CommonTaskData testTaskData = isExpirationDateGiven
                ? GetOpenAssignedTaskWithExpirationDate(idType)
                : GetOpenAssignedTaskWithoutExpirationDate(idType);

            INotifyScenario scenario = ArrangeTaskScenario_TryGetData(
                testDistributionChannel,
                testTaskData,
                isCaseTypeIdWhitelisted: true,
                isNotificationExpected: true);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(QueryResources.Response_QueryingData_SUCCESS_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(1));

                string actualSerializedPersonalization = JsonSerializer.Serialize(actualResult.Content.First().Personalization);
                string expectedSerializedPersonalization =
                    $"{{" +
                      $"\"klant.voornaam\":\"Jackie\"," +
                      $"\"klant.voorvoegselAchternaam\":null," +
                      $"\"klant.achternaam\":\"Chan\","+
                      (testDistributionChannel == DistributionChannels.Letter ?
                          $"\"klant.street\":null," +
                          $"\"klant.number\":null," +
                          $"\"klant.zip\":null," +
                          $"\"klant.city\":null," +
                          $"\"klant.country\":null,"
                          : "") +

                      $"\"taak.verloopdatum\":\"{testExpirationDate}\"," +
                      $"\"taak.heeft_verloopdatum\":\"{isExpirationDateGivenText}\"," +
                      $"\"taak.record.data.title\":\"{TestTaskTitle}\"," +

                      $"\"zaak.identificatie\":\"{CaseId}\"," +
                      $"\"zaak.omschrijving\":\"\"" +
                    $"}}";

                Assert.That(actualSerializedPersonalization, Is.EqualTo(expectedSerializedPersonalization));

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1);
            });
        }
        #endregion

        #region ProcessDataAsync()
        [Test]
        public async Task ProcessDataAsync_EmptyNotifyData_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(true);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsFailure, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(ApiResources.Processing_ERROR_Scenario_MissingNotifyData));

                VerifyProcessDataMethodCalls(0, 0, 0);
            });
        }

        [TestCase(NotifyMethods.None)]
        [TestCase((NotifyMethods)(-1))]
        public async Task ProcessDataAsync_ValidNotifyData_InvalidNotifyMethod_ReturnsFailure(NotifyMethods invalidNotifyMethod)
        {
            // Arrange
            NotifyData testData = new(invalidNotifyMethod);

            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(
                isSendingSuccessful: false,
                testData);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, [testData]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsFailure, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(QueryResources.Response_ProcessingData_ERROR_DeliveryMethodUnknown));

                VerifyProcessDataMethodCalls(0, 0, 0);
            });
        }

        [TestCase(NotifyMethods.Email, 1, 0, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1, 0)]
        [TestCase(NotifyMethods.Letter, 0, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingFailed_ReturnsFailure(
            NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount, int sendLetterInvokeCount)
        {
            // Arrange
            NotifyData testData = new(testNotifyMethod);

            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(
                isSendingSuccessful: false,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null,
                letterNotifyData:    testNotifyMethod == NotifyMethods.Letter? testData : null);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, [testData]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsFailure, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(SimulatedNotifyExceptionMessage));

                VerifyProcessDataMethodCalls(sendEmailInvokeCount, sendSmsInvokeCount, sendLetterInvokeCount);
            });
        }

        [TestCase(NotifyMethods.Email, 1, 0, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1, 0)]
        [TestCase(NotifyMethods.Letter, 0, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_ValidNotifyMethod_SendingSuccessful_ReturnsSuccess(
            NotifyMethods testNotifyMethod, int sendEmailInvokeCount, int sendSmsInvokeCount, int sendLetterInvokeCount)
        {
            // Arrange
            NotifyData testData = new(testNotifyMethod);

            INotifyScenario scenario = ArrangeTaskScenario_ProcessData(
                isSendingSuccessful: true,
                emailNotifyData:     testNotifyMethod == NotifyMethods.Email ? testData : null,
                smsNotifyData:       testNotifyMethod == NotifyMethods.Sms   ? testData : null,
                letterNotifyData:    testNotifyMethod == NotifyMethods.Letter ? testData : null);

            // Act
            ProcessingDataResponse actualResponse = await scenario.ProcessDataAsync(default, [testData]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.IsSuccess, Is.True);
                Assert.That(actualResponse.Message, Is.EqualTo(ApiResources.Processing_SUCCESS_Scenario_DataProcessed));
                
                VerifyProcessDataMethodCalls(sendEmailInvokeCount, sendSmsInvokeCount, sendLetterInvokeCount);
            });
        }
        #endregion

        #region Setup
        private TaskAssignedScenario ArrangeTaskScenario_TryGetData(
            DistributionChannels testDistributionChannel, CommonTaskData testTask, bool isCaseTypeIdWhitelisted, bool isNotificationExpected)
        {
            // IQueryContext
            this._mockedQueryContext
                .Setup(mock => mock.GetTaskAsync())
                .ReturnsAsync(testTask);

            this._mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(
                    It.IsAny<CaseStatuses?>()))
                .ReturnsAsync(new CaseType
                {
                    Identification = isCaseTypeIdWhitelisted ? "1" : "4",
                    IsNotificationExpected = isNotificationExpected
                });

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(
                    It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses());

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(
                    It.IsAny<Uri?>(),
                    It.IsAny<string?>(), 
                    It.IsAny<string?>()))
                .ReturnsAsync(new CommonPartyData
                {
                    Name = "Jackie",
                    Surname = "Chan",
                    DistributionChannel = testDistributionChannel,
                    EmailAddress = TestEmailAddress,
                    TelephoneNumber = TestPhoneNumber
                });

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(
                    It.IsAny<Uri?>()))
                .ReturnsAsync(new Case
                {
                    Identification = CaseId
                });

            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(
                    It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Task Scenario
            return new TaskAssignedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }

        private const string SimulatedNotifyExceptionMessage = "Some NotifyClientException";

        private TaskAssignedScenario ArrangeTaskScenario_ProcessData(
            bool isSendingSuccessful, NotifyData? emailNotifyData = default, NotifyData? smsNotifyData = default, NotifyData? letterNotifyData = default)
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

            this._mockedNotifyService.Setup(mock => mock.SendLetterAsync(letterNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            // Task Scenario
            return new TaskAssignedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }

        private static Guid DetermineTemplateId(NotifyMethods notifyMethod, OmcConfiguration configuration)
        {
            return notifyMethod switch
            {
                NotifyMethods.Email => configuration.Notify.TemplateId.Email.TaskAssigned(),
                NotifyMethods.Sms => configuration.Notify.TemplateId.Sms.TaskAssigned(),
                NotifyMethods.Letter => configuration.Notify.TemplateId.Letter.TaskAssigned(),
                _ => Guid.Empty
            };
        }
        #endregion

        #region Verify
        private bool _getDataVerified;
        private bool _processDataVerified;

        private void VerifyGetDataMethodCalls(
            int fromInvokeCount, int getTaskInvokeCount, int getCaseTypeInvokeCount, int getPartyDataInvokeCount, int getCaseInvokeCount)
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
                .Verify(mock => mock.GetTaskAsync(),
                Times.Exactly(getTaskInvokeCount));

            this._mockedQueryContext  // Dependent queries
                .Verify(mock => mock.GetLastCaseTypeAsync(
                    It.IsAny<CaseStatuses?>()),
                Times.Exactly(getCaseTypeInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(
                    It.IsAny<Uri?>()),
                Times.Exactly(getCaseTypeInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(
                    It.IsAny<Uri?>(),
                    It.IsAny<string?>(), 
                    It.IsAny<string?>()),
                Times.Exactly(getPartyDataInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(
                    It.IsAny<Uri?>()),
                Times.Exactly(getCaseInvokeCount));
            
            this._getDataVerified = true;

            VerifyProcessDataMethodCalls(0, 0, 0);
        }

        private void VerifyProcessDataMethodCalls(int sendEmailInvokeCount, int sendSmsInvokeCount, int sendLetterInvokeCount)
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

            this._mockedNotifyService
                .Verify(mock => mock.SendLetterAsync(
                        It.IsAny<NotifyData>()),
                    Times.Exactly(sendLetterInvokeCount));

            this._processDataVerified = true;

            VerifyGetDataMethodCalls(0, 0, 0, 0, 0);
        }
        #endregion

        #region Helper methods
        private static CommonTaskData GetOpenAssignedTaskWithExpirationDate(IdTypes idType)
        {
            return idType == IdTypes.Bsn ? s_taskOpenAssignedToPersonWithExpirationDate :
                   idType == IdTypes.Kvk ? s_taskOpenAssignedToOrganizationWithExpirationDate
                                         : default;
        }

        private static CommonTaskData GetOpenAssignedTaskWithoutExpirationDate(IdTypes idType)
        {
            return idType == IdTypes.Bsn ? s_taskOpenAssignedToPersonWithoutExpirationDate :
                   idType == IdTypes.Kvk ? s_taskOpenAssignedToOrganizationWithoutExpirationDate
                                         : default;
        }
        #endregion
    }
}