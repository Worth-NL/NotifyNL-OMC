// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Models.Responses;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using Moq;
using System.Text.Json;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Properties;
using ZgwModels.Enums;
using ZgwModels.Mapping.Enums.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Tests.Unit.Services.DataProcessing.Strategy.Implementations
{
    [TestFixture]
    public sealed class CaseClosedScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

        private OmcConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            // NOTE: ZGW_WHITELIST_ZAAKCLOSE_IDS = "*" in ValidEnvironment_v1 — every case type is allowed
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
        private static readonly CaseStatusType s_statusType = new() { Identification = "1", Name = "Afgehandeld" };

        private const string TestEmailAddress = "test@email.com";
        private const string TestPhoneNumber = "911";
        private const string CaseId = "ZAAK-2024-00000000001";
        private static readonly Uri s_resultTypeUri = new("https://www.domain.com/resultaattypen/some-uuid");
        #endregion

        #region TryGetDataAsync()
        [TestCase(DistributionChannels.None)]
        [TestCase(DistributionChannels.Unknown)]
        [TestCase((DistributionChannels)(-1))]
        public async Task TryGetDataAsync_WhitelistIsWildcard_InvalidDistributionChannel_ReturnsFailure(
            DistributionChannels invalidChannel)
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseClosedScenario_TryGetData(
                invalidChannel, hasResultType: false);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(QueryResources.Response_QueryingData_ERROR_NotificationMethodMissing));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1, 0);
            });
        }

        [TestCase(DistributionChannels.Email, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(DistributionChannels.Sms, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(DistributionChannels.Letter, NotifyMethods.Letter, 1, "")]
        [TestCase(DistributionChannels.Both, null, 2, TestEmailAddress + TestPhoneNumber)]
        public async Task TryGetDataAsync_WhitelistIsWildcard_NoResultType_ValidChannel_ReturnsSuccess(
            DistributionChannels testDistributionChannel, NotifyMethods? expectedNotifyMethod, int notifyDataCount, string expectedContactDetails)
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseClosedScenario_TryGetData(
                testDistributionChannel, hasResultType: false);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Content, Has.Count.EqualTo(notifyDataCount));

                string contactDetails = testDistributionChannel == DistributionChannels.Both
                    ? actualResult.Content.First().ContactDetails + actualResult.Content.Last().ContactDetails
                    : actualResult.Content.First().ContactDetails;

                Assert.That(contactDetails, Is.EqualTo(expectedContactDetails));

                VerifyGetDataMethodCalls(1, 1, 1, 0);
            });
        }

        [TestCase(DistributionChannels.Email)]
        [TestCase(DistributionChannels.Sms)]
        [TestCase(DistributionChannels.Letter)]
        public async Task TryGetDataAsync_WhitelistIsWildcard_WithResultType_ValidChannel_ReturnsSuccess(
            DistributionChannels testDistributionChannel)
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseClosedScenario_TryGetData(
                testDistributionChannel, hasResultType: true);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Content, Has.Count.EqualTo(1));

                VerifyGetDataMethodCalls(1, 1, 1, 1);
            });
        }
        #endregion

        #region GetPersonalizationAsync()
        [TestCase(DistributionChannels.Email, false)]
        [TestCase(DistributionChannels.Sms, false)]
        [TestCase(DistributionChannels.Letter, false)]
        [TestCase(DistributionChannels.Email, true)]
        public async Task GetPersonalizationAsync_ReturnsExpectedPersonalization(
            DistributionChannels testDistributionChannel, bool hasResultType)
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseClosedScenario_TryGetData(
                testDistributionChannel, hasResultType);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Content, Has.Count.EqualTo(1));

                string actualSerialized = JsonSerializer.Serialize(actualResult.Content.First().Personalization);
                string expectedResultTypeName = hasResultType ? "Gegrond" : string.Empty;

                // NOTE: Letter personalization in CaseClosedScenario does NOT include the resultType key
                bool isLetter = testDistributionChannel == DistributionChannels.Letter;

                string expectedSerialized =
                    $"{{" +
                      $"\"klant.voornaam\":\"Jackie\"," +
                      $"\"klant.voorvoegselAchternaam\":null," +
                      $"\"klant.achternaam\":\"Chan\"," +
                      (isLetter ?
                          $"\"klant.street\":null," +
                          $"\"klant.number\":null," +
                          $"\"klant.zip\":null," +
                          $"\"klant.city\":null," +
                          $"\"klant.country\":null,"
                          : "") +
                      $"\"zaak.identificatie\":\"{CaseId}\"," +
                      $"\"zaak.omschrijving\":\"\"," +
                      $"\"status.omschrijving\":\"Afgehandeld\"" +
                      (!isLetter ? $",\"zaak.resultaat.resultaatType.omschrijving\":\"{expectedResultTypeName}\"" : "") +
                    $"}}";

                Assert.That(actualSerialized, Is.EqualTo(expectedSerialized));
            });
        }
        #endregion

        #region ProcessDataAsync()
        [Test]
        public async Task ProcessDataAsync_EmptyNotifyData_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseClosedScenario_ProcessData(true);

            // Act
            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_ERROR_Scenario_MissingNotifyData));

                VerifyProcessDataMethodCalls(0, 0, 0);
            });
        }

        [TestCase(NotifyMethods.Email, 1, 0, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1, 0)]
        [TestCase(NotifyMethods.Letter, 0, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_SendingFailed_ReturnsFailure(
            NotifyMethods testNotifyMethod, int emailCount, int smsCount, int letterCount)
        {
            NotifyData testData = new(testNotifyMethod);

            INotifyScenario scenario = ArrangeCaseClosedScenario_ProcessData(
                isSendingSuccessful: false,
                emailNotifyData:  testNotifyMethod == NotifyMethods.Email   ? testData : null,
                smsNotifyData:    testNotifyMethod == NotifyMethods.Sms     ? testData : null,
                letterNotifyData: testNotifyMethod == NotifyMethods.Letter  ? testData : null);

            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, [testData]);

            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(SimulatedNotifyExceptionMessage));

                VerifyProcessDataMethodCalls(emailCount, smsCount, letterCount);
            });
        }

        [TestCase(NotifyMethods.Email, 1, 0, 0)]
        [TestCase(NotifyMethods.Sms, 0, 1, 0)]
        [TestCase(NotifyMethods.Letter, 0, 0, 1)]
        public async Task ProcessDataAsync_ValidNotifyData_SendingSucceeded_ReturnsSuccess(
            NotifyMethods testNotifyMethod, int emailCount, int smsCount, int letterCount)
        {
            NotifyData testData = new(testNotifyMethod);

            INotifyScenario scenario = ArrangeCaseClosedScenario_ProcessData(
                isSendingSuccessful: true,
                emailNotifyData:  testNotifyMethod == NotifyMethods.Email   ? testData : null,
                smsNotifyData:    testNotifyMethod == NotifyMethods.Sms     ? testData : null,
                letterNotifyData: testNotifyMethod == NotifyMethods.Letter  ? testData : null);

            ProcessingDataResponse actualResult = await scenario.ProcessDataAsync(default, [testData]);

            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(ApiResources.Processing_SUCCESS_Scenario_DataProcessed));

                VerifyProcessDataMethodCalls(emailCount, smsCount, letterCount);
            });
        }
        #endregion

        #region Setup
        private INotifyScenario ArrangeCaseClosedScenario_TryGetData(
            DistributionChannels testDistributionChannel, bool hasResultType)
        {
            Case testCase = hasResultType
                ? new Case
                {
                    Identification = CaseId,
                    Expanded = new Case.ExpandedResult
                    {
                        Result = new Case.Result { ResultType = s_resultTypeUri }
                    }
                }
                : new Case { Identification = CaseId };

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(testCase);

            if (hasResultType)
            {
                this._mockedQueryContext
                    .Setup(mock => mock.GetCaseResultTypeAsync(It.IsAny<Uri?>()))
                    .ReturnsAsync(new CaseResultType { Name = "Gegrond" });
            }

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<Uri?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new CommonPartyData
                {
                    Name = "Jackie",
                    Surname = "Chan",
                    DistributionChannel = testDistributionChannel,
                    EmailAddress = TestEmailAddress,
                    TelephoneNumber = TestPhoneNumber
                });

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            return new CaseClosedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object)
                .SetCaseStatusType(s_statusType);
        }

        private const string SimulatedNotifyExceptionMessage = "Some NotifyClientException";

        private INotifyScenario ArrangeCaseClosedScenario_ProcessData(
            bool isSendingSuccessful, NotifyData? emailNotifyData = null, NotifyData? smsNotifyData = null, NotifyData? letterNotifyData = null)
        {
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            this._mockedNotifyService
                .Setup(mock => mock.SendEmailAsync(emailNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            this._mockedNotifyService
                .Setup(mock => mock.SendSmsAsync(smsNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            this._mockedNotifyService
                .Setup(mock => mock.SendLetterAsync(letterNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(isSendingSuccessful ? NotifySendResponse.Success() : NotifySendResponse.Failure(SimulatedNotifyExceptionMessage));

            return new CaseClosedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object)
                .SetCaseStatusType(s_statusType);
        }
        #endregion

        #region Verify
        private bool _getDataVerified;
        private bool _processDataVerified;

        private void VerifyGetDataMethodCalls(int fromCount, int getCaseCount, int getPartyCount, int getResultTypeCount)
        {
            if (this._getDataVerified) return;

            this._mockedDataQuery
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()), Times.Exactly(fromCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<Uri?>()), Times.Exactly(getCaseCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(It.IsAny<Uri?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Exactly(getPartyCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseResultTypeAsync(It.IsAny<Uri?>()), Times.Exactly(getResultTypeCount));

            this._getDataVerified = true;

            VerifyProcessDataMethodCalls(0, 0, 0);
        }

        private void VerifyProcessDataMethodCalls(int emailCount, int smsCount, int letterCount)
        {
            if (this._processDataVerified) return;

            this._mockedNotifyService
                .Verify(mock => mock.SendEmailAsync(It.IsAny<NotifyData>()), Times.Exactly(emailCount));

            this._mockedNotifyService
                .Verify(mock => mock.SendSmsAsync(It.IsAny<NotifyData>()), Times.Exactly(smsCount));

            this._mockedNotifyService
                .Verify(mock => mock.SendLetterAsync(It.IsAny<NotifyData>()), Times.Exactly(letterCount));

            this._processDataVerified = true;

            VerifyGetDataMethodCalls(0, 0, 0, 0);
        }
        #endregion
    }
}
