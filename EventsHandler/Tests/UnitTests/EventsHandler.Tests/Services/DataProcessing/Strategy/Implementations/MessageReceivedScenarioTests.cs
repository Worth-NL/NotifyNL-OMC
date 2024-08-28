// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
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

        private WebApiConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidEnvironment);
        }

        [TearDown]
        public void TestsReset()
        {
            this._mockedDataQuery.Reset();
            this._mockedQueryContext.Reset();
            this._mockedNotifyService.Reset();
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._testConfiguration.Dispose();
        }

        #region TryGetDataAsync()
        [Test]
        public void TryGetDataAsync_NotAllowedMessages_ThrowsAbortedNotifyingException()
        {

        }
        #endregion

        #region Setup
        private INotifyScenario ArrangeMessageScenario_TryGetData(
            bool isMessageAllowed, DistributionChannels testDistributionChannel = DistributionChannels.Email)
        {
            // IQueryContext


            // IDataQueryService
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Decision Scenario
            return new MessageReceivedScenario(this._testConfiguration, this._mockedDataQuery.Object, this._mockedNotifyService.Object);
        }
        #endregion
    }
}