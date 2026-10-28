// © 2026, Worth Systems.

using NUnit.Framework;
using System.Text.Json;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

namespace ZgwModels.Tests.Unit.Mapping.Models.POCOs.OpenZaak
{
    [TestFixture]
    public sealed class CaseResultTypeTests
    {
        [Test]
        public void Deserialize_WithOmschrijvingGeneriek_MapsBothDescriptions()
        {
            const string json = "{\"omschrijving\":\"Gegrond\",\"omschrijvingGeneriek\":\"Toegekend\"}";

            CaseResultType resultType = JsonSerializer.Deserialize<CaseResultType>(json);

            Assert.Multiple(() =>
            {
                Assert.That(resultType.Name, Is.EqualTo("Gegrond"));
                Assert.That(resultType.GenericName, Is.EqualTo("Toegekend"));
            });
        }

        [Test]
        public void Deserialize_WithoutOmschrijvingGeneriek_StillDeserializes_WithAnEmptyGenericName()
        {
            // The Catalogi API derives omschrijvingGeneriek from the selectielijst, so it can be missing
            const string json = "{\"omschrijving\":\"Gegrond\"}";

            CaseResultType resultType = JsonSerializer.Deserialize<CaseResultType>(json);

            Assert.Multiple(() =>
            {
                Assert.That(resultType.Name, Is.EqualTo("Gegrond"));
                Assert.That(resultType.GenericName, Is.Empty);
            });
        }
    }
}
