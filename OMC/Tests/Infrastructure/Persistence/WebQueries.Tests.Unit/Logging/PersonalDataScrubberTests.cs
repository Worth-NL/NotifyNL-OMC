// © 2026, Worth Systems.

using NUnit.Framework;
using WebQueries.Logging;

namespace WebQueries.Tests.Unit.Logging
{
    [TestFixture]
    public sealed class PersonalDataScrubberTests
    {
        [TestCase("GET https://openklant.test/partijen?partijIdentificator__codeSoortObjectId=bsn&partijIdentificator__objectId=123456789&expand=digitaleAdressen",
                  "GET https://openklant.test/partijen?partijIdentificator__codeSoortObjectId=bsn&partijIdentificator__objectId=[…]&expand=digitaleAdressen")]
        [TestCase("partijIdentificator__objectId=12345678", "partijIdentificator__objectId=[…]")]  // KVK number
        [TestCase("vestigingsnummer 000012345678", "vestigingsnummer […]")]
        [TestCase("\"ontvanger\":\"urn:nld:bsn:nummer:123456789\"", "\"ontvanger\":\"urn:nld:bsn:nummer:[…]\"")]
        [TestCase("\"objectId\":\"123456789\"", "\"objectId\":\"[…]\"")]
        public void Scrub_MasksBsnKvkAndVestigingsnummer(string text, string expected)
        {
            Assert.That(PersonalDataScrubber.Scrub(text), Is.EqualTo(expected));
        }

        [TestCase("https://openklant.test/partijen/12345678-1234-1234-1234-123456789012")]  // UUID
        [TestCase("404 NotFound | Bericht 55555555-5555-5555-5555-555555555555 not found")]
        [TestCase("Retried 3 times, waited 1234567 ms")]  // Fewer than 8 digits
        [TestCase("")]
        [TestCase(null)]
        public void Scrub_LeavesTheRestAlone(string? text)
        {
            Assert.That(PersonalDataScrubber.Scrub(text), Is.EqualTo(text));
        }
    }
}
