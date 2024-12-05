// © 2024, Worth Systems.

using Common.Constants;
using Common.Enums.Processing;
using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Exceptions;
using EventsHandler.Models.DTOs.Processing;
using EventsHandler.Models.Responses.Processing;
using EventsHandler.Models.Responses.Querying;
using EventsHandler.Models.Responses.Sending;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Proxy.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using Moq;
using System.Text.Json;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Enums.OpenKlant;
using ZhvModels.Mapping.Enums.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using Decision = ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.Decision;
using DecisionResource = ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.DecisionResource;
using DecisionType = ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.DecisionType;
using Document = ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.Document;
using Documents = ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.Documents;

namespace EventsHandler.Tests.Unit.Services.DataProcessing.Strategy.Implementations
{
    [TestFixture]
    public sealed class DecisionMadeScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

        private WebApiConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.BothValid_v1);
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
            new($"https://www.domain.com/{ConfigurationHandler.TestInfoObjectTypeUuid2}");  // NOTE: Matches to UUID from test Environment Configuration
        
        private static readonly ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject s_invalidInfoObjectType = new()
        {
            TypeUri = CommonValues.Default.Models.EmptyUri
        };

        private static readonly ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject s_invalidInfoObjectStatus = new()
        {
            Status = MessageStatus.Unknown,
            TypeUri = s_validUri
        };

        private static readonly ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject s_invalidInfoObjectConfidentiality = new()
        {
            Confidentiality = PrivacyNotices.Confidential,
            Status = MessageStatus.Definitive,
            TypeUri = s_validUri
        };

        private static readonly ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject s_validInfoObject = new()
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
                Assert.That(exception?.Message.StartsWith(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_InfoObjectType
                                              .Replace("{0}", $"{s_invalidInfoObjectType.TypeUri.GetGuid()}")
                                              .Replace("{1}", "ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS")), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);
                
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
                Assert.That(exception?.Message.StartsWith(ApiResources.Processing_ABORT_DoNotSendNotification_DecisionStatus), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);
                
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

                string expectedMessage = ApiResources.Processing_ABORT_DoNotSendNotification_DecisionConfidentiality
                    .Replace("{0}", s_invalidInfoObjectConfidentiality.Confidentiality.ToString());

                Assert.That(exception?.Message.StartsWith(expectedMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);

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

                string expectedErrorMessage = ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_CaseTypeId
                    .Replace("{0}", "4")
                    .Replace("{1}", "ZGW_WHITELIST_DECISIONMADE_IDS");

                Assert.That(exception?.Message.StartsWith(expectedErrorMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);

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
                Assert.That(exception?.Message.StartsWith(ApiResources.Processing_ABORT_DoNotSendNotification_Informeren), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);

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
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_ERROR_Scenario_NotificationMethod));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1, 1, 1, 1);
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
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(notifyDataCount));

                Guid expectedTemplateId = this._testConfiguration.Notify.TemplateId.DecisionMade();
                string contactDetails;

                if (testDistributionChannel == DistributionChannels.Both)
                {
                    NotifyData firstResult = actualResult.Content.First();
                    Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                    Assert.That(firstResult.TemplateId, Is.EqualTo(expectedTemplateId));

                    NotifyData secondResult = actualResult.Content.Last();
                    Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                    Assert.That(secondResult.TemplateId, Is.EqualTo(expectedTemplateId));

                    contactDetails = firstResult.ContactDetails + secondResult.ContactDetails;
                }
                else
                {
                    NotifyData onlyResult = actualResult.Content.First();
                    Assert.That(onlyResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod!.Value));
                    Assert.That(onlyResult.TemplateId, Is.EqualTo(expectedTemplateId));

                    contactDetails = onlyResult.ContactDetails;
                }

                Assert.That(contactDetails, Is.EqualTo(expectedContactDetails));

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1, 1, 1, 1);
            });
        }
        #endregion

        #region GetPersonalizationAsync()
        [TestCase(DistributionChannels.Email)]
        [TestCase(DistributionChannels.Sms)]
        public async Task GetPersonalizationAsync_ReturnsExpectedPersonalization(DistributionChannels testDistributionChannel)
        {
            INotifyScenario scenario = ArrangeDecisionScenario_TryGetData(
                s_validInfoObject,
                isCaseTypeIdWhitelisted: true,
                isNotificationExpected: true,
                testDistributionChannel: testDistributionChannel);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_SUCCESS_Scenario_DataRetrieved));
                Assert.That(actualResult.Content, Has.Count.EqualTo(1));

                string actualSerializedPersonalization = JsonSerializer.Serialize(actualResult.Content.First().Personalization);
                const string expectedSerializedPersonalization =
                    $"{{" +
                      $"\"klant.voornaam\":\"Jackie\"," +
                      $"\"klant.voorvoegselAchternaam\":null," +
                      $"\"klant.achternaam\":\"Chan\"," +
                      $"\"besluit.identificatie\":\"\"," +
                      $"\"besluit.datum\":\"01-01-0001\"," +
                      $"\"besluit.toelichting\":\"\"," +
                      $"\"besluit.bestuursorgaan\":\"\"," +
                      $"\"besluit.ingangsdatum\":\"01-01-0001\"," +
                      $"\"besluit.vervaldatum\":\"01-01-0001\"," +
                      $"\"besluit.vervalreden\":\"\"," +
                      $"\"besluit.publicatiedatum\":\"01-01-0001\"," +
                      $"\"besluit.verzenddatum\":\"01-01-0001\"," +
                      $"\"besluit.uiterlijkereactiedatum\":\"01-01-0001\"," +
                      $"\"besluittype.omschrijving\":\"\"," +
                      $"\"besluittype.omschrijvingGeneriek\":\"\"," +
                      $"\"besluittype.besluitcategorie\":\"\"," +
                      $"\"besluittype.publicatieindicatie\":false," +
                      $"\"besluittype.publicatietekst\":\"\"," +
                      $"\"besluittype.toelichting\":\"\"," +
                      $"\"zaak.identificatie\":\"ZAAK-2024-00000000001\"," +
                      $"\"zaak.omschrijving\":\"\"," +
                      $"\"zaak.registratiedatum\":\"01-01-0001\"," +
                      $"\"zaaktype.omschrijving\":\"\"," +
                      $"\"zaaktype.omschrijvingGeneriek\":\"\"" +
                    $"}}";

                Assert.That(actualSerializedPersonalization, Is.EqualTo(expectedSerializedPersonalization));

                VerifyGetDataMethodCalls(1, 1, 1, 1, 1, 1, 1, 1);
            });
        }
        #endregion

        #region ProcessDataAsync()
        [Test]
        public async Task ProcessDataAsync_EmptyNotifyData_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario_ProcessData(true, true, true);

            NotifyData[] emptyNotifyData = [];

            // Act
            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, emptyNotifyData);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_ERROR_Scenario_MissingNotifyData));

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
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_ERROR_Scenario_MissingInfoObjectsURIs));

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
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_SUCCESS_Scenario_DataProcessed));

                VerifyProcessDataMethodCalls(1, 1, 1, 5, 1);
            });
        }
        #endregion

        #region ReplaceWhitespaces()
        [TestCase(1, "Hello\\n\\n")]
        [TestCase(2, "Hello\\n\n")]
        [TestCase(3, "Hello\n\\n")]
        [TestCase(4, "Hello\n\n")]
        [TestCase(5, "Hello\\n")]
        [TestCase(6, "Hello\n")]
        [TestCase(7, "Hello\\r\\n")]
        [TestCase(8, "Hello\r\n")]
        [TestCase(9, "Hello\\r")]
        [TestCase(10, "Hello\r")]
        public void ReplaceWhitespaces_Newlines_ReturnsExpectedString(int testNumber, string testExample)
        {
            // Act
            string actualResult = DecisionMadeScenario.ReplaceWhitespaces(testExample);

            // Assert
            const string expectedResult = "Hello\\r\\n";

            Assert.That(actualResult, Is.EqualTo(expectedResult), $"{testNumber}");
        }

        [TestCase(1, "Hello\\t")]
        [TestCase(2, "Hello\t")]
        public void ReplaceWhitespaces_Tabs_ReturnsExpectedString(int testNumber, string testExample)
        {
            // Act
            string actualResult = DecisionMadeScenario.ReplaceWhitespaces(testExample);

            // Assert
            const string expectedResult = "Hello\\t";

            Assert.That(actualResult, Is.EqualTo(expectedResult), $"{testNumber}");
        }
        #endregion

        #region Setup
        private DecisionMadeScenario ArrangeDecisionScenario_TryGetData(
            ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject testInfoObject, bool isCaseTypeIdWhitelisted, bool isNotificationExpected, DistributionChannels testDistributionChannel = DistributionChannels.Email)
        {
            // IQueryContext
            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionResourceAsync(
                    It.IsAny<Uri?>()))
                .ReturnsAsync(new DecisionResource());

            this._mockedQueryContext
                .Setup(mock => mock.GetInfoObjectAsync(
                    It.IsAny<object?>()))
                .ReturnsAsync(testInfoObject);

            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionAsync(
                    It.IsAny<DecisionResource?>()))
                .ReturnsAsync(new Decision());

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
                .Setup(mock => mock.GetBsnNumberAsync(
                    It.IsAny<Uri>()))
                .ReturnsAsync(string.Empty);

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

            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionTypeAsync(
                    It.IsAny<Decision?>()))
                .ReturnsAsync(new DecisionType());

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

        private DecisionMadeScenario ArrangeDecisionScenario_ProcessData(bool isGenerateSuccessful, bool hasValidUris, bool isCreationSuccessful)
        {
            // INotifyService
            this._mockedNotifyService
                .Setup(mock => mock.GenerateTemplatePreviewAsync(It.IsAny<NotifyData>()))
                .ReturnsAsync(isGenerateSuccessful
                    ? NotifyTemplateResponse.Success(TestSubject, TestBody)
                    : NotifyTemplateResponse.Failure(TestGenerationError));

            // IQueryContext
            (Document Document, ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject InfoObject)[] testData =
            [
                (new Document { InfoObjectUri = s_firstDocumentObjectUri  }, new ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject { Status = MessageStatus.Definitive, Confidentiality = PrivacyNotices.NonConfidential }),  // Valid InfoObject
                (new Document { InfoObjectUri = s_secondDocumentObjectUri }, new ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject { Status = MessageStatus.Definitive, Confidentiality = PrivacyNotices.NonConfidential }),  // Valid InfoObject
                (new Document { InfoObjectUri = s_thirdDocumentObjectUri  }, new ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject { Status = MessageStatus.Unknown,    Confidentiality = PrivacyNotices.NonConfidential }),  // Invalid InfoObject
                (new Document { InfoObjectUri = s_fourthDocumentObjectUri }, new ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject { Status = MessageStatus.Definitive, Confidentiality = PrivacyNotices.Confidential }),     // Invalid InfoObject
                (new Document { InfoObjectUri = s_fifthDocumentObjectUri  }, new ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject { Status = MessageStatus.Unknown,    Confidentiality = PrivacyNotices.Confidential })      // Invalid InfoObject
            ];

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
                foreach ((Document Document, ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject InfoObject) data in testData)
                {
                    this._mockedQueryContext
                        .Setup(mock => mock.GetInfoObjectAsync(data.Document))
                        .ReturnsAsync(data.InfoObject);
                }
            }

            this._mockedQueryContext
                .Setup(mock => mock.PrepareObjectJsonBody(It.IsAny<string>()))
                .Returns(string.Empty);

            this._mockedQueryContext
                .Setup(mock => mock.CreateObjectAsync(It.IsAny<string>()))
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

        private static NotifyData[] GetNotifyData(Dictionary<string, object>? personalization = null)
        {
            return
            [
                new NotifyData(
                    NotifyMethods.Email,
                    TestEmailAddress,
                    Guid.NewGuid(),
                    personalization ?? [],
                    default)
            ];
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
                .Verify(mock => mock.From(
                    It.IsAny<NotificationEvent>()),
                Times.Exactly(fromInvokeCount));
            
            // IQueryContext
            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionResourceAsync(
                    It.IsAny<Uri?>()),
                Times.Exactly(getDecisionResInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetInfoObjectAsync(
                    It.IsAny<object?>()),
                Times.Exactly(getInfoObjectInvokeCount));
          
            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionAsync(
                    It.IsAny<DecisionResource?>()),
                Times.Exactly(getDecisionInvokeCount));

            this._mockedQueryContext  // Dependent queries
                .Verify(mock => mock.GetLastCaseTypeAsync(
                    It.IsAny<CaseStatuses?>()),
                Times.Exactly(getCaseTypeInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(
                    It.IsAny<Uri?>()),
                Times.Exactly(getCaseTypeInvokeCount));
          
            this._mockedQueryContext  // Dependent + consecutive queries
                .Verify(mock => mock.GetBsnNumberAsync(
                    It.IsAny<Uri>()),
                Times.Exactly(getCitizenDetailsInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(
                    It.IsAny<Uri?>(),
                    It.IsAny<string?>()),
                Times.Exactly(getCitizenDetailsInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionTypeAsync(
                    It.IsAny<Decision?>()),
                Times.Exactly(getDecisionTypeInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(
                    It.IsAny<Uri?>()),
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
                    It.IsAny<NotifyData>()),
                Times.Exactly(generateInvokeCount));

            // IQueryContext
            this._mockedQueryContext
                .Verify(mock => mock.GetDocumentsAsync(
                    It.IsAny<DecisionResource?>()),
                Times.Exactly(getDocumentsInvokeCount));
            
            this._mockedQueryContext
                .Verify(mock => mock.GetInfoObjectAsync(
                    It.IsAny<object?>()),
                Times.Exactly(getInfoObjectInvokeCount));

            this._mockedQueryContext  // Dependent queries
                .Verify(mock => mock.PrepareObjectJsonBody(
                    It.IsAny<string>()),
                Times.Exactly(createInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.CreateObjectAsync(
                    It.IsAny<string>()),
                Times.Exactly(createInvokeCount));

            this._processDataVerified = true;

            VerifyGetDataMethodCalls(fromInvokeCount, 0, getInfoObjectInvokeCount, 0, 0, 0, 0, 0);
        }
        #endregion
    }
}