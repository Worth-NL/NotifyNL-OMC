// © 2026, Worth Systems.

using NUnit.Framework;
using WebQueries.Tracing;

namespace WebQueries.Tests.Unit.Tracing
{
    [TestFixture]
    public sealed class TraceDetailSanitizerTests
    {
        [Test]
        public void Sanitize_FailedRegisterCall_DropsTheUriAndResponseBody()
        {
            // NOTE: The exact shape QueryBase.GetMessage produces. The URI of a party search holds the BSN.
            const string detail =
                "HTTP Request: no party results | URI: https://openklant.test/klantinteracties/api/v1/partijen" +
                "?partijIdentificator__codeSoortObjectId=bsn&partijIdentificator__objectId=999990019 | JSON response: {\"bsn\":\"999990019\"}";

            string? result = TraceDetailSanitizer.Sanitize(detail);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo("HTTP Request: no party results"));
                Assert.That(result, Does.Not.Contain("999990019"));
            });
        }

        [TestCase("Lookup for 999990019 failed", "Lookup for […] failed")]          // BSN (9 digits)
        [TestCase("Organisation 12345678 not found", "Organisation […] not found")]  // KVK number (8 digits)
        [TestCase("Vestiging 000012345678", "Vestiging […]")]                         // vestigingsnummer (12 digits)
        public void Sanitize_LongDigitRuns_AreMasked(string detail, string expected)
        {
            Assert.That(TraceDetailSanitizer.Sanitize(detail), Is.EqualTo(expected));
        }

        [Test]
        public void Sanitize_Uuids_AreLeftAlone()
        {
            // NOTE: An all-digit UUID segment is not a BSN; masking it would make the detail useless.
            const string detail = "bericht 12345678-1234-1234-1234-123456789012 retrieved";

            Assert.That(TraceDetailSanitizer.Sanitize(detail), Is.EqualTo(detail));
        }

        [Test]
        public void Sanitize_UrlQueryStrings_AreStripped()
        {
            const string detail = "GET https://openklant.test/partijen?partijIdentificator__objectId=abc&expand=x failed";

            Assert.That(TraceDetailSanitizer.Sanitize(detail), Is.EqualTo("GET https://openklant.test/partijen failed"));
        }

        [Test]
        public void Sanitize_LongDetail_IsCapped()
        {
            string detail = new('x', 1000);

            string? result = TraceDetailSanitizer.Sanitize(detail);

            Assert.That(result!.Length, Is.LessThanOrEqualTo(301));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Sanitize_NoDetail_StaysEmpty(string? detail)
        {
            Assert.That(TraceDetailSanitizer.Sanitize(detail), Is.EqualTo(detail));
        }

        [Test]
        public void Sanitize_OrdinaryDetail_IsUnchanged()
        {
            const string detail = "Product type \"PARKEERVERGUNNING-A\" is not whitelisted in ZGW_WHITELIST_PRODUCTCREATE_IDS.";

            Assert.That(TraceDetailSanitizer.Sanitize(detail), Is.EqualTo(detail));
        }
    }
}
