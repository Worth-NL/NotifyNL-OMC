// © 2026, Worth Systems.

using NUnit.Framework;
using ZgwModels.Mapping.Enums.Urns;
using ZgwModels.Mapping.Models.Urns;

namespace ZgwModels.Tests.Unit.Mapping.Models.Urns
{
    [TestFixture]
    public sealed class BetrokkeneUrnTests
    {
        #region TryParse (success)
        // The layout used by the print objecttype (#237's payload example, with the BSN equivalent).
        [TestCase("urn:nld:bsn:nummer:123456782", "123456782")]
        // The layout the MOBB CloudEvent path already produces - value straight after the namespace.
        [TestCase("urn:nld:bsn:123456782", "123456782")]
        // Casing is not meaningful in a URN's namespace identifiers.
        [TestCase("URN:NLD:BSN:NUMMER:123456782", "123456782")]
        public void TryParse_BsnUrn_ReturnsBsnNamespaceAndValue(string testUrn, string expectedValue)
        {
            // Act
            bool actualResult = BetrokkeneUrn.TryParse(testUrn, out BetrokkeneUrn actualUrn);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Is.True);
                Assert.That(actualUrn.Namespace, Is.EqualTo(UrnNamespaces.Bsn));
                Assert.That(actualUrn.Value, Is.EqualTo(expectedValue));
            });
        }

        [Test]
        public void TryParse_KvkUrn_ReturnsKvkNamespaceAndValue()
        {
            // Arrange
            const string testUrn = "urn:nld:hr:kvk:nummer:12345678";

            // Act
            bool actualResult = BetrokkeneUrn.TryParse(testUrn, out BetrokkeneUrn actualUrn);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Is.True);
                Assert.That(actualUrn.Namespace, Is.EqualTo(UrnNamespaces.Kvk));
                Assert.That(actualUrn.Value, Is.EqualTo("12345678"));
                Assert.That(actualUrn.NamespaceSegment, Is.EqualTo("kvk"));
            });
        }

        [Test]
        public void TryParse_UnknownNamespace_ParsesButReportsUnknown()
        {
            // Arrange: a well-formed URN naming something OMC has no resolution path for.
            const string testUrn = "urn:nld:rsin:nummer:002564440";

            // Act
            bool actualResult = BetrokkeneUrn.TryParse(testUrn, out BetrokkeneUrn actualUrn);

            // Assert: parsing succeeds so the caller can name what it was handed, rather than only
            // reporting that something was unparseable.
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Is.True);
                Assert.That(actualUrn.Namespace, Is.EqualTo(UrnNamespaces.Unknown));
                Assert.That(actualUrn.Value, Is.EqualTo("002564440"));
            });
        }
        #endregion

        #region TryParse (failure)
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("https://example.com/bsn/123456782")]  // Not a URN at all
        [TestCase("urn:bsn")]                            // Too few segments to carry a value
        [TestCase("urn:nld:bsn:")]                       // Trailing separator, so no value
        public void TryParse_Unusable_ReturnsFalse(string? testUrn)
        {
            // Act
            bool actualResult = BetrokkeneUrn.TryParse(testUrn, out _);

            // Assert
            Assert.That(actualResult, Is.False);
        }
        #endregion
    }
}
