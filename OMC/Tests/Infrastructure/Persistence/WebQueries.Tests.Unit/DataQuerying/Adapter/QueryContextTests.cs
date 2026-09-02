// © 2026, Worth Systems.

using Common.Constants;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Adapter;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Besluiten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Documenten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Objecten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenVtb.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.KTO.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.Tests.Unit.DataQuerying.Adapter
{
    [TestFixture]
    public sealed class QueryContextTests
    {
        private const string TestBsn = "999990019";

        private Mock<IQueryBase> _mockedQueryBase = null!;
        private Mock<IQueryZaak> _mockedQueryZaak = null!;
        private Mock<IQueryKlant> _mockedQueryKlant = null!;
        private IQueryContext _queryContext = null!;

        [SetUp]
        public void ResetMocks()
        {
            this._mockedQueryBase = new Mock<IQueryBase>(MockBehavior.Strict);
            this._mockedQueryZaak = new Mock<IQueryZaak>(MockBehavior.Strict);
            this._mockedQueryKlant = new Mock<IQueryKlant>(MockBehavior.Strict);

            this._queryContext = new QueryContext(
                new Mock<IHttpNetworkService>(MockBehavior.Strict).Object,
                new Mock<IHttpNetworkServiceKto>(MockBehavior.Strict).Object,
                this._mockedQueryBase.Object,
                this._mockedQueryZaak.Object,
                this._mockedQueryKlant.Object,
                new Mock<IQueryBesluiten>(MockBehavior.Strict).Object,
                new Mock<IQueryObjecten>(MockBehavior.Strict).Object,
                new Mock<IQueryObjectTypen>(MockBehavior.Strict).Object,
                new Mock<IQueryVtb>(MockBehavior.Strict).Object,
                new Mock<IQueryDocumenten>(MockBehavior.Strict).Object);
        }

        [Test]
        public async Task GetPartyDataAsync_CaseUriNull_CreateIfMissingTrue_PassesThroughToQueryKlant()
        {
            // Arrange
            this._mockedQueryKlant
                .Setup(mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, null, false, true))
                .ReturnsAsync(new CommonPartyData { Uri = CommonValues.Default.Models.EmptyUri });

            // Act
            await this._queryContext.GetPartyDataAsync(
                caseUri: null, bsnNumber: TestBsn, requireDigitalAddress: false, createIfMissing: true);

            // Assert
            this._mockedQueryKlant.Verify(
                mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, null, false, true),
                Times.Once);
        }

        [Test]
        public async Task GetPartyDataAsync_CaseUriNull_CreateIfMissingFalse_PassesThroughAsFalse()
        {
            // Arrange: regression guard - the default-false path behaves exactly as it did before this feature.
            this._mockedQueryKlant
                .Setup(mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, null, false, false))
                .ReturnsAsync(new CommonPartyData { Uri = CommonValues.Default.Models.EmptyUri });

            // Act
            await this._queryContext.GetPartyDataAsync(
                caseUri: null, bsnNumber: TestBsn, requireDigitalAddress: false, createIfMissing: false);

            // Assert
            this._mockedQueryKlant.Verify(
                mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, null, false, false),
                Times.Once);
        }

        [Test]
        public async Task GetPartyDataAsync_CaseUriProvided_InvolvedPartyMissing_CreateIfMissingNotHonored()
        {
            // Arrange: a case-linked citizen lookup (no direct "betrokkene" party URI on the case role) still
            // falls back to a BSN-based OpenKlant lookup - but createIfMissing must NOT reach it, proving the
            // feature is scoped to the direct (no-case) BSN lookup path only, regardless of what a caller
            // asks for on the case-role branch.
            var caseUri = new Uri("https://test.domain/api/v1/zaken/11111111-1111-1111-1111-111111111111");

            this._mockedQueryZaak
                .Setup(mock => mock.GetCaseRoleAsync(this._mockedQueryBase.Object, caseUri))
                .ReturnsAsync(new CaseRole { InvolvedPartyUri = CommonValues.Default.Models.EmptyUri });

            this._mockedQueryKlant
                .Setup(mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, null, true, false))
                .ReturnsAsync(new CommonPartyData { Uri = CommonValues.Default.Models.EmptyUri });

            // Act: the caller asks for createIfMissing: true, but this is the case-role branch, not the
            // direct BSN lookup - the BSN is already supplied, so GetBsnNumberAsync (OpenZaak) is skipped.
            await this._queryContext.GetPartyDataAsync(caseUri: caseUri, bsnNumber: TestBsn, createIfMissing: true);

            // Assert: IQueryKlant was called with createIfMissing staying false, not the true the caller asked for.
            this._mockedQueryKlant.Verify(
                mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, TestBsn, null, true, false),
                Times.Once);
        }

        [Test]
        public async Task GetPartyDataAsync_CaseUriProvided_InvolvedPartyPresent_UsesUriOverload_CreateIfMissingHasNoEffect()
        {
            // Arrange: when the case role carries a direct "betrokkene" party URI, resolution goes through
            // the Uri-keyed IQueryKlant overload entirely - which has no createIfMissing parameter at all.
            var caseUri = new Uri("https://test.domain/api/v1/zaken/22222222-2222-2222-2222-222222222222");
            var partyUri = new Uri("https://test.domain/api/v1/partijen/33333333-3333-3333-3333-333333333333");

            this._mockedQueryZaak
                .Setup(mock => mock.GetCaseRoleAsync(this._mockedQueryBase.Object, caseUri))
                .ReturnsAsync(new CaseRole { InvolvedPartyUri = partyUri });

            this._mockedQueryKlant
                .Setup(mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, partyUri, null))
                .ReturnsAsync(new CommonPartyData { Uri = partyUri });

            // Act
            await this._queryContext.GetPartyDataAsync(caseUri: caseUri, createIfMissing: true);

            // Assert
            this._mockedQueryKlant.Verify(
                mock => mock.TryGetPartyDataAsync(this._mockedQueryBase.Object, partyUri, null),
                Times.Once);
        }
    }
}
