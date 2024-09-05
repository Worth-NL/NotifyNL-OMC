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

        #region Test data (fields)
        private const string CaseTypeIdentification = "ZAAKTYPE-2023-0000000010";
        private const string Name = "begin";
        private const string Description = "begin";
        private const string IsFinalStatus = "false";
        private const string IsNotificationExpected = "true";
        #endregion

        #region Test data (JSON input)
        // ReSharper disable InconsistentNaming
        private const string Input_CaseType_Original =
            $"{{" +
              $"\"url\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/statustypen/e22c1e78-1893-4fd7-a674-3900672859c7\"," +
              $"\"omschrijving\":\"{Name}\"," +
              $"\"omschrijvingGeneriek\":\"{Description}\"," +  // "G" upper case
              $"\"statustekst\":\"begin status\"," +
              $"\"zaaktype\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/zaaktypen/54c6063d-d3ae-47dd-90df-9e00cfa122a2\"," +
              $"\"zaaktypeIdentificatie\":\"{CaseTypeIdentification}\"," +
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
        
        private const string Input_CaseType_LowerCase =
            $"{{" +
              $"\"url\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/statustypen/e22c1e78-1893-4fd7-a674-3900672859c7\"," +
              $"\"omschrijving\":\"{Name}\"," +
              $"\"omschrijvinggeneriek\":\"{Description}\"," +  // "g" lower case
              $"\"statustekst\":\"begin status\"," +
              $"\"zaaktype\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/zaaktypen/54c6063d-d3ae-47dd-90df-9e00cfa122a2\"," +
              $"\"zaaktypeIdentificatie\":\"{CaseTypeIdentification}\"," +
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

        private const string Input_TaskObject =
            $"{{" +
              $"\"url\":\"https://objecten.test.notifynl.nl/api/v1/objects/ced88e8f-83fb-4f9d-866e-33b4bd0e4e78\"," +
              $"\"uuid\":\"ced88e8f-83fb-4f9d-866e-33b4bd0e4e78\"," +
              $"\"type\":\"https://objecttypen.test.notifynl.nl/api/v1/objecttypes/3e852115-277a-4570-873a-9a64be3aeb34\"," +
              $"\"record\":{{" +
                $"\"index\":1," +
                $"\"typeVersion\":1," +
                $"\"data\":{{" +
                  $"\"data\":{{" +
                    $"\"informatieverzoek\":{{" +
                      $"\"deadline\":\"2024-05-03T21:59:59.999Z\"," +
                      $"\"toelichting\":\"\"," +
                      $"\"opleidingVolgendePersonen\":null," +
                      $"\"opTeVragenBenodigdeInformatie\":{{" +
                        $"\"partnerID\":false," +
                        $"\"jaarrekening\":true," +
                        $"\"balansrekening\":false," +
                        $"\"specificatiePensioen\":false," +
                        $"\"bewijsVanInschrijving\":false," +
                        $"\"laatsteInkomstenSpecificatie\":false," +
                        $"\"bankafschriftenAfgelopenMaand\":false," +
                        $"\"belastingaanslagAfgelopenJaar\":false," +
                        $"\"belastingaangifteAfgelopenJaar\":true," +
                        $"\"bankafschriftenZakelijkeRekeningen\":false," +
                        $"\"laatsteInkomstenSpecificatiePartner\":false," +
                        $"\"specifiekeBankafschriftenPerPeriode\":false," +
                        $"\"specifiekeBankafschriftenAfgelopenMaand\":false," +
                        $"\"bankafschriftenZakelijkeRekeningenPartner\":false" +
                      $"}}," +
                      $"\"specifiekeBankrekeningNummers\":null," +
                      $"\"bankrekeningNummersSpecifiekePeriode\":null" +
                    $"}}" +
                  $"}}," +
                  $"\"zaak\":\"http://localhost:8001/zaken/api/v1/zaken/f621749d-d222-49b8-9392-eff8723e0922\"," +
                  $"\"title\":\"Aanleveren informatie\"," +
                  $"\"status\":\"open\"," +
                  $"\"formulier\":{{" +
                    $"\"type\":\"url\"," +
                    $"\"value\":\"http://localhost:8010/api/v2/objects/0db2a8a0-1ca8-4395-8a7a-c6293e33b4cd\"" +
                  $"}}," +
                  $"\"verloopdatum\":null," +
                  $"\"identificatie\":{{" +
                    $"\"type\":\"bsn\"," +
                    $"\"value\":\"569312863\"" +
                  $"}}," +
                  $"\"verzonden_data\":{{" +
                  $"}}," +
                  $"\"verwerker_taak_id\":\"1809f547-d0be-48a9-bba4-a6d7d5f36ba5\"" +
                $"}}," +
                $"\"geometry\":null," +
                $"\"startAt\":\"2053-01-01\"," +
                $"\"endAt\":null," +
                $"\"registrationAt\":\"2024-09-04\"," +
                $"\"correctionFor\":null," +
                $"\"correctedBy\":null" +
              $"}}" +
            $"}}";
        #endregion

        #region Test data (JSON output)
        private const string Output_CaseType =
            $"{{" +
              $"\"omschrijving\":\"{Name}\"," +
              $"\"omschrijvingGeneriek\":\"{Description}\"," +
              $"\"zaaktypeIdentificatie\":\"{CaseTypeIdentification}\"," +
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
        [TestCase(Input_CaseType_Original)]
        [TestCase(Input_CaseType_LowerCase)]
        public void Deserialize_CaseType_ValidJson_ReturnsExpectedModel(string inputJson)
        {
            // Act
            CaseType actualResult = this._serializer!.Deserialize<CaseType>(inputJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.Name, Is.EqualTo(Name));
                Assert.That(actualResult.Description, Is.EqualTo(Description));
                Assert.That(actualResult.Identification, Is.EqualTo(CaseTypeIdentification));
                Assert.That(actualResult.IsFinalStatus, Is.EqualTo(Convert.ToBoolean(IsFinalStatus)));
                Assert.That(actualResult.IsNotificationExpected, Is.EqualTo(Convert.ToBoolean(IsNotificationExpected)));
            });
        }

        [Test]
        public void Deserialize_CaseType_EmptyJson_ThrowsJsonException()
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                JsonException? exception = Assert.Throws<JsonException>(() => this._serializer!.Deserialize<CaseType>(DefaultValues.Models.EmptyJson));

                const string expectedMessage =
                    "The given value cannot be deserialized into dedicated target object | " +
                    "Target: CaseType | " +
                    "Value: {} | " +
                    "Required properties: omschrijving, omschrijvingGeneriek, zaaktypeIdentificatie, isEindstatus, informeren";

                Assert.That(exception?.Message, Is.EqualTo(expectedMessage));
            });
        }
        #endregion

        #region Serialize
        [Test]
        public void Serialize_CaseType_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new CaseType
            {
                Identification = CaseTypeIdentification,
                Name = Name,
                Description = Description,
                IsFinalStatus = Convert.ToBoolean(IsFinalStatus),
                IsNotificationExpected = Convert.ToBoolean(IsNotificationExpected)
            };

            // Act
            string actualResult = this._serializer!.Serialize(testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(Output_CaseType));
        }

        [Test]
        public void Serialize_CaseType_Default_ReturnsDefaultJson()
        {
            // Act
            string actualResult = this._serializer!.Serialize(default(CaseType));

            // Assert
            Assert.That(actualResult, Is.EqualTo("{\"omschrijving\":\"\",\"omschrijvingGeneriek\":\"\",\"zaaktypeIdentificatie\":\"\",\"isEindstatus\":false,\"informeren\":false}"));
        }
        #endregion
    }
}