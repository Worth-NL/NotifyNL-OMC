// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Models.Responses;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base;
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
    public sealed class CaseCreatedScenarioTests
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
        private static readonly CaseStatusType s_whitelistedStatusType = new() { Identification = "1" };
        private static readonly CaseStatusType s_notWhitelistedStatusType = new() { Identification = "4" };

        private const string TestEmailAddress = "test@email.com";
        private const string TestPhoneNumber = "911";
        private const string CaseId = "ZAAK-2024-00000000001";
        #endregion

        #region TryGetDataAsync()
        [Test]
        public void TryGetDataAsync_NotWhitelistedCaseId_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseCreatedScenario_TryGetData(
                s_notWhitelistedStatusType, DistributionChannels.Email);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(default));

                string expectedErrorMessage = ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_CaseTypeId
                    .Replace("{0}", "4")
                    .Replace("{1}", "ZGW_WHITELIST_ZAAKCREATE_IDS");

                Assert.That(exception?.Message.StartsWith(expectedErrorMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(ApiResources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(1, 0, 0);
            });
        }

        [TestCase(DistributionChannels.None)]
        [TestCase(DistributionChannels.Unknown)]
        [TestCase((DistributionChannels)(-1))]
        public async Task TryGetDataAsync_WhitelistedCaseId_InvalidDistributionChannel_ReturnsFailure(
            DistributionChannels invalidChannel)
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseCreatedScenario_TryGetData(
                s_whitelistedStatusType, invalidChannel);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.Message, Is.EqualTo(QueryResources.Response_QueryingData_ERROR_NotificationMethodMissing));
                Assert.That(actualResult.Content, Has.Count.EqualTo(0));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }

        [TestCase(DistributionChannels.Email, NotifyMethods.Email, 1, TestEmailAddress)]
        [TestCase(DistributionChannels.Sms, NotifyMethods.Sms, 1, TestPhoneNumber)]
        [TestCase(DistributionChannels.Letter, NotifyMethods.Letter, 1, "")]
        [TestCase(DistributionChannels.Both, null, 2, TestEmailAddress + TestPhoneNumber)]
        public async Task TryGetDataAsync_WhitelistedCaseId_ValidDistributionChannel_ReturnsSuccess(
            DistributionChannels testDistributionChannel, NotifyMethods? expectedNotifyMethod, int notifyDataCount, string expectedContactDetails)
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseCreatedScenario_TryGetData(
                s_whitelistedStatusType, testDistributionChannel);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

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

                    NotifyData secondResult = actualResult.Content.Last();
                    Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));

                    contactDetails = firstResult.ContactDetails + secondResult.ContactDetails;
                }
                else
                {
                    NotifyData onlyResult = actualResult.Content.First();
                    Assert.That(onlyResult.NotificationMethod, Is.EqualTo(expectedNotifyMethod!.Value));
                    contactDetails = onlyResult.ContactDetails;
                }

                Assert.That(contactDetails, Is.EqualTo(expectedContactDetails));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }
        #endregion

        #region GetPersonalizationAsync()
        [TestCase(DistributionChannels.Email)]
        [TestCase(DistributionChannels.Sms)]
        [TestCase(DistributionChannels.Letter)]
        public async Task GetPersonalizationAsync_ReturnsExpectedPersonalization(DistributionChannels testDistributionChannel)
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseCreatedScenario_TryGetData(
                s_whitelistedStatusType, testDistributionChannel);

            // Act
            QueryingDataResponse actualResult = await scenario.TryGetDataAsync(default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.Content, Has.Count.EqualTo(1));

                string actualSerialized = JsonSerializer.Serialize(actualResult.Content.First().Personalization);
                string expectedSerialized =
                    $"{{" +
                      $"\"klant.voornaam\":\"Jackie\"," +
                      $"\"klant.voorvoegselAchternaam\":null," +
                      $"\"klant.achternaam\":\"Chan\"," +
                      (testDistributionChannel == DistributionChannels.Letter ?
                          $"\"klant.street\":null," +
                          $"\"klant.number\":null," +
                          $"\"klant.zip\":null," +
                          $"\"klant.city\":null," +
                          $"\"klant.country\":null,"
                          : "") +
                      $"\"zaak.identificatie\":\"{CaseId}\"," +
                      $"\"zaak.omschrijving\":\"\"" +
                    $"}}";

                Assert.That(actualSerialized, Is.EqualTo(expectedSerialized));

                VerifyGetDataMethodCalls(1, 1, 1);
            });
        }
        #endregion

        #region ProcessDataAsync()
        [Test]
        public async Task ProcessDataAsync_EmptyNotifyData_ReturnsFailure()
        {
            // Arrange
            INotifyScenario scenario = ArrangeCaseCreatedScenario_ProcessData(true);

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

            INotifyScenario scenario = ArrangeCaseCreatedScenario_ProcessData(
                isSendingSuccessful: false,
                emailNotifyData:   testNotifyMethod == NotifyMethods.Email   ? testData : null,
                smsNotifyData:     testNotifyMethod == NotifyMethods.Sms     ? testData : null,
                letterNotifyData:  testNotifyMethod == NotifyMethods.Letter  ? testData : null);

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

            INotifyScenario scenario = ArrangeCaseCreatedScenario_ProcessData(
                isSendingSuccessful: true,
                emailNotifyData:   testNotifyMethod == NotifyMethods.Email   ? testData : null,
                smsNotifyData:     testNotifyMethod == NotifyMethods.Sms     ? testData : null,
                letterNotifyData:  testNotifyMethod == NotifyMethods.Letter  ? testData : null);

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
        private INotifyScenario ArrangeCaseCreatedScenario_TryGetData(
            CaseStatusType caseStatusType, DistributionChannels testDistributionChannel)
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new Case { Identification = CaseId });

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

            return new CaseCreatedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object)
                .SetCaseStatusType(caseStatusType);
        }

        private const string SimulatedNotifyExceptionMessage = "Some NotifyClientException";

        private INotifyScenario ArrangeCaseCreatedScenario_ProcessData(
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

            return new CaseCreatedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object)
                .SetCaseStatusType(s_whitelistedStatusType);
        }
        #endregion

        #region Verify
        private bool _getDataVerified;
        private bool _processDataVerified;

        private void VerifyGetDataMethodCalls(int fromCount, int getCaseCount, int getPartyCount)
        {
            if (this._getDataVerified) return;

            this._mockedDataQuery
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()), Times.Exactly(fromCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<Uri?>()), Times.Exactly(getCaseCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(It.IsAny<Uri?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Exactly(getPartyCount));

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

            VerifyGetDataMethodCalls(0, 0, 0);
        }
        #endregion
    }
}
