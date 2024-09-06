// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.UnitTests.Services.Serialization
{
    [TestFixture]
    public sealed class SpecificSerializerTests
    {
        private ISerializationService _serializer = null!;

        #region Test data (fields)
        // Case
        private const string CaseIdentification = "ZAAK-2023-0000000010";
        private const string CaseName = "Case type";
        private const string TestUrl = "https://www.domain.test/00000000-0000-0000-0000-000000000000";
        
        // Case Type
        private const string CaseTypeIdentification = "ZAAKTYPE-2024-0000000001";
        private const string CaseTypeName = "Case type";
        private const string Description = "The description of the case type";
        private const string IsFinalStatus = "false";
        private const string IsNotificationExpected = "true";

        // Task Object
        private const string Title = "Test title";
        private const string BsnNumber = "123456789";
        #endregion

        #region Test data (JSON input)
        // ReSharper disable InconsistentNaming
        private const string Input_Case =
            $"{{" +
              $"\"identificatie\":\"{CaseIdentification}\"," +
              $"\"omschrijving\":null," +              // Should be deserialized as default not null
              $"\"omschrijvingGeneriek\":\"Test\"," +  // Should be ignored
              $"\"zaaktype\":null," +                  // Should be deserialized as default not null
              $"\"registratiedatum\":null" +           // Should be deserialized as default not null
            $"}}";

        private const string Input_CaseType_OriginalSpelling =
            $"{{" +
              $"\"url\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/statustypen/e22c1e78-1893-4fd7-a674-3900672859c7\"," +
              $"\"omschrijving\":\"{CaseTypeName}\"," +
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
        
        private const string Input_CaseType_LowerAndCapitalCase =
            $"{{" +
              $"\"url\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/statustypen/e22c1e78-1893-4fd7-a674-3900672859c7\"," +
              $"\"omschrijving\":\"{CaseTypeName}\"," +
              $"\"omschrijvinggeneriek\":\"{Description}\"," +  // "g" lower case => case-insensitive option should deserialize this property anyway
              $"\"statustekst\":\"begin status\"," +
              $"\"zaaktype\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/zaaktypen/54c6063d-d3ae-47dd-90df-9e00cfa122a2\"," +
              $"\"zaaktypeIdentificatie\":\"{CaseTypeIdentification}\"," +
              $"\"volgnummer\":2," +
              $"\"ISEINDSTATUS\":{IsFinalStatus}," +  // Everything is capital => case-insensitive option should deserialize this property anyway
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
              $"\"record\":{{" +   // Multiple nested models should be deserialized properly
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
                  $"\"status\":\"open\"," +  // Enum should be deserialized properly
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
        
        private const string Input_ContactMoment =
            $"{{" +
              $"\"uuid\":null," +  // Should be deserialized as default not null
              $"\"url\":null" +    // Should be deserialized as default not null
            $"}}";
        #endregion

        #region Test data (JSON output)
        private const string Output_Case =
            $"{{" +
              $"\"identificatie\":\"{CaseIdentification}\"," +
              $"\"omschrijving\":\"{CaseTypeName}\"," +
              $"\"zaaktype\":\"{TestUrl}\"," +
              $"\"registratiedatum\":\"2024-09-05\"" +
            $"}}";

        private const string Output_CaseType =
            $"{{" +
              $"\"omschrijving\":\"{CaseName}\"," +
              $"\"omschrijvingGeneriek\":\"{Description}\"," +
              $"\"zaaktypeIdentificatie\":\"{CaseTypeIdentification}\"," +
              $"\"isEindstatus\":{IsFinalStatus}," +
              $"\"informeren\":{IsNotificationExpected}" +
            $"}}";

        private const string Output_TaskObject =
            $"{{" +
              $"\"record\":{{" +
                $"\"data\":{{" +
                  $"\"zaak\":\"{TestUrl}\"," +
                  $"\"title\":\"{Title}\"," +
                  $"\"status\":\"open\"," +
                  $"\"verloopdatum\":\"2024-09-05T15:45:30.0000000Z\"," +
                  $"\"identificatie\":{{" +
                    $"\"type\":\"bsn\"," +
                    $"\"value\":\"{BsnNumber}\"" +
                  $"}}" +
                $"}}" +
              $"}}" +
            $"}}";
        #endregion

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._serializer = new SpecificSerializer();
        }

        #region Deserialize
        [Test]
        public void Deserialize_Case_PartiallyValidJson_Nulls_ReturnsExpectedModel()  // Strings, Uri, and DateOnly should be properly deserialized from nulls
        {
            // Act
            Case actualResult = this._serializer.Deserialize<Case>(Input_Case);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.Identification, Is.EqualTo(CaseIdentification));
                Assert.That(actualResult.Name, Is.Empty);
                Assert.That(actualResult.CaseTypeUri, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualResult.RegistrationDate, Is.EqualTo(DateOnly.MinValue));
            });
        }

        [TestCase(Input_CaseType_OriginalSpelling)]
        [TestCase(Input_CaseType_LowerAndCapitalCase)]
        public void Deserialize_CaseType_ValidJson_ReturnsExpectedModel(string inputJson)  // Case-insensitive option should deserialize these properties as well
        {
            // Act
            CaseType actualResult = this._serializer.Deserialize<CaseType>(inputJson);

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [Test]
        public void Deserialize_TaskObject_ValidJson_ReturnsExpectedModel()  // Nested objects and enums should be deserialized properly
        {
            // Act
            TaskObject actualResult = this._serializer.Deserialize<TaskObject>(Input_TaskObject);

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [Test]
        public void Deserialize_ContactMoment_PartiallyValidJson_ReturnsExpectedModel()  // GUID should be deserialized properly
        {
            // Act
            ContactMoment actualResult = this._serializer.Deserialize<ContactMoment>(Input_ContactMoment);

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [Test]
        public void Deserialize_CaseType_EmptyJson_ThrowsJsonException_ListsRequiredProperties()
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                JsonException? exception = Assert.Throws<JsonException>(() => this._serializer.Deserialize<CaseType>(DefaultValues.Models.EmptyJson));

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
        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_Case_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: A bit more complex model: empty DateTime should be DateTime.Min, and Uri the default one
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new Case());

            // Assert
            Assert.That(actualResult, Is.EqualTo("{\"identificatie\":\"\",\"omschrijving\":\"\",\"zaaktype\":\"http://0.0.0.0:0/\",\"registratiedatum\":\"0001-01-01\"}"));
        }

        [Test]
        public void Serialize_Case_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new Case
            {
                Identification = CaseIdentification,
                Name = CaseTypeName,
                CaseTypeUri = new Uri(TestUrl),
                RegistrationDate = new DateOnly(2024, 09, 05)
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(Output_Case));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_CaseType_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: Relatively simple model: empty strings should be "" not null
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new CaseType());

            // Assert
            Assert.That(actualResult, Is.EqualTo("{\"omschrijving\":\"\",\"omschrijvingGeneriek\":\"\",\"zaaktypeIdentificatie\":\"\",\"isEindstatus\":false,\"informeren\":false}"));
        }

        [Test]
        public void Serialize_CaseType_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new CaseType
            {
                Identification = CaseTypeIdentification,
                Name = CaseTypeName,
                Description = Description,
                IsFinalStatus = Convert.ToBoolean(IsFinalStatus),
                IsNotificationExpected = Convert.ToBoolean(IsNotificationExpected)
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(Output_CaseType));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_TaskObject_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: Very complex model: nested objects (with objects and enums) should be initialized as well
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new TaskObject());

            // Assert
            Assert.That(actualResult, Is.EqualTo("{\"record\":{\"data\":{\"zaak\":\"http://0.0.0.0:0/\",\"title\":\"\",\"status\":\"-\",\"verloopdatum\":\"0001-01-01T00:00:00.0000000\",\"identificatie\":{\"type\":\"-\",\"value\":\"\"}}}}"));
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_ContactMoment_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: Simple model: GUID should be handled properly
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new ContactMoment());

            // Assert
            Assert.That(actualResult, Is.EqualTo("{\"uuid\":\"00000000-0000-0000-0000-000000000000\",\"url\":\"http://0.0.0.0:0/\"}"));
        }

        [Test]
        public void Serialize_TaskObject_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new TaskObject
            {
                Record = new Record
                {
                    Data = new Data
                    {
                        CaseUri = new Uri($"https://www.domain.test/{Guid.Empty}"),
                        Title = Title,
                        Status = TaskStatuses.Open,
                        ExpirationDate = new DateTime(2024, 09, 05, 15, 45, 30, DateTimeKind.Utc),
                        Identification = new Identification
                        {
                            Type = IdTypes.Bsn,
                            Value = BsnNumber
                        }
                    }
                }
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(Output_TaskObject));
        }
        #endregion

        #region Helper methods
        private static void AssertRequiredProperties<TModel>(TModel instance)
            where TModel : struct, IJsonSerializable
        {
            Assert.Multiple(() =>
            {
                PropertyInfo[] requiredProperties = instance.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property => property.GetCustomAttribute<JsonRequiredAttribute>() != null)
                    .ToArray();

                foreach (PropertyInfo property in requiredProperties)
                {
                    Assert.That(property.GetValue(instance), Is.Not.Default);
                }
            });
        }
        #endregion
    }
}