// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataSending.Models.DTOs;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;

namespace WebQueries.Tests.Unit.DataSending.Models
{
    [TestFixture]
    public sealed class PreparedDataTests
    {
        [Test]
        public void Constructor_SetsPartyAndCaseUri()
        {
            CommonPartyData party = new() { Name = "Test", Surname = "User" };
            Uri caseUri = new("https://example.com/cases/1");

            PreparedData data = new(party, caseUri);

            Assert.Multiple(() =>
            {
                Assert.That(data.Party, Is.EqualTo(party));
                Assert.That(data.CaseUri, Is.EqualTo(caseUri));
            });
        }

        [Test]
        public void Constructor_WithNullCaseUri_CaseUriIsNull()
        {
            CommonPartyData party = new();

            PreparedData data = new(party, null);

            Assert.That(data.CaseUri, Is.Null);
        }
    }
}
