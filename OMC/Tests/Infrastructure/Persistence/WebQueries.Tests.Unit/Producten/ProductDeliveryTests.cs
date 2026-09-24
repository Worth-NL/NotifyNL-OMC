// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Exceptions;
using WebQueries.Producten;
using WebQueries.Producten.Interfaces;
using WebQueries.Producten.Models;
using WebQueries.Register.Interfaces;
using WebQueries.Tracing;
using ZgwModels.Enums;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Enums.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenProducten;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.Producten
{
    /// <summary>
    /// The delivery half of the "Product created" scenario - what happens once the answer is settled.
    /// </summary>
    /// <remarks>
    ///   Driven through <c>ProcessProductAsync</c>, because the sending is part of that same call: every
    ///   test here arranges a product that passes every gate, which leaves delivery as the only thing left
    ///   to observe. That the sends have already happened by the time it returns is the point - nothing
    ///   below waits for, polls or drains anything.
    /// </remarks>
    [TestFixture]
    public sealed class ProductDeliveryTests
    {
        private Mock<IQueryContext> _mockedQueryContext = null!;
        private Mock<INotifyClient> _mockedNotifyClient = null!;
        private Mock<IHttpClientFactory<INotifyClient, string>> _mockedNotifyClientFactory = null!;
        private Mock<ISerializationService> _mockedSerializer = null!;
        private Mock<ITelemetryService> _mockedTelemetry = null!;

        private OmcConfiguration _configuration = null!;
        private IProductScenario _scenario = null!;
        private string _traceId = string.Empty;

        #region Test data
        private static readonly Guid s_productId = Guid.Parse("da0df49a-cd71-4e24-9bae-5be8b01f2c36");
        private static readonly Uri s_productUri = new($"https://openproduct.test/producten/api/v1/producten/{s_productId}");
        private static readonly Uri s_partyUri = new("https://openklant.test/partijen/22222222-2222-2222-2222-222222222222");

        // ZGW_WHITELIST_PRODUCTCREATE_IDS is "1, 2, 3" in the test configuration.
        private const string WhitelistedProductTypeCode = "1";

        // The "referentie" the scenario asks OpenKlant to prefer.
        private const string Portaalvoorkeur = "portaalvoorkeur";
        #endregion

        [OneTimeSetUp]
        public void SetupTests()
            => this._configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.BothValid_v2);

        [OneTimeTearDown]
        public void CleanUpTests() => this._configuration.Dispose();

        [SetUp]
        public void ResetMocks()
        {
            // A real trace, so the identifier the reference carries can be asserted against the one this
            // notification is actually being processed under.
            this._traceId = TraceContext.Start(new TraceEmitter());

            this._mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);

            Mock<IDataQueryService<NotificationEvent>> mockedDataQuery = new(MockBehavior.Strict);
            mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            this._mockedNotifyClient = new Mock<INotifyClient>(MockBehavior.Strict);
            this._mockedNotifyClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            this._mockedNotifyClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(this._mockedNotifyClient.Object);

            this._mockedSerializer = new Mock<ISerializationService>(MockBehavior.Strict);
            this._mockedSerializer
                .Setup(mock => mock.Serialize(It.IsAny<ProductNotifyReference>()))
                .Returns("{}");

            this._mockedTelemetry = new Mock<ITelemetryService>(MockBehavior.Strict);

            this._scenario = new ProductScenarioImplementation(
                mockedDataQuery.Object,
                this._mockedNotifyClientFactory.Object,
                this._mockedSerializer.Object,
                this._mockedTelemetry.Object,
                this._configuration,
                NullLogger<ProductScenarioImplementation>.Instance);
        }

        [TearDown]
        public void ClearTrace() => TraceContext.Clear();

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

        /// <summary>
        /// Arranges a product that passes every gate, with one owner per e-mail address given. An empty
        /// address is an owner whose party resolved but carries no e-mail.
        /// </summary>
        private void SetupProductWithOwners(params string[] emailAddresses)
        {
            List<Owner> owners = [];

            for (int index = 0; index < emailAddresses.Length; index++)
            {
                string bsn = $"99999001{index}";

                owners.Add(new Owner { BsnNumber = bsn });

                this._mockedQueryContext
                    .Setup(mock => mock.GetPartyDataByIdentifierAsync(
                        "bsn", bsn, Portaalvoorkeur, false, DistributionChannels.Email))
                    .ReturnsAsync(new CommonPartyData
                    {
                        Uri = s_partyUri,
                        Name = "Jane",
                        SurnamePrefix = "de",
                        Surname = "Vries",
                        EmailAddress = emailAddresses[index]
                    });
            }

            this._mockedQueryContext
                .Setup(mock => mock.GetProductAsync(s_productUri))
                .ReturnsAsync(new Product
                {
                    Id = s_productId,
                    Uri = s_productUri,
                    Name = "verhuurvergunning: straatweg 14",
                    Status = "gereed",
                    IsPublished = true,
                    ProductType = new NestedProductType
                    {
                        Code = WhitelistedProductTypeCode,
                        Name = "Parkeervergunning"
                    },
                    Owners = [.. owners]
                });
        }

        private void SetupSendSucceeds()
            => this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

        private void SetupContactMomentSucceeds()
            => this._mockedTelemetry
                .Setup(mock => mock.ReportProductCompletionAsync(
                    It.IsAny<ProductNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Success(string.Empty));

        private void VerifySendCount(Times times)
            => this._mockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), times);

        private void VerifyContactMomentCount(Times times)
            => this._mockedTelemetry.Verify(mock => mock.ReportProductCompletionAsync(
                It.IsAny<ProductNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()), times);
        #endregion

        [Test]
        public async Task ProcessProductAsync_ReachableOwner_SendsAndRegistersNothingYet()
        {
            // NOTE: "Notify NL" accepting the request is not delivery. The contactmoment is written from
            //       the delivery receipt, so writing one here would claim a contact that may never happen.

            // Arrange
            SetupProductWithOwners("jane@example.com");
            SetupSendSucceeds();

            // Act
            HttpRequestResponse response = await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);

                VerifySendCount(Times.Once());
                VerifyContactMomentCount(Times.Never());
            });
        }

        [Test]
        public async Task ProcessProductAsync_ReachableOwner_SendsTheProductReferenceToNotify()
        {
            // NOTE: The reference is the only state that survives to the delivery receipt. Without it the
            //       receipt comes back unrecognisable, the callback responder cannot tell it is a product,
            //       and the contactmoment for a notification that did arrive is never written. That fails
            //       silently - the send succeeds and nothing complains - so it is pinned here.

            // Arrange
            SetupProductWithOwners("jane@example.com");

            ProductNotifyReference captured = default;
            string sentReference = string.Empty;

            this._mockedSerializer
                .Setup(mock => mock.Serialize(It.IsAny<ProductNotifyReference>()))
                .Callback<ProductNotifyReference>(reference => captured = reference)
                .Returns("{}");

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .Callback<string, string, Dictionary<string, object>, string>(
                    (_, _, _, reference) => sentReference = reference)
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(captured.ProductId, Is.EqualTo(s_productId));
                Assert.That(captured.PartyId, Is.EqualTo(s_partyUri.GetGuid()));
                Assert.That(captured.ProductName, Is.EqualTo("verhuurvergunning: straatweg 14"));
                Assert.That(captured.ProductTypeCode, Is.EqualTo(WhitelistedProductTypeCode));
                Assert.That(captured.OriginalResourceUrl, Is.EqualTo(s_productUri.AbsoluteUri));

                // Read from the ambient trace: the send runs on the same call chain as the notification.
                Assert.That(captured.TraceId, Is.EqualTo(this._traceId));

                Assert.That(sentReference, Is.Not.Empty);
            });
        }

        [Test]
        public async Task ProcessProductAsync_UnreachableOwner_RegistersAFailedContactMomentWithoutSending()
        {
            // Arrange
            SetupProductWithOwners(string.Empty);
            SetupContactMomentSucceeds();

            // Act
            HttpRequestResponse response = await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                // An owner nobody can reach was never a reason to answer differently: the party resolved,
                // which is all the answer was ever based on.
                Assert.That(response.IsSuccess, Is.True);

                VerifySendCount(Times.Never());

                this._mockedTelemetry.Verify(mock => mock.ReportProductCompletionAsync(
                    It.Is<ProductNotifyReference>(reference => reference.ProductId == s_productId),
                    NotifyMethods.Email,
                    It.Is<string[]>(messages => messages[2] == "false")), Times.Once);
            });
        }

        [Test]
        public async Task ProcessProductAsync_SendRefused_RegistersAFailedContactMomentAndStillReportsSuccess()
        {
            // NOTE: The status was settled before the first send went out. "Notify NL" refusing one is
            //       reported as a contactmoment; reporting it as a failed notification instead would have
            //       the whole thing redelivered, re-notifying every owner who did receive theirs.

            // Arrange
            SetupProductWithOwners("jane@example.com");
            SetupContactMomentSucceeds();

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("Template not found"));

            // Act
            HttpRequestResponse response = await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);

                this._mockedTelemetry.Verify(mock => mock.ReportProductCompletionAsync(
                    It.IsAny<ProductNotifyReference>(), NotifyMethods.Email,
                    It.Is<string[]>(messages => messages[2] == "false")), Times.Once);
            });
        }

        [Test]
        public async Task ProcessProductAsync_SendThrows_StillNotifiesTheRemainingOwners()
        {
            // NOTE: One owner is not allowed to take the others down with it. The whole reason parties are
            //       validated up front is that every failure from here on can be recorded per owner.

            // Arrange
            SetupProductWithOwners("first@example.com", "second@example.com");
            SetupContactMomentSucceeds();

            this._mockedNotifyClient
                .SetupSequence(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Notify is down"))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse response = await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);

                VerifySendCount(Times.Exactly(2));
                VerifyContactMomentCount(Times.Once());
            });
        }

        [Test]
        public async Task ProcessProductAsync_RegisteringTheFailureThrows_StillReportsSuccess()
        {
            // NOTE: Nothing the delivery runs into may escape. Losing the record of a notification that
            //       failed is bad; turning it into a non-2xx is worse, because the notification is then
            //       redelivered and every owner who was reached is notified a second time.

            // Arrange
            SetupProductWithOwners(string.Empty, "second@example.com");
            SetupSendSucceeds();

            this._mockedTelemetry
                .Setup(mock => mock.ReportProductCompletionAsync(
                    It.IsAny<ProductNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()))
                .ThrowsAsync(new HttpRequestException("OpenKlant is down"));

            // Act
            HttpRequestResponse response = await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);

                // And the owner behind the failed registration is still notified.
                VerifySendCount(Times.Once());
            });
        }

        [Test]
        public async Task ProcessProductAsync_MixedOwners_SendsToTheReachableAndRecordsTheRest()
        {
            // Arrange
            SetupProductWithOwners("jane@example.com", string.Empty);
            SetupSendSucceeds();
            SetupContactMomentSucceeds();

            // Act
            HttpRequestResponse response = await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);

                VerifySendCount(Times.Once());
                VerifyContactMomentCount(Times.Once());
            });
        }

        [Test]
        public async Task ProcessProductAsync_ReachableOwner_PersonalizesWithTheProductAndTheParty()
        {
            // Arrange
            SetupProductWithOwners("jane@example.com");

            Dictionary<string, object>? captured = null;

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .Callback<string, string, Dictionary<string, object>, string>(
                    (_, _, personalization, _) => captured = personalization)
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(captured!["klant.voornaam"], Is.EqualTo("Jane"));
                Assert.That(captured["klant.voorvoegselAchternaam"], Is.EqualTo("de"));
                Assert.That(captured["klant.achternaam"], Is.EqualTo("Vries"));
                Assert.That(captured["producttype.naam"], Is.EqualTo("Parkeervergunning"));
                Assert.That(captured["product.naam"], Is.EqualTo("verhuurvergunning: straatweg 14"));
                Assert.That(captured["product.status"], Is.EqualTo("gereed"));
            });
        }

        [Test]
        public async Task ProcessProductAsync_TwoOwners_BuildsAPersonalizationDictionaryPerOwner()
        {
            // NOTE: The other scenarios hand back a shared static dictionary guarded by a lock that covers
            //       writing it but not the reading of it by the caller. Survivable for one recipient; for
            //       a fan-out it means one owner can be sent the details of another.

            // Arrange
            SetupProductWithOwners("first@example.com", "second@example.com");

            List<Dictionary<string, object>> captured = [];

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .Callback<string, string, Dictionary<string, object>, string>(
                    (_, _, personalization, _) => captured.Add(personalization))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            await this._scenario.ProcessProductAsync(GetProductNotification());

            // Assert
            Assert.That(captured[0], Is.Not.SameAs(captured[1]));
        }

        [Test]
        public void ProcessProductAsync_OneOwnerWithoutAParty_SendsToNobody()
        {
            // NOTE: The dividing line between the two halves, and the reason the second half can afford to
            //       swallow everything. This failure is decided before the answer, so it aborts - and the
            //       owner that did resolve goes un-notified, which is only acceptable because not a single
            //       send has gone out by then.

            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.GetProductAsync(s_productUri))
                .ReturnsAsync(new Product
                {
                    Id = s_productId,
                    Uri = s_productUri,
                    IsPublished = true,
                    ProductType = new NestedProductType { Code = WhitelistedProductTypeCode },
                    Owners =
                    [
                        new Owner { BsnNumber = "999990010" },
                        new Owner { BsnNumber = "999990099" }
                    ]
                });

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataByIdentifierAsync(
                    "bsn", "999990010", Portaalvoorkeur, false, DistributionChannels.Email))
                .ReturnsAsync(new CommonPartyData { Uri = s_partyUri, EmailAddress = "jane@example.com" });

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataByIdentifierAsync(
                    "bsn", "999990099", Portaalvoorkeur, false, DistributionChannels.Email))
                .ThrowsAsync(new HttpRequestException("No party results"));

            // Act & Assert
            Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(GetProductNotification()));

            Assert.Multiple(() =>
            {
                VerifySendCount(Times.Never());
                VerifyContactMomentCount(Times.Never());
            });
        }
    }
}
