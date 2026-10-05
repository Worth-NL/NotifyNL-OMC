// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Moq;
using System.Net;
using NUnit.Framework;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.v2;
using WebQueries.DataSending.Clients.Enums;
using ZgwModels.Exceptions;
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
        public async Task TryGetPartyDataAsync_CreateIfMissingFalse_EmptyResults_ThrowsPartyNotFoundException_WithoutPosting()
        {
            // Arrange
            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetEmptyPartyResults());

            // Act & Assert
            Assert.ThrowsAsync<PartyNotFoundException>(() =>
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

        #region Vestiging
        [Test]
        public async Task TryGetBranchPartyDataAsync_SearchesTheVestigingsnummerScopedUnderItsKvkNumber()
        {
            // NOTE: A vestigingsnummer is only unique under its KVK number, so both have to be in the query:
            //       the vestiging as the partijIdentificator, its organisation as the subIdentificatorVan.

            // Arrange
            const string kvkNumber = "12345678";
            const string branchNumber = "000012345678";
            Uri partyUri = new($"https://openklant.test/klantinteracties/api/v1/partijen/{Guid.NewGuid()}");
            Uri? requestedUri = null;

            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .Callback<HttpClientTypes, Uri, string>((_, uri, _) => requestedUri = uri)
                .ReturnsAsync(GetSingleBareParty(partyUri));

            // Act
            await this._queryKlant.TryGetBranchPartyDataAsync(
                this._mockedQueryBase.Object, kvkNumber, branchNumber, requireDigitalAddress: false);

            // Assert
            string query = requestedUri!.Query;

            Assert.Multiple(() =>
            {
                Assert.That(query, Does.Contain("partijIdentificator__codeSoortObjectId=vestigingsnummer"));
                Assert.That(query, Does.Contain($"partijIdentificator__objectId={branchNumber}"));
                Assert.That(query, Does.Contain("subIdentificatorVan__codeSoortObjectId=kvk_nummer"));
                Assert.That(query, Does.Contain($"subIdentificatorVan__objectId={kvkNumber}"));
            });
        }

        [Test]
        public void TryGetBranchPartyDataAsync_EmptyResults_ThrowsPartyNotFound_NamingTheVestigingsnummer()
        {
            // Arrange
            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetEmptyPartyResults());

            // Act & Assert
            PartyNotFoundException? exception = Assert.ThrowsAsync<PartyNotFoundException>(() =>
                this._queryKlant.TryGetBranchPartyDataAsync(this._mockedQueryBase.Object, "12345678", "000012345678"));

            Assert.That(exception!.IdentifierKind, Is.EqualTo("vestigingsnummer"));
        }

        [Test]
        public async Task TryGetPartyDataByIdentifierAsync_KvkNumberAlone_DoesNotScopeTheSearch()
        {
            // NOTE: A KVK number on its own finds the organisation, which carries no subIdentificatorVan.

            // Arrange
            Uri partyUri = new($"https://openklant.test/klantinteracties/api/v1/partijen/{Guid.NewGuid()}");
            Uri? requestedUri = null;

            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .Callback<HttpClientTypes, Uri, string>((_, uri, _) => requestedUri = uri)
                .ReturnsAsync(GetSingleBareParty(partyUri));

            // Act
            await this._queryKlant.TryGetPartyDataByIdentifierAsync(
                this._mockedQueryBase.Object, "kvk_nummer", "12345678", requireDigitalAddress: false);

            // Assert
            Assert.That(requestedUri!.Query, Does.Not.Contain("subIdentificatorVan"));
        }
        #endregion

        #region Party not found
        [TestCase("bsn", "BSN")]
        [TestCase("kvk_nummer", "KVK number")]
        public void TryGetPartyDataByIdentifierAsync_EmptyResults_ThrowsPartyNotFound_NamingTheIdentifierKind(
            string codeSoortObjectId, string expectedKind)
        {
            // Arrange
            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(GetEmptyPartyResults());

            // Act & Assert
            PartyNotFoundException? exception = Assert.ThrowsAsync<PartyNotFoundException>(() =>
                this._queryKlant.TryGetPartyDataByIdentifierAsync(this._mockedQueryBase.Object, codeSoortObjectId, "12345678"));

            Assert.Multiple(() =>
            {
                Assert.That(exception!.IdentifierKind, Is.EqualTo(codeSoortObjectId));
                Assert.That(exception.Message, Does.Contain($"No partij found in OpenKlant for this {expectedKind}"));

                // The value itself is personal data (a BSN) and must never reach the message.
                Assert.That(exception.Message, Does.Not.Contain("12345678"));
            });
        }

        [Test]
        public void TryGetPartyDataAsync_ByPartyUri_NotFound_ThrowsPartyNotFound_NamingThePartyId()
        {
            // NOTE: The case role path: the zaak points at a partij by its URL, and OpenKlant answers 404.

            // Arrange
            Guid partyId = Guid.NewGuid();
            Uri partyUri = new($"https://openklant.test/klantinteracties/api/v1/partijen/{partyId}");

            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResult>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));

            // Act & Assert
            PartyNotFoundException? exception = Assert.ThrowsAsync<PartyNotFoundException>(() =>
                this._queryKlant.TryGetPartyDataAsync(this._mockedQueryBase.Object, partyUri));

            Assert.Multiple(() =>
            {
                Assert.That(exception!.IdentifierKind, Is.EqualTo("uuid"));
                Assert.That(exception.Message, Does.Contain(partyId.ToString()));
            });
        }

        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(null)]
        public void TryGetPartyDataAsync_ByPartyUri_OpenKlantUnreachable_IsNotReportedAsPartyNotFound(HttpStatusCode? statusCode)
        {
            // NOTE: Only a 404 means the partij is not there. Anything else is OpenKlant failing, which has to
            //       stay a plain failure so the notification is retried.

            // Arrange
            Uri partyUri = new($"https://openklant.test/klantinteracties/api/v1/partijen/{Guid.NewGuid()}");

            this._mockedQueryBase
                .Setup(mock => mock.ProcessGetAsync<PartyResult>(HttpClientTypes.OpenKlant_v2, It.IsAny<Uri>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Boom", null, statusCode));

            // Act & Assert
            HttpRequestException? exception = Assert.ThrowsAsync<HttpRequestException>(() =>
                this._queryKlant.TryGetPartyDataAsync(this._mockedQueryBase.Object, partyUri));

            Assert.That(exception, Is.Not.InstanceOf<PartyNotFoundException>());
        }
        #endregion
    }
}
