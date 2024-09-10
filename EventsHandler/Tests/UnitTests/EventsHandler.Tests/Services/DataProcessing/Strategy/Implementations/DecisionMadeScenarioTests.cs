// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Enums.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
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
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.UnitTests.Services.DataProcessing.Strategy.Implementations
{
    [TestFixture]
    public sealed class DecisionMadeScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotificationEvent, NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

        private WebApiConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.BothValid_v1);
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
        private static readonly Uri s_validUri =
            new($"https://www.domain.com/{ConfigurationHandler.TestMessageObjectTypeUuid2}");  // NOTE: Matches to UUID from test Environment Configuration
        
        private static readonly InfoObject s_invalidInfoObjectType = new()
        {
            TypeUri = DefaultValues.Models.EmptyUri
        };

        private static readonly InfoObject s_invalidInfoObjectStatus = new()
        {
            Status = MessageStatus.Unknown,
            TypeUri = s_validUri
        };

        private static readonly InfoObject s_invalidInfoObjectConfidentiality = new()
        {
            Confidentiality = PrivacyNotices.Confidential,
            Status = MessageStatus.Definitive,
            TypeUri = s_validUri
        };

        private static readonly InfoObject s_validInfoObject = new()
        {
            Confidentiality = PrivacyNotices.NonConfidential,
            Status = MessageStatus.Definitive,
            TypeUri = s_validUri
        };

        private const string TestEmailAddress = "test@email.com";
        private const string TestPhoneNumber = "911";
        private const string CaseId = "ZAAK-2024-00000000001";
        #endregion

        #region TryGetDataAsync()
        [Test]
        public void TryGetDataAsync_InvalidMessageType_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_invalidInfoObjectType, true, true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_MessageType
                                              .Replace("{0}", "USER_WHITELIST_MESSAGEOBJECTTYPE_UUIDS")), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 1, 1, 0, 0, 0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidMessageType_InvalidStatus_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_invalidInfoObjectStatus, true, true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_DecisionStatus), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyGetDataMethodCalls(1, 1, 1, 0, 0, 0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidMessageType_ValidStatus_InvalidConfidentiality_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_invalidInfoObjectConfidentiality, true, true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));

                string expectedMessage = Resources.Processing_ABORT_DoNotSendNotification_DecisionConfidentiality
                    .Replace("{0}", s_invalidInfoObjectConfidentiality.Confidentiality.ToString());

                Assert.That(exception?.Message.StartsWith(expectedMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(1, 1, 1, 0, 0, 0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidMessageType_ValidStatus_ValidConfidentiality_NotWhitelistedCaseId_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_validInfoObject, false, true);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));

                string expectedErrorMessage = Resources.Processing_ABORT_DoNotSendNotification_CaseTypeIdWhitelist
                    .Replace("{0}", "4")
                    .Replace("{1}", "USER_WHITELIST_DECISIONMADE_IDS");

                Assert.That(exception?.Message.StartsWith(expectedErrorMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1, 0, 0, 0);
            });
        }

        [Test]
        public void TryGetDataAsync_ValidMessageType_ValidStatus_ValidConfidentiality_WhitelistedCaseId_InformSetToFalse_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_validInfoObject, true, false);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_Informeren), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1, 0, 0, 0);
            });
        }

        [TestCase(DistributionChannels.None)]
        [TestCase(DistributionChannels.Unknown)]
        [TestCase((DistributionChannels)(-1))]
        public async Task TryGetDataAsync_ValidMessageType_ValidStatus_ValidConfidentiality_WhitelistedCaseId_InformSetToTrue_WithInvalidNotifyMethod_ReturnsFailure(
            DistributionChannels testDistributionChannel)
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_validInfoObject, true, true, testDistributionChannel);

            // Act
            GettingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1, 1, 0, 0);
            });
        }

        [TestCase(DistributionChannels.Email, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(DistributionChannels.Sms, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(DistributionChannels.Both, null, 2, TestEmailAddress + TestPhoneNumber)]
        public async Task TryGetDataAsync_ValidMessageType_ValidStatus_ValidConfidentiality_WhitelistedCaseId_InformSetToTrue_WithInvalidNotifyMethod_ReturnsSuccess(
            DistributionChannels testDistributionChannel, NotifyMethods? expectedNotificationMethod, int notifyDataCount, string expectedContactDetails)
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_validInfoObject, true, true, testDistributionChannel);

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
                    Assert.That(firstResult.TemplateId, Is.EqualTo(Guid.Empty));

                    NotifyData secondResult = actualResult.Content.Last();
                    Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                    Assert.That(secondResult.TemplateId, Is.EqualTo(Guid.Empty));

                    contactDetails = firstResult.ContactDetails + secondResult.ContactDetails;
                }
                else
                {
                    NotifyData onlyResult = actualResult.Content.First();
                    Assert.That(onlyResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod!.Value));
                    Assert.That(onlyResult.TemplateId, Is.EqualTo(Guid.Empty));

                    contactDetails = onlyResult.ContactDetails;
                }

                Assert.That(contactDetails, Is.EqualTo(expectedContactDetails));
                
                int invocationCounts = testDistributionChannel == DistributionChannels.Both ? 2 : 1;

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1, 1, invocationCounts, invocationCounts);
            });
        }
        #endregion

        // TODO: Add GetPersonalization tests

        #region ProcessDataAsync()
        [Test]
        public async Task ProcessDataAsync_EmptyNotifyData_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_ProcessData(true, true, true);

            NotifyData[] emptyNotifyData = Array.Empty<NotifyData>();

            // Act
            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, emptyNotifyData);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_MissingNotifyData));

                VerifyProcessDataMethodCalls(0, 0, 0, 0, 0);
            });
        }

        [Test]
        public async Task ProcessDataAsync_ValidNotifyData_GenerationFailed_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_ProcessData(false, true, true);

            // Act
            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, GetNotifyData());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(TestGenerationError));

                VerifyProcessDataMethodCalls(0, 1, 0, 0, 0);
            });
        }

        [Test]
        public async Task ProcessDataAsync_ValidNotifyData_GenerationSucceeded_MissingDocuments_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_ProcessData(true, false, true);

            // Act
            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, GetNotifyData());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_ERROR_Scenario_MissingInfoObjectsURIs));

                VerifyProcessDataMethodCalls(1, 1, 1, 0, 0);
            });
        }

        [Test]
        public async Task ProcessDataAsync_ValidNotifyData_GenerationSucceeded_HasDocuments_CreationFailed_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_ProcessData(true, true, false);

            // Act
            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, GetNotifyData());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(TestCreationError));

                VerifyProcessDataMethodCalls(1, 1, 1, 5, 1);
            });
        }

        [Test]
        public async Task ProcessDataAsync_ValidNotifyData_GenerationSucceeded_HasDocuments_CreationSucceeded_ReturnsSuccess()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_ProcessData(true, true, true);

            // Act
            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, GetNotifyData());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(Resources.Processing_SUCCESS_Scenario_DataProcessed));

                VerifyProcessDataMethodCalls(1, 1, 1, 5, 1);
            });
        }
        #endregion

        #region Setup
        private INotifyScenario ArrangeDecisionScenario_TryGetData(
            InfoObject testInfoObject, bool isCaseTypeIdWhitelisted, bool isNotificationExpected, DistributionChannels testDistributionChannel = DistributionChannels.Email)
        {
            // IQueryContext
            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionResourceAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new DecisionResource());

            this._mockedQueryContext
                .Setup(mock => mock.GetInfoObjectAsync(It.IsAny<object?>()))
                .ReturnsAsync(testInfoObject);

            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionAsync(It.IsAny<DecisionResource?>()))
                .ReturnsAsync(new Decision());

            this._mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()))
                .ReturnsAsync(new CaseType
                {
                    Identification = isCaseTypeIdWhitelisted ? "1" : "4",
                    IsNotificationExpected = isNotificationExpected
                });

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses());

            this._mockedQueryContext
                .Setup(mock => mock.GetBsnNumberAsync(It.IsAny<Uri>()))
                .ReturnsAsync(string.Empty);

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string?>()))
                .ReturnsAsync(new CommonPartyData
                {
                    Name = "Jackie",
                    Surname = "Chan",
                    DistributionChannel = testDistributionChannel,
                    EmailAddress = TestEmailAddress,
                    TelephoneNumber = TestPhoneNumber
                });

            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionTypeAsync(It.IsAny<Decision?>()))
                .ReturnsAsync(new DecisionType());

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new Case
                {
                    Identification = CaseId
                });

            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Decision Scenario
            return new DecisionMadeScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }

        private const string TestSubject = "Test subject";
        private const string TestBody = "Test body";
        private const string TestGenerationError = "Test generation error";
        private const string TestCreationError = "Test creation error";
        private const string TestJsonResponse = "{ \"IsValid\": \"OK\" }";
        private static readonly Uri s_firstDocumentObjectUri = new($"https://www.test1.nl/{new Guid()}");
        private static readonly Uri s_secondDocumentObjectUri = new($"https://www.test2.nl/{new Guid()}");
        private static readonly Uri s_thirdDocumentObjectUri = new($"https://www.test3.nl/{new Guid()}");
        private static readonly Uri s_fourthDocumentObjectUri = new($"https://www.test4.nl/{new Guid()}");
        private static readonly Uri s_fifthDocumentObjectUri = new($"https://www.test5.nl/{new Guid()}");

        private INotifyScenario ArrangeDecisionScenario_ProcessData(bool isGenerateSuccessful, bool hasValidUris, bool isCreationSuccessful)
        {
            // INotifyService
            this._mockedNotifyService
                .Setup(mock => mock.GenerateTemplatePreviewAsync(
                    It.IsAny<NotificationEvent>(), It.IsAny<NotifyData>()))
                .ReturnsAsync(isGenerateSuccessful
                    ? NotifyTemplateResponse.Success(TestSubject, TestBody)
                    : NotifyTemplateResponse.Failure(TestGenerationError));

            // IQueryContext
            (Document Document, InfoObject InfoObject)[] testData =
            {
                (new Document { InfoObjectUri = s_firstDocumentObjectUri  }, new InfoObject { Status = MessageStatus.Definitive, Confidentiality = PrivacyNotices.NonConfidential }),  // Valid InfoObject
                (new Document { InfoObjectUri = s_secondDocumentObjectUri }, new InfoObject { Status = MessageStatus.Definitive, Confidentiality = PrivacyNotices.NonConfidential }),  // Valid InfoObject
                (new Document { InfoObjectUri = s_thirdDocumentObjectUri  }, new InfoObject { Status = MessageStatus.Unknown,    Confidentiality = PrivacyNotices.NonConfidential }),  // Invalid InfoObject
                (new Document { InfoObjectUri = s_fourthDocumentObjectUri }, new InfoObject { Status = MessageStatus.Definitive, Confidentiality = PrivacyNotices.Confidential }),     // Invalid InfoObject
                (new Document { InfoObjectUri = s_fifthDocumentObjectUri  }, new InfoObject { Status = MessageStatus.Unknown,    Confidentiality = PrivacyNotices.Confidential })      // Invalid InfoObject
            };

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentsAsync(It.IsAny<DecisionResource?>()))
                .ReturnsAsync(hasValidUris
                    ? new Documents
                    {
                        Results = testData.Select(data => data.Document).ToList()
                    }
                    : new Documents());

            if (hasValidUris)
            {
                foreach ((Document Document, InfoObject InfoObject) data in testData)
                {
                    this._mockedQueryContext
                        .Setup(mock => mock.GetInfoObjectAsync(data.Document))
                        .ReturnsAsync(data.InfoObject);
                }
            }

            this._mockedQueryContext
                .Setup(mock => mock.CreateMessageObjectAsync(It.IsAny<string>()))
                .ReturnsAsync(isCreationSuccessful
                    ? RequestResponse.Success(TestJsonResponse)
                    : RequestResponse.Failure(TestCreationError));

            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Decision Scenario
            return new DecisionMadeScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }

        private static IReadOnlyCollection<NotifyData> GetNotifyData(Dictionary<string, object>? personalization = null)
        {
            return new[]
            {
                new NotifyData(
                    NotifyMethods.Email,
                    TestEmailAddress,
                    Guid.NewGuid(),
                    personalization ?? new Dictionary<string, object>())
            };
        }
        #endregion

        #region Verify
        private bool _getDataVerified;
        private bool _processDataVerified;

        private void VerifyGetDataMethodCalls(
            int fromInvokeCount, int getDecisionResInvokeCount, int getInfoObjectInvokeCount, int getDecisionInvokeCount,
            int getCaseTypeInvokeCount, int getCitizenDetailsInvokeCount, int getDecisionTypeInvokeCount, int getCaseInvokeCount)
        {
            if (this._getDataVerified)
            {
                return;
            }
            
            // IDataQueryService
            this._mockedDataQuery
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()),
                Times.Exactly(fromInvokeCount));
            
            // IQueryContext
            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionResourceAsync(It.IsAny<Uri?>()),
                Times.Exactly(getDecisionResInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetInfoObjectAsync(It.IsAny<object?>()),
                Times.Exactly(getInfoObjectInvokeCount));
          
            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionAsync(It.IsAny<DecisionResource?>()),
                Times.Exactly(getDecisionInvokeCount));

            this._mockedQueryContext  // Dependent queries
                .Verify(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()),
                Times.Exactly(getCaseTypeInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCaseTypeInvokeCount));
          
            this._mockedQueryContext  // Dependent + consecutive queries
                .Verify(mock => mock.GetBsnNumberAsync(It.IsAny<Uri>()),
                Times.Exactly(getCitizenDetailsInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(It.IsAny<string?>()),
                Times.Exactly(getCitizenDetailsInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionTypeAsync(It.IsAny<Decision?>()),
                Times.Exactly(getDecisionTypeInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCaseInvokeCount));

            this._getDataVerified = true;

            VerifyProcessDataMethodCalls(0, 0, 0, 1, 0);
        }

        private void VerifyProcessDataMethodCalls(int fromInvokeCount, int generateInvokeCount,
            int getDocumentsInvokeCount, int getInfoObjectInvokeCount, int createInvokeCount)
        {
            if (this._processDataVerified)
            {
                return;
            }

            // INotifyService
            this._mockedNotifyService
                .Verify(mock => mock.GenerateTemplatePreviewAsync(
                    It.IsAny<NotificationEvent>(), It.IsAny<NotifyData>()),
                Times.Exactly(generateInvokeCount));

            // IQueryContext
            this._mockedQueryContext
                .Verify(mock => mock.GetDocumentsAsync(It.IsAny<DecisionResource?>()),
                Times.Exactly(getDocumentsInvokeCount));
            
            this._mockedQueryContext
                .Verify(mock => mock.GetInfoObjectAsync(It.IsAny<object?>()),
                Times.Exactly(getInfoObjectInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.CreateMessageObjectAsync(It.IsAny<string>()),
                Times.Exactly(createInvokeCount));

            this._processDataVerified = true;

            VerifyGetDataMethodCalls(fromInvokeCount, 0, getInfoObjectInvokeCount, 0, 0, 0, 0, 0);
        }
        #endregion
    }
}