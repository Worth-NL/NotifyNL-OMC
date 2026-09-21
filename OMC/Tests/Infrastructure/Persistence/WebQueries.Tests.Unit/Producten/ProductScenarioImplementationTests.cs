// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System.Net;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.Exceptions;
using WebQueries.Producten;
using WebQueries.Producten.Interfaces;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenProducten;

namespace WebQueries.Tests.Unit.Producten
{
    [TestFixture]
    public sealed class ProductScenarioImplementationTests
    {
        private Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = null!;
        private Mock<IQueryContext> _mockedQueryContext = null!;

        private OmcConfiguration _configuration = null!;
        private IProductScenario _scenario = null!;

        #region Test data
        private static readonly Guid s_productId = Guid.Parse("da0df49a-cd71-4e24-9bae-5be8b01f2c36");

        private static readonly Uri s_productUri =
            new($"https://openproduct.test/producten/api/v1/producten/{s_productId}");
        #endregion

        [OneTimeSetUp]
        public void SetupTests()
        {
            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.BothValid_v2);
        }

        [SetUp]
        public void ResetMocks()
        {
            this._mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            this._mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            this._scenario = new ProductScenarioImplementation(
                this._mockedDataQuery.Object,
                this._configuration,
                NullLogger<ProductScenarioImplementation>.Instance);
        }

        [OneTimeTearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        #region Helper methods
        private static NotificationEvent GetProductNotification()
            => new()
            {
                Action = Actions.Create,
                Channel = Channels.Products,
                Resource = Resources.Product,
                MainObjectUri = s_productUri,
                ResourceUri = s_productUri
            };
        #endregion

        [Test]
        public void ProcessProductAsync_ProductGone_ThrowsProcessingAborted_SoTheNotificationIsNotRedelivered()
        {
            // NOTE: 404 means "Open Product" deleted the product between publishing the notification and
            //       OMC getting to it. Redelivering that could never succeed, so it has to abort (206)
            //       rather than fail (412).

            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.GetProductAsync(s_productUri))
                .ThrowsAsync(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.That(exception!.Message, Does.Contain(s_productId.ToString()));
        }

        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(null)]
        public void ProcessProductAsync_OpenProductUnreachable_RethrowsSoTheNotificationIsRedelivered(HttpStatusCode? statusCode)
        {
            // NOTE: The mirror image of the test above. A service that could not be reached or refused the
            //       call has to stay a failure, otherwise a transient outage silently drops the product for
            //       good. A null status is the "no answer at all" case - a timeout or a DNS failure.

            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.GetProductAsync(s_productUri))
                .ThrowsAsync(new HttpRequestException("Boom", null, statusCode));

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));
        }

        [Test]
        public void ProcessProductAsync_ProductFound_ReadsItFromTheNotificationsResourceUrl()
        {
            // NOTE: Only the fetch is asserted here - everything past it lands in the follow-up commits and
            //       still throws NotImplementedException.

            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.GetProductAsync(s_productUri))
                .ReturnsAsync(new Product
                {
                    Id = s_productId,
                    Name = "verhuurvergunning: straatweg 14",
                    IsPublished = true,
                    ProductType = new NestedProductType { Code = "PARKEERVERGUNNING-A" }
                });

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            this._mockedQueryContext.Verify(mock => mock.GetProductAsync(s_productUri), Times.Once);
        }
    }
}
