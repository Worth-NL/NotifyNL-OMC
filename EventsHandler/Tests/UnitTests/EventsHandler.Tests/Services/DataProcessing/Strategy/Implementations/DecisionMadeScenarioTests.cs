// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
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

        #region Test data
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

        private static readonly Uri s_validUri =
            new($"https://www.domain.com/{ConfigurationHandler.TestTypeUuid}");  // NOTE: Matches to UUID from test Environment Configuration
        
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
        #endregion

        [TearDown]
        public void ResetTests()
        {
            this._mockedDataQuery.Reset();
            this._mockedQueryContext.Reset();
        }

        #region GetAllNotifyDataAsync()
        [Test]
        public void GetAllNotifyDataAsync_InvalidMessageType_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario(s_invalidInfoObjectType);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_MessageType), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyInvoke(0, 0, 0, 0);
            });
        }

        [Test]
        public void GetAllNotifyDataAsync_ValidMessageType_InvalidStatus_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario(s_invalidInfoObjectStatus);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_DecisionStatus), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyInvoke(0, 0, 0, 0);
            });
        }

        [Test]
        public void GetAllNotifyDataAsync_InvalidConfidentiality_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeDecisionScenario(s_invalidInfoObjectConfidentiality);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));

                string expectedMessage = Resources.Processing_ABORT_DoNotSendNotification_DecisionConfidentiality
                    .Replace("{0}", s_invalidInfoObjectConfidentiality.Confidentiality.ToString());

                Assert.That(exception?.Message.StartsWith(expectedMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);

                VerifyInvoke(0, 0, 0, 0);
            });
        }
        #endregion

        #region Helper methods
        private INotifyScenario ArrangeDecisionScenario(InfoObject testInfoObject)
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
                .Setup(mock => mock.GetCaseAsync(It.IsAny<object?>()))
                .ReturnsAsync(new Case());

            this._mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()))
                .ReturnsAsync(new CaseType());

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(new CaseStatuses());

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string?>()))
                .ReturnsAsync(new CommonPartyData());

            this._mockedQueryContext
                .Setup(mock => mock.GetBsnNumberAsync(It.IsAny<Uri>()))
                .ReturnsAsync(string.Empty);

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseTypeUriAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(DefaultValues.Models.EmptyUri);

            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Decision Scenario
            return new DecisionMadeScenario(this._testConfiguration, this._mockedDataQuery.Object);
        }
        #endregion

        #region Verify
        private void VerifyInvoke(int getDecisionInvokeCount, int getCaseInvokeCount, int getCaseTypeInvokeCount,
            int getCitizenDetailsInvokeCount)
        {
            this._mockedDataQuery
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()),
                Times.Once);

            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionResourceAsync(It.IsAny<Uri?>()),
                Times.Once);

            this._mockedQueryContext
                .Verify(mock => mock.GetInfoObjectAsync(It.IsAny<object?>()),
                Times.Once);

            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionAsync(It.IsAny<DecisionResource?>()),
                Times.Exactly(getDecisionInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<object?>()),
                Times.Exactly(getCaseInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()),
                Times.Exactly(getCaseTypeInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCaseTypeInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(It.IsAny<string?>()),
                Times.Exactly(getCitizenDetailsInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetBsnNumberAsync(It.IsAny<Uri>()),
                Times.Exactly(getCitizenDetailsInvokeCount));
            this._mockedQueryContext
                .Verify(mock => mock.GetCaseTypeUriAsync(It.IsAny<Uri?>()),
                Times.Exactly(getCitizenDetailsInvokeCount));
        }
        #endregion
    }
}