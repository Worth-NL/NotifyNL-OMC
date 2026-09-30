// © 2026, Worth Systems.

using NUnit.Framework;
using System.Text.Json;
using ZgwModels.Mapping.Enums.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.Converters;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.v2;

namespace ZgwModels.Tests.Unit.Mapping.Models.POCOs.OpenKlant.v2
{
    [TestFixture]
    public sealed class PartyIdentificationTests
    {
        [Test]
        public void Deserialize_Person_ReadsContactName()
        {
            // Arrange
            const string json = """{ "contactnaam": { "voornaam": "Samantha", "voorvoegselAchternaam": "van", "achternaam": "Rogers" } }""";

            // Act
            PartyIdentification identification = JsonSerializer.Deserialize<PartyIdentification>(json);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(identification.Details.Name, Is.EqualTo("Samantha"));
                Assert.That(identification.Details.SurnamePrefix, Is.EqualTo("van"));
                Assert.That(identification.Details.Surname, Is.EqualTo("Rogers"));
                Assert.That(identification.OrganizationName, Is.Empty);
            });
        }

        [Test]
        public void Deserialize_Organization_ReadsNameAndDoesNotRequireContactName()
        {
            // Arrange - an organization (e.g. one identified by its KVK number) has "naam", never "contactnaam"
            const string json = """{ "naam": "Bakkerij Rogers B.V." }""";

            // Act
            PartyIdentification identification = JsonSerializer.Deserialize<PartyIdentification>(json);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(identification.OrganizationName, Is.EqualTo("Bakkerij Rogers B.V."));
                // NOTE: An absent "contactnaam" leaves the struct at its default, whose strings are null
                Assert.That(identification.Details.Name, Is.Null.Or.Empty);
                Assert.That(identification.Details.Surname, Is.Null.Or.Empty);
            });
        }

        [Test]
        public void ConvertToUnified_Organization_PutsItsNameWhereASurnameGoes()
        {
            // Arrange
            var party = new PartyResult
            {
                Identification = new PartyIdentification { OrganizationName = "Bakkerij Rogers B.V." }
            };

            // Act
            CommonPartyData unified = (party, DistributionChannels.Email, "info@example.com", string.Empty, string.Empty)
                .ConvertToUnified();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(unified.Name, Is.Empty);
                Assert.That(unified.SurnamePrefix, Is.Empty);
                Assert.That(unified.Surname, Is.EqualTo("Bakkerij Rogers B.V."));
            });
        }

        [Test]
        public void ConvertToUnified_Person_KeepsItsSurname_AndIgnoresAnyOrganizationName()
        {
            // Arrange
            var party = new PartyResult
            {
                Identification = new PartyIdentification
                {
                    Details = new PartyDetails { Name = "Samantha", Surname = "Rogers" },
                    OrganizationName = "Should not win"
                }
            };

            // Act
            CommonPartyData unified = (party, DistributionChannels.Email, "sam@example.com", string.Empty, string.Empty)
                .ConvertToUnified();

            // Assert
            Assert.That(unified.Surname, Is.EqualTo("Rogers"));
        }
    }
}
