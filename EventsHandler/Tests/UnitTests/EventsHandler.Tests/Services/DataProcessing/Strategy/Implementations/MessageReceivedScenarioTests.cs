// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using Moq;

namespace EventsHandler.UnitTests.Services.DataProcessing.Strategy.Implementations
{
    [TestFixture]
    public sealed class MessageReceivedScenarioTests
    {
        private readonly Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = new(MockBehavior.Strict);
        private readonly Mock<IQueryContext> _mockedQueryContext = new(MockBehavior.Strict);
        private readonly Mock<INotifyService<NotificationEvent, NotifyData>> _mockedNotifyService = new(MockBehavior.Strict);

        [TearDown]
        public void TestsReset()
        {
            this._mockedDataQuery.Reset();
            this._mockedQueryContext.Reset();
            this._mockedNotifyService.Reset();

            this._getDataVerified = false;
            this._processDataVerified = false;
        }

        #region Test data
        private static readonly NotificationEvent s_invalidNotification = new();
        private static readonly NotificationEvent s_validNotification = new()
        {
            Attributes = new EventAttributes
            {
                ObjectTypeUri = new Uri($"http://www.domain.com/{ConfigurationHandler.TestMessageObjectTypeUuid}")
            }
        };
        #endregion

        #region TryGetDataAsync()
        [Test]
        public void TryGetDataAsync_NotAllowedMessages_ThrowsAbortedNotifyingException()
        {
            // Arrange
            INotifyScenario scenario = ArrangeMessageScenario_TryGetData(false);

            // Act & Assert
            Assert.Multiple(() =>
            {
                AbortedNotifyingException? exception =
                    Assert.ThrowsAsync<AbortedNotifyingException>(() => scenario.TryGetDataAsync(s_validNotification));

                string expectedMessage = Resources.Processing_ABORT_DoNotSendNotification_MessagesForbidden
                    .Replace("{0}", "USER_WHITELIST_MESSAGE_ALLOWED");

                Assert.That(exception?.Message.StartsWith(expectedMessage), Is.True);
                Assert.That(exception?.Message.EndsWith(Resources.Processing_ABORT), Is.True);

                VerifyGetDataMethodCalls(0, 0, 0, 0);
            });
        }
        #endregion

        #region Setup
        private INotifyScenario ArrangeMessageScenario_TryGetData(
            bool isMessageAllowed, DistributionChannels testDistributionChannel = DistributionChannels.Email)
        {
            WebApiConfiguration configuration = isMessageAllowed
                ? ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidEnvironment)
                : ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.InvalidEnvironment);

            // IQueryContext


            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Decision Scenario
            return new MessageReceivedScenario(configuration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }
        #endregion

        #region Verify
        private bool _getDataVerified;
        private bool _processDataVerified;

        private void VerifyGetDataMethodCalls(int fromInvokeCount, int getCaseAsyncInvokeCount,
            int getCaseTypeInvokeCount, int getPartyDataAsyncInvokeCount)
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
            //this._mockedQueryContext
            //    .Verify(mock => mock.GetCaseAsync(It.IsAny<object?>()),
            //    Times.Exactly(getCaseAsyncInvokeCount));
            
            //this._mockedQueryContext  // Dependent queries
            //    .Verify(mock => mock.GetLastCaseTypeAsync(It.IsAny<CaseStatuses?>()),
            //    Times.Exactly(getCaseTypeInvokeCount));
            //this._mockedQueryContext
            //    .Verify(mock => mock.GetCaseStatusesAsync(It.IsAny<Uri?>()),
            //    Times.Exactly(getCaseTypeInvokeCount));

            //this._mockedQueryContext
            //    .Verify(mock => mock.GetPartyDataAsync(It.IsAny<string>()),
            //    Times.Exactly(getPartyDataAsyncInvokeCount));

            this._getDataVerified = true;

            VerifyProcessDataMethodCalls(0, 0);
        }

        private void VerifyProcessDataMethodCalls(int sendEmailInvokeCount, int sendSmsInvokeCount)
        {
            if (this._processDataVerified)
            {
                return;
            }
            
            // INotifyService
            //this._mockedNotifyService
            //    .Verify(mock => mock.SendEmailAsync(
            //        It.IsAny<NotificationEvent>(),
            //        It.IsAny<NotifyData>()),
            //    Times.Exactly(sendEmailInvokeCount));
            
            //this._mockedNotifyService
            //    .Verify(mock => mock.SendSmsAsync(
            //        It.IsAny<NotificationEvent>(),
            //        It.IsAny<NotifyData>()),
            //    Times.Exactly(sendSmsInvokeCount));

            this._processDataVerified = true;

            VerifyGetDataMethodCalls(0, 0, 0, 0);
        }
        #endregion
    }
}