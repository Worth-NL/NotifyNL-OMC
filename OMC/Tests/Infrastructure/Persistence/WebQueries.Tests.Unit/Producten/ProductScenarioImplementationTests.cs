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
using ZgwModels.Mapping.Enums.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
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

        // ZGW_WHITELIST_PRODUCTCREATE_IDS is "1, 2, 3" in the test configuration.
        private const string WhitelistedProductTypeCode = "1";
        private const string BlockedProductTypeCode = "9";

        private const string TestBsn = "999990019";
        private const string TestKvk = "12345678";
        private const string TestVestigingsnummer = "000012345678";

        // The "referentie" the scenario asks OpenKlant to prefer.
        private const string Portaalvoorkeur = "portaalvoorkeur";
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

        private static Product GetProduct(
            string productTypeCode = WhitelistedProductTypeCode, bool isPublished = true)
            => new()
            {
                Id = s_productId,
                Name = "verhuurvergunning: straatweg 14",
                IsPublished = isPublished,
                ProductType = new NestedProductType { Code = productTypeCode, Name = "Parkeervergunning" }
            };

        private void SetupProduct(Product product)
            => this._mockedQueryContext
                .Setup(mock => mock.GetProductAsync(s_productUri))
                .ReturnsAsync(product);

        private static Product GetProductWithOwners(params Eigenaar[] owners)
        {
            Product product = GetProduct();
            product.Owners = [.. owners];

            return product;
        }

        private void SetupPartyFound(
            string codeSoortObjectId, string objectId, string emailAddress = "test@example.com")
            => this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataByIdentifierAsync(codeSoortObjectId, objectId, Portaalvoorkeur, false, DistributionChannels.Email))
                .ReturnsAsync(new CommonPartyData { Name = "Test", EmailAddress = emailAddress });

        private void SetupPartyMissing(string codeSoortObjectId, string objectId)
            => this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataByIdentifierAsync(codeSoortObjectId, objectId, Portaalvoorkeur, false, DistributionChannels.Email))
                .ThrowsAsync(new HttpRequestException("No party results"));
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
            // NOTE: Only the fetch, the gates and the owner resolution are asserted here - everything past
            //       them lands in the follow-up commits and still throws NotImplementedException.

            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { BsnNumber = TestBsn }));
            SetupPartyFound("bsn", TestBsn);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            this._mockedQueryContext.Verify(mock => mock.GetProductAsync(s_productUri), Times.Once);
        }


        [Test]
        public void ProcessProductAsync_BsnOwner_ResolvesThePartyOnTheBsnIdentificator()
        {
            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { BsnNumber = TestBsn }));
            SetupPartyFound("bsn", TestBsn);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            this._mockedQueryContext.Verify(
                mock => mock.GetPartyDataByIdentifierAsync("bsn", TestBsn, Portaalvoorkeur, false, DistributionChannels.Email), Times.Once);
        }

        [Test]
        public void ProcessProductAsync_KvkOwner_ResolvesThePartyOnTheKvkIdentificator()
        {
            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { KvkNumber = TestKvk }));
            SetupPartyFound("kvk", TestKvk);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            this._mockedQueryContext.Verify(
                mock => mock.GetPartyDataByIdentifierAsync("kvk", TestKvk, Portaalvoorkeur, false, DistributionChannels.Email), Times.Once);
        }

        [Test]
        public void ProcessProductAsync_KvkOwnerWithVestigingsnummer_StillResolvesOnTheKvkNumberAlone()
        {
            // NOTE: V1 does not narrow to a branch. OpenKlant finds a vestiging by combining
            //       subIdentificatorVan__ with partijIdentificator__, which is a second query path this
            //       does not implement yet.

            // Arrange
            SetupProduct(GetProductWithOwners(
                new Eigenaar { KvkNumber = TestKvk, BranchNumber = TestVestigingsnummer }));
            SetupPartyFound("kvk", TestKvk);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            this._mockedQueryContext.Verify(
                mock => mock.GetPartyDataByIdentifierAsync("kvk", TestKvk, Portaalvoorkeur, false, DistributionChannels.Email), Times.Once);
        }

        [Test]
        public void ProcessProductAsync_EveryOwnerResolves_ResolvesThemAll()
        {
            // Arrange
            SetupProduct(GetProductWithOwners(
                new Eigenaar { BsnNumber = TestBsn },
                new Eigenaar { KvkNumber = TestKvk }));
            SetupPartyFound("bsn", TestBsn);
            SetupPartyFound("kvk", TestKvk);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.Multiple(() =>
            {
                this._mockedQueryContext.Verify(
                    mock => mock.GetPartyDataByIdentifierAsync("bsn", TestBsn, Portaalvoorkeur, false, DistributionChannels.Email), Times.Once);
                this._mockedQueryContext.Verify(
                    mock => mock.GetPartyDataByIdentifierAsync("kvk", TestKvk, Portaalvoorkeur, false, DistributionChannels.Email), Times.Once);
            });
        }

        [Test]
        public void ProcessProductAsync_OneOwnerHasNoParty_AbortsWithoutNotifyingTheOthers()
        {
            // NOTE: The all-or-nothing rule. The first owner resolves perfectly well and is still not
            //       notified, because a failure OMC cannot attach to a party is a failure it cannot record.

            // Arrange
            SetupProduct(GetProductWithOwners(
                new Eigenaar { BsnNumber = TestBsn },
                new Eigenaar { KvkNumber = TestKvk }));
            SetupPartyFound("bsn", TestBsn);
            SetupPartyMissing("kvk", TestKvk);

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.That(exception!.Message, Does.Contain("no partij"));
        }

        [Test]
        public void ProcessProductAsync_OwnerWithOnlyAKlantnummer_Aborts()
        {
            // NOTE: OpenKlant matches a customer number only through a filter its own schema marks
            //       deprecated, so V1 treats such an owner as unresolvable rather than guessing.

            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { CustomerNumber = "K-12345" }));

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.That(exception!.Message, Does.Contain("no BSN or KVK"));
        }

        [Test]
        public void ProcessProductAsync_ProductWithoutOwners_Aborts()
        {
            // Arrange
            SetupProduct(GetProductWithOwners());

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.That(exception!.Message, Does.Contain("no eigenaren"));
        }

        [Test]
        public void ProcessProductAsync_OwnerResolutionFailure_NeverPutsTheIdentifierInTheReason()
        {
            // NOTE: A BSN is personal data and the reason travels into traces and logs. Only the kind of
            //       identificator belongs there, never its value.

            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { BsnNumber = TestBsn }));
            SetupPartyMissing("bsn", TestBsn);

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.That(exception!.Message, Does.Not.Contain(TestBsn));
        }


        [Test]
        public void ProcessProductAsync_OwnerLookup_AsksOpenKlantToPreferThePortaalvoorkeurAddress()
        {
            // NOTE: PartyResults treats a matching "referentie" as an outright win over the party's own
            //       preferred address, which is the precedence a product notification wants. Pinned here
            //       because passing the wrong reference fails silently - a usable address is still found,
            //       just not the one the party nominated for portal correspondence.

            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { BsnNumber = TestBsn }));
            SetupPartyFound("bsn", TestBsn);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            this._mockedQueryContext.Verify(
                mock => mock.GetPartyDataByIdentifierAsync("bsn", TestBsn, "portaalvoorkeur", false, DistributionChannels.Email), Times.Once);
        }

        [Test]
        public void ProcessProductAsync_OwnerLookup_RestrictsTheSearchToEmailAddresses()
        {
            // NOTE: Without the restriction, a party whose preferred address is a phone number reads as
            //       having no e-mail: PartyResults settles on the preferred address and never reaches the
            //       e-mail behind it. Only e-mail is deliverable here, so the phone must not be considered
            //       at all rather than be considered and discarded.

            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { BsnNumber = TestBsn }));
            SetupPartyFound("bsn", TestBsn);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            this._mockedQueryContext.Verify(
                mock => mock.GetPartyDataByIdentifierAsync(
                    "bsn", TestBsn, "portaalvoorkeur", false, DistributionChannels.Email), Times.Once);
        }

        [Test]
        public void ProcessProductAsync_OwnerWithoutAnEmailAddress_StillGetsPastResolution()
        {
            // NOTE: The counterpart of the all-or-nothing party rule. A missing party stops everything; a
            //       missing address does not, because it can be recorded against the party that was found.

            // Arrange
            SetupProduct(GetProductWithOwners(new Eigenaar { BsnNumber = TestBsn }));
            SetupPartyFound("bsn", TestBsn, emailAddress: string.Empty);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));
        }

        [Test]
        public void ProcessProductAsync_SomeOwnersReachableAndSomeNot_StillGetsPastResolution()
        {
            // Arrange
            SetupProduct(GetProductWithOwners(
                new Eigenaar { BsnNumber = TestBsn },
                new Eigenaar { KvkNumber = TestKvk }));
            SetupPartyFound("bsn", TestBsn);
            SetupPartyFound("kvk", TestKvk, emailAddress: string.Empty);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));
        }

        [Test]
        public void ProcessProductAsync_ProductTypeNotWhitelisted_ThrowsProcessingAborted_NamingTheSetting()
        {
            // Arrange
            SetupProduct(GetProduct(productTypeCode: BlockedProductTypeCode));

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain(BlockedProductTypeCode));

                // The reason has to name the setting to change, not just say "not whitelisted".
                Assert.That(exception.Message, Does.Contain("ZGW_WHITELIST_PRODUCTCREATE_IDS"));
            });
        }

        [Test]
        public void ProcessProductAsync_ProductNotPublished_ThrowsProcessingAborted()
        {
            // Arrange
            SetupProduct(GetProduct(isPublished: false));

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.That(exception!.Message, Does.Contain("gepubliceerd"));
        }

        [Test]
        public void ProcessProductAsync_ProductTypeNotWhitelisted_AndNotPublished_ReportsTheWhitelistFirst()
        {
            // NOTE: Both gates reject this product. The whitelist runs first so the reported reason is the
            //       one an operator can act on, rather than a property of the product itself.

            // Arrange
            SetupProduct(GetProduct(productTypeCode: BlockedProductTypeCode, isPublished: false));

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.That(exception!.Message, Does.Contain("not whitelisted"));
        }
    }
}
