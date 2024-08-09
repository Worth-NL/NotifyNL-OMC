// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
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
        public void GetAllNotifyDataAsync_ForInvalidMessageType_ThrowsAbortedNotifyingException()
        {
            // Arrange
            SetMockedQueryContext(s_invalidInfoObjectType);
            SetMockedDataQueryService();

            INotifyScenario scenario = new DecisionMadeScenario(this._testConfiguration, this._mockedDataQuery.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_MessageType), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyInvoke(0, 0, 0);
            });
        }

        [Test]
        public void GetAllNotifyDataAsync_ForInvalidStatus_ThrowsAbortedNotifyingException()
        {
            // Arrange
            SetMockedQueryContext(s_invalidInfoObjectStatus);
            SetMockedDataQueryService();

            INotifyScenario scenario = new DecisionMadeScenario(this._testConfiguration, this._mockedDataQuery.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));
                Assert.That(exception?.Message.StartsWith(Resources.Processing_ABORT_DoNotSendNotification_DecisionStatus), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);
                
                VerifyInvoke(0, 0, 0);
            });
        }

        [Test]
        public void GetAllNotifyDataAsync_ForInvalidConfidentiality_ThrowsAbortedNotifyingException()
        {
            // Arrange
            SetMockedQueryContext(s_invalidInfoObjectConfidentiality);
            SetMockedDataQueryService();

            INotifyScenario scenario = new DecisionMadeScenario(this._testConfiguration, this._mockedDataQuery.Object);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.GetAllNotifyDataAsync(default));

                string expectedMessage = Resources.Processing_ABORT_DoNotSendNotification_DecisionConfidentiality
                    .Replace("{0}", s_invalidInfoObjectConfidentiality.Confidentiality.ToString());

                Assert.That(exception?.Message.StartsWith(expectedMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);

                VerifyInvoke(0, 0, 0);
            });
        }
        #endregion

        #region Helper methods
        private void SetMockedQueryContext(InfoObject infoObject)
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionResourceAsync())
                .ReturnsAsync(new DecisionResource());

            this._mockedQueryContext
                .Setup(mock => mock.GetInfoObjectAsync(It.IsAny<object?>()))
                .ReturnsAsync(infoObject);

            this._mockedQueryContext
                .Setup(mock => mock.GetDecisionAsync(It.IsAny<DecisionResource?>()))
                .ReturnsAsync(new Decision());

            this._mockedQueryContext
                .Setup(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()))
                .ReturnsAsync(new CaseType());

            this._mockedQueryContext
                .Setup(mock => mock.GetCaseStatusesAsync())
                .ReturnsAsync(new CaseStatuses());
        }

        private void SetMockedDataQueryService()
        {
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);
        }
        #endregion

        #region Verify
        private void VerifyInvoke(int getLastCaseTypeInvokeCount, int getCaseStatusesInvokeCount, int getDecisionInvokeCount)
        {
            this._mockedDataQuery
                .Verify(mock => mock.From(It.IsAny<NotificationEvent>()),
                Times.Once);

            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionResourceAsync(),
                Times.Once);

            this._mockedQueryContext
                .Verify(mock => mock.GetInfoObjectAsync(It.IsAny<object?>()),
                Times.Once);

            this._mockedQueryContext
                .Verify(mock => mock.GetDecisionAsync(It.IsAny<DecisionResource?>()),
                Times.Exactly(getDecisionInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()),
                Times.Exactly(getLastCaseTypeInvokeCount));

            this._mockedQueryContext
                .Verify(mock => mock.GetCaseStatusesAsync(),
                Times.Exactly(getCaseStatusesInvokeCount));
        }
        #endregion
    }
}