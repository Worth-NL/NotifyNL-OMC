// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using System.Text.Json;

namespace EventsHandler.UnitTests.Services.Serialization
{
    [TestFixture]
    public sealed class SpecificSerializerTests
    {
        private ISerializationService? _serializer;

        #region Test data
        private const string Identification = "ZAAKTYPE-2023-0000000010";
        private const string Name = "begin";
        private const string Description = "begin";
        private const string IsFinalStatus = "false";
        private const string IsNotificationExpected = "true";

        private const string InputJson1 =
            $"{{" +
              $"\"url\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/statustypen/e22c1e78-1893-4fd7-a674-3900672859c7\"," +
              $"\"identificatie\":\"{Identification}\"," +
              $"\"omschrijving\":\"{Name}\"," +
              $"\"omschrijvingGeneriek\":\"{Description}\"," +  // "G" upper case
              $"\"statustekst\":\"begin status\"," +
              $"\"zaaktype\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/zaaktypen/54c6063d-d3ae-47dd-90df-9e00cfa122a2\"," +
              $"\"zaaktypeIdentificatie\":\"1\"," +
              $"\"volgnummer\":2," +
              $"\"isEindstatus\":{IsFinalStatus}," +  // "E" upper case
              $"\"informeren\":{IsNotificationExpected}," +
              $"\"doorlooptijd\":null," +
              $"\"toelichting\":\"begin status\"," +
              $"\"checklistitemStatustype\":[]," +
              $"\"catalogus\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/catalogussen/34061b3c-cc85-4572-ba27-e286c279fb40\"," +
              $"\"eigenschappen\":[]," +
              $"\"zaakobjecttypen\":[]," +
              $"\"beginGeldigheid\":null," +
              $"\"eindeGeldigheid\":null," +
              $"\"beginObject\":null," +
              $"\"eindeObject\":null" +
            $"}}";
        
        private const string InputJson2 =
            $"{{" +
              $"\"url\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/statustypen/e22c1e78-1893-4fd7-a674-3900672859c7\"," +
              $"\"identificatie\":\"{Identification}\"," +
              $"\"omschrijving\":\"{Name}\"," +
              $"\"omschrijvinggeneriek\":\"{Description}\"," +  // "g" lower case
              $"\"statustekst\":\"begin status\"," +
              $"\"zaaktype\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/zaaktypen/54c6063d-d3ae-47dd-90df-9e00cfa122a2\"," +
              $"\"zaaktypeIdentificatie\":\"1\"," +
              $"\"volgnummer\":2," +
              $"\"iseindstatus\":{IsFinalStatus}," +  // "e" lower case
              $"\"informeren\":{IsNotificationExpected}," +
              $"\"doorlooptijd\":null," +
              $"\"toelichting\":\"begin status\"," +
              $"\"checklistitemStatustype\":[]," +
              $"\"catalogus\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/catalogussen/34061b3c-cc85-4572-ba27-e286c279fb40\"," +
              $"\"eigenschappen\":[]," +
              $"\"zaakobjecttypen\":[]," +
              $"\"beginGeldigheid\":null," +
              $"\"eindeGeldigheid\":null," +
              $"\"beginObject\":null," +
              $"\"eindeObject\":null" +
            $"}}";

        private const string OutputJson =
            $"{{" +
              $"\"identificatie\":\"{Identification}\"," +
              $"\"omschrijving\":\"{Name}\"," +
              $"\"omschrijvingGeneriek\":\"{Description}\"," +
              $"\"isEindstatus\":{IsFinalStatus}," +
              $"\"informeren\":{IsNotificationExpected}" +
            $"}}";
        #endregion

        [SetUp]
        public void InitializeTests()
        {
            this._serializer = new SpecificSerializer();
        }

        #region Deserialize
        [TestCase(InputJson1)]
        [TestCase(InputJson2)]
        public void Deserialize_TakesValidJson_AndReturnsDeserializedModel(string inputJson)
        {
            // Act
            CaseType actualResult = this._serializer!.Deserialize<CaseType>(inputJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.Name, Is.EqualTo(Name));
                Assert.That(actualResult.Description, Is.EqualTo(Description));
                Assert.That(actualResult.IsFinalStatus, Is.EqualTo(Convert.ToBoolean(IsFinalStatus)));
                Assert.That(actualResult.IsNotificationExpected, Is.EqualTo(Convert.ToBoolean(IsNotificationExpected)));
            });
        }

        [Test]
        public void Deserialize_TakesEmptyJson_ThrowsJsonException()
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                JsonException? exception = Assert.Throws<JsonException>(() => this._serializer!.Deserialize<CaseType>(DefaultValues.Models.EmptyJson));

                const string expectedMessage =
                    "The given value cannot be deserialized into dedicated target object | " +
                    "Target: CaseType | " +
                    "Value: {} | " +
                    "Required properties: identificatie, omschrijving, omschrijvingGeneriek, isEindstatus, informeren";

                Assert.That(exception?.Message, Is.EqualTo(expectedMessage));
            });
        }
        #endregion

        #region Serialize
        [Test]
        public void Serialize_TakesValidModel_AndReturnsSerializedJson()
        {
            // Arrange
            var testModel = new CaseType
            {
                Identification = Identification,
                Name = Name,
                Description = Description,
                IsFinalStatus = Convert.ToBoolean(IsFinalStatus),
                IsNotificationExpected = Convert.ToBoolean(IsNotificationExpected)
            };

            // Act
            string actualResult = this._serializer!.Serialize(testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(OutputJson));
        }

        [Test]
        public void Serialize_TakesUnknownModel_DoesNotThrowException_AndReturnsDefaultJson()
        {
            // Act
            string actualResult = this._serializer!.Serialize(default(CaseType));

            // Assert
            Assert.That(actualResult, Is.EqualTo("{\"identificatie\":null,\"omschrijving\":null,\"omschrijvingGeneriek\":null,\"isEindstatus\":false,\"informeren\":false}"));
        }
        #endregion
    }
}