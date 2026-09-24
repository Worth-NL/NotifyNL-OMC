// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries;
using WebQueries.DataQuerying.Strategies.Queries.OpenProducten;
using WebQueries.DataQuerying.Strategies.Queries.OpenProducten.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenProducten;

namespace WebQueries.Tests.Unit.DataQuerying.Strategies.Queries.OpenProducten
{
    [TestFixture]
    public sealed class QueryProductenTests
    {
        private static readonly Uri s_productUri =
            new("https://test.domain/producten/api/v1/producten/da0df49a-cd71-4e24-9bae-5be8b01f2c36");

        private OmcConfiguration _configuration = null!;
        private Mock<IQueryBase> _mockedQueryBase = null!;
        private IQueryProducten _queryProducten = null!;

        [OneTimeSetUp]
        public void SetupTests()
        {
            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.BothValid_v2);
        }

        [SetUp]
        public void ResetMocks()
        {
            this._mockedQueryBase = new Mock<IQueryBase>(MockBehavior.Strict);
            this._queryProducten = new QueryProducten(this._configuration);
        }

        [OneTimeTearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        [Test]
        public async Task GetProductAsync_ValidUri_QueriesOpenProductenWithThatUri()
        {
            // Arrange
            Uri? capturedUri = null;

            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<Product>(
                    HttpClientTypes.OpenProducten, It.IsAny<Uri>(), It.IsAny<string>()))
                .Callback<HttpClientTypes, Uri, string>((_, uri, _) => capturedUri = uri)
                .ReturnsAsync(new Product { Id = Guid.NewGuid() });

            // Act
            _ = await this._queryProducten.GetProductAsync(this._mockedQueryBase.Object, s_productUri);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(capturedUri, Is.EqualTo(s_productUri));

                this._mockedQueryBase.Verify(mock => mock.ProcessGetAsync<Product>(
                    HttpClientTypes.OpenProducten, s_productUri, It.IsAny<string>()), Times.Once);
            });
        }

        [Test]
        public async Task GetProductAsync_Response_MapsTheNestedProductTypeWithoutASecondRequest()
        {
            // Arrange
            // NOTE: "Open Product" embeds the whole product type in the product response, so the product
            //       type is available after a single call - see ZgwModels...OpenProducten.Product.
            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<Product>(
                    HttpClientTypes.OpenProducten, s_productUri, It.IsAny<string>()))
                .ReturnsAsync(new Product
                {
                    Name = "verhuurvergunning: straatweg 14",
                    IsPublished = true,
                    ProductType = new NestedProductType { Code = "PARKEERVERGUNNING-A", Name = "Parkeervergunning" },
                    Owners = [new Owner { BsnNumber = "999990019" }]
                });

            // Act
            Product product = await this._queryProducten.GetProductAsync(this._mockedQueryBase.Object, s_productUri);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(product.ProductType.Code, Is.EqualTo("PARKEERVERGUNNING-A"));
                Assert.That(product.ProductType.Name, Is.EqualTo("Parkeervergunning"));
                Assert.That(product.IsPublished, Is.True);
                Assert.That(product.Owners, Has.Count.EqualTo(1));

                this._mockedQueryBase.Verify(mock => mock.ProcessGetAsync<Product>(
                    It.IsAny<HttpClientTypes>(), It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
            });
        }

        [Test]
        public async Task GetHealthCheckAsync_CallsTheProductenCollectionOfTheConfiguredDomain()
        {
            // Arrange
            Mock<IHttpNetworkService> mockedNetworkService = new(MockBehavior.Strict);
            Uri? capturedUri = null;

            mockedNetworkService
                .Setup(mock => mock.GetAsync(HttpClientTypes.OpenProducten, It.IsAny<Uri>()))
                .Callback<HttpClientTypes, Uri>((_, uri) => capturedUri = uri)
                .ReturnsAsync(WebQueries.DataQuerying.Models.Responses.HttpRequestResponse.Success(string.Empty));

            // Act
            _ = await ((IDomain)this._queryProducten).GetHealthCheckAsync(mockedNetworkService.Object);

            // Assert
            Assert.That(capturedUri?.AbsoluteUri,
                Is.EqualTo($"{this._configuration.ZGW.Endpoint.OpenProducten()}/producten"));
        }
    }
}
