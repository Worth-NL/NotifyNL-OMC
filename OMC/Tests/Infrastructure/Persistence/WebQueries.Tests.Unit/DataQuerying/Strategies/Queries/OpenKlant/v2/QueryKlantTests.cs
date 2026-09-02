// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.v2;
using WebQueries.DataSending.Clients.Enums;
using ZgwModels.Mapping.Enums.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.v2;

namespace WebQueries.Tests.Unit.DataQuerying.Strategies.Queries.OpenKlant.v2
{
    [TestFixture]
    public sealed class QueryKlantTests
    {
        private const string TestBsn = "999990019";

        private OmcConfiguration _configuration = null!;
        private Mock<IQueryBase> _mockedQueryBase = null!;
        private IQueryKlant _queryKlant = null!;

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
            this._queryKlant = new QueryKlant(this._configuration);
        }

        [OneTimeTearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        #region Helpers
        private static PartyResults GetEmptyPartyResults()
            => new() { Count = 0, Results = [] };

        private static PartyResults GetSingleBareParty(Uri partyUri)
            => new()
            {
                Count = 1,
                Results =
                [
                    new PartyResult
                    {
                        Uri = partyUri,
                        PreferredDigitalAddress = null,
                        Identification = new PartyIdentification { Details = new PartyDetails() },
                        Expansion = new Expansion { DigitalAddresses = [] }  // Freshly created party: no digital address
                    }
                ]
            };
        #endregion

        [Test]
        public async Task TryGetPartyDataAsync_CreateIfMissingFalse_EmptyResults_ThrowsHttpRequestException_WithoutPosting()
        {
            // Arrange
            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetEmptyPartyResults());

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(() =>
                this._queryKlant.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, requireDigitalAddress: false, createIfMissing: false));

            this._mockedQueryBase.Verify(
                mock => mock.ProcessPostAsync<PartyCreationResult>(It.IsAny<HttpClientTypes>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task TryGetPartyDataAsync_CreateIfMissingTrue_NonEmptyResults_DoesNotCreateParty()
        {
            // Arrange
            var partyUri = new Uri("https://test.domain/api/v1/partijen/11111111-1111-1111-1111-111111111111");

            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetSingleBareParty(partyUri));

            // Act
            CommonPartyData result = await this._queryKlant.TryGetPartyDataAsync(
                this._mockedQueryBase.Object, TestBsn, requireDigitalAddress: false, createIfMissing: true);

            // Assert
            Assert.That(result.Uri, Is.EqualTo(partyUri));

            this._mockedQueryBase.Verify(
                mock => mock.ProcessPostAsync<PartyCreationResult>(It.IsAny<HttpClientTypes>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            this._mockedQueryBase.Verify(
                mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task TryGetPartyDataAsync_CreateIfMissingFalse_NonEmptyResults_ReturnsExistingParty_Unaffected()
        {
            // Arrange: regression guard - the default-false path behaves exactly as it did before this feature.
            var partyUri = new Uri("https://test.domain/api/v1/partijen/22222222-2222-2222-2222-222222222222");

            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetSingleBareParty(partyUri));

            // Act
            CommonPartyData result = await this._queryKlant.TryGetPartyDataAsync(
                this._mockedQueryBase.Object, TestBsn, requireDigitalAddress: false, createIfMissing: false);

            // Assert
            Assert.That(result.Uri, Is.EqualTo(partyUri));

            this._mockedQueryBase.Verify(
                mock => mock.ProcessPostAsync<PartyCreationResult>(It.IsAny<HttpClientTypes>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task TryGetPartyDataAsync_CreateIfMissingTrue_EmptyResults_CreatesPartyThenRefetches_ReturnsCreatedParty()
        {
            // Arrange
            var createdPartyUri = new Uri("https://test.domain/api/v1/partijen/33333333-3333-3333-3333-333333333333");

            this._mockedQueryBase
                .SetupSequence(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetEmptyPartyResults())                       // First lookup: nobody home yet
                .ReturnsAsync(GetSingleBareParty(createdPartyUri));         // Re-fetch after creation: found

            this._mockedQueryBase
                .Setup(mock => mock.ProcessPostAsync<PartyCreationResult>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PartyCreationResult { Uri = createdPartyUri });

            // Act
            CommonPartyData result = await this._queryKlant.TryGetPartyDataAsync(
                this._mockedQueryBase.Object, TestBsn, requireDigitalAddress: false, createIfMissing: true);

            // Assert: the created, address-less party is returned via the existing "no digital address on
            // file" fallback (PartyResults.Party), unchanged.
            Assert.Multiple(() =>
            {
                Assert.That(result.Uri, Is.EqualTo(createdPartyUri));
                Assert.That(result.DistributionChannel, Is.EqualTo(DistributionChannels.Unknown));
                Assert.That(result.EmailAddress, Is.Empty);
                Assert.That(result.TelephoneNumber, Is.Empty);
            });

            this._mockedQueryBase.Verify(
                mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()),
                Times.Exactly(2));
            this._mockedQueryBase.Verify(
                mock => mock.ProcessPostAsync<PartyCreationResult>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void TryGetPartyDataAsync_CreateIfMissingTrue_CreatePartyThrows_PropagatesWithoutRetry()
        {
            // Arrange
            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetEmptyPartyResults());

            this._mockedQueryBase
                .Setup(mock => mock.ProcessPostAsync<PartyCreationResult>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Party creation failed"));

            // Act & Assert: no swallow-and-retry - a create failure propagates directly.
            Assert.ThrowsAsync<HttpRequestException>(() =>
                this._queryKlant.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, requireDigitalAddress: false, createIfMissing: true));

            // Only the initial lookup happened - no re-fetch was attempted after the failed create.
            this._mockedQueryBase.Verify(
                mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task TryGetPartyDataAsync_CreateIfMissingTrue_EmptyResults_PostsExpectedJsonBody()
        {
            // Arrange
            var createdPartyUri = new Uri("https://test.domain/api/v1/partijen/44444444-4444-4444-4444-444444444444");
            string? capturedJsonBody = null;

            this._mockedQueryBase
                .SetupSequence(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetEmptyPartyResults())
                .ReturnsAsync(GetSingleBareParty(createdPartyUri));

            this._mockedQueryBase
                .Setup(mock => mock.ProcessPostAsync<PartyCreationResult>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<HttpClientTypes, Uri, string, string>((_, _, jsonBody, _) => capturedJsonBody = jsonBody)
                .ReturnsAsync(new PartyCreationResult { Uri = createdPartyUri });

            // Act
            await this._queryKlant.TryGetPartyDataAsync(
                this._mockedQueryBase.Object, TestBsn, requireDigitalAddress: false, createIfMissing: true);

            // Assert: only what's required, plus the BSN identifier - no name/address/voorkeurstaal.
            Assert.That(capturedJsonBody, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedJsonBody, Does.Contain("\"soortPartij\":\"persoon\""));
                Assert.That(capturedJsonBody, Does.Contain("\"indicatieActief\":true"));
                Assert.That(capturedJsonBody, Does.Contain("\"codeObjecttype\":\"natuurlijk_persoon\""));
                Assert.That(capturedJsonBody, Does.Contain("\"codeSoortObjectId\":\"bsn\""));
                Assert.That(capturedJsonBody, Does.Contain("\"codeRegister\":\"brp\""));
                Assert.That(capturedJsonBody, Does.Contain($"\"objectId\":\"{TestBsn}\""));
            });
        }
    }
}
