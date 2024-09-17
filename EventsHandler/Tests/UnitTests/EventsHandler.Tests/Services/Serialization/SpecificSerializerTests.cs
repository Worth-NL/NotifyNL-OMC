// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Utilities._TestHelpers;
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
        private const string TestString = "text";
        private const string TestBoolean = "false";
        private const string TestGuid = "00000000-0000-0000-0000-000000000000";
        private const string TestUrl = $"https://www.domain.test/{TestGuid}";
        #endregion

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._serializer = new SpecificSerializer();
        }

        #region Deserialize
        [TestCaseSource(nameof(GetTestNotifications))]
        public void Deserialize_NotificationEvent_EverythingIsMapped(string testNotification)
        {
            // Act
            NotificationEvent notification = this._serializer.Deserialize<NotificationEvent>(testNotification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(notification.Action, Is.Not.EqualTo(Actions.Unknown));
                Assert.That(notification.Channel, Is.Not.EqualTo(Channels.Unknown));
                Assert.That(notification.Resource, Is.Not.EqualTo(Resources.Unknown));
                Assert.That(notification.Attributes, Is.Not.Default);
                Assert.That(notification.MainObjectUri, Is.Not.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(notification.ResourceUri, Is.Not.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(notification.CreateDate, Is.Not.Default);
            });
        }

        private static IEnumerable<string> GetTestNotifications()
        {
            yield return NotificationEventHandler.GetNotification_Real_CaseCreateScenario_TheHague();
            yield return NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague();
            yield return NotificationEventHandler.GetNotification_Real_TaskAssignedScenario_TheHague();
            yield return NotificationEventHandler.GetNotification_Real_DecisionMadeScenario_TheHague();
            yield return NotificationEventHandler.GetNotification_Real_MessageReceivedScenario_TheHague();
        }

        [Test]
        public void Deserialize_Case_PartiallyValidJson_Nulls_ReturnsExpectedModel()  // Strings, Uri, and DateOnly should be properly deserialized from nulls
        {
            // Arrange
            const string testJson =
                $"{{" +
                  $"\"identificatie\":\"{TestString}\"," +
                  $"\"omschrijving\":null," +              // Should be deserialized as default not null
                  $"\"omschrijvingGeneriek\":\"Test\"," +  // Should be ignored
                  $"\"zaaktype\":null," +                  // Should be deserialized as default not null
                  $"\"registratiedatum\":null" +           // Should be deserialized as default not null
                $"}}";

            // Act
            Case actualResult = this._serializer.Deserialize<Case>(testJson);

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [TestCase("omschrijvingGeneriek")]  // Original spelling
        [TestCase("omschrijvinggeneriek")]
        [TestCase("OMSCHRIJVINGGENERIEK")]
        public void Deserialize_CaseType_ValidJson_ReturnsExpectedModel(string jsonAttributeName)  // Case-insensitive option should deserialize these properties as well
        {
            // Arrange
            const string testJson =
                $"{{" +
                  $"\"url\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/statustypen/e22c1e78-1893-4fd7-a674-3900672859c7\"," +
                  $"\"omschrijving\":\"{TestString}\"," +
                  $"\"{{0}}\":\"{TestString}\"," +  // Different spelling
                  $"\"statustekst\":\"begin status\"," +
                  $"\"zaaktype\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/zaaktypen/54c6063d-d3ae-47dd-90df-9e00cfa122a2\"," +
                  $"\"zaaktypeIdentificatie\":\"{TestString}\"," +
                  $"\"volgnummer\":2," +
                  $"\"isEindstatus\":{TestBoolean}," +
                  $"\"informeren\":{TestBoolean}," +
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

            // Act
            CaseType actualResult = this._serializer.Deserialize<CaseType>(testJson.Replace("{0}", jsonAttributeName));

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

        private const string TaskDataJsonTheHague =
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
                  $"\"verloopdatum\":\"2024-05-03T21:59:59.999Z\"," +
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

        private const string TaskDataJsonNijmegen =
            $"{{" +
              $"\"record\":{{" +
                $"\"data\":{{" +
                  $"\"titel\":\"Check loan\"," +
                  $"\"status\":\"open\"," +
                  $"\"soort\":\"formtaak\"," +
                  $"\"verloopdatum\":\"2023-09-20T18:25:43.524Z\"," +
                  $"\"identificatie\":{{" +
                    $"\"type\":\"bsn\"," +
                    $"\"value\":\"82395551\"" +
                  $"}}," +
                  $"\"koppeling\":{{" +
                    $"\"registratie\":\"zaak\"," +
                    $"\"uuid\":\"5551a7c5-4e92-43e6-8d23-80359b7e22b7\"" +
                  $"}}," +
                  $"\"url\":{{" +
                    $"\"uri\":\"https://google.com\"" +
                  $"}}," +
                  $"\"portaalformulier\":{{" +
                    $"\"formulier\":{{" +
                      $"\"soort\":\"url\"," +
                      $"\"value\":\"http://localhost:8010/api/v2/objects/4e40fb4c-a29a-4e48-944b-c34a1ff6c8f4\"" +
                    $"}}," +
                    $"\"data\":{{" +
                      $"\"voornaam\":\"Jan\"," +
                      $"\"achternaam\":\"Smit\"," +
                      $"\"toestemming\":true," +
                      $"\"geboortedatum\":\"01-01-1970\"" +
                    $"}}," +
                    $"\"verzonden_data\":{{" +
                      $"\"voornaam\":\"Jan\"," +
                      $"\"achternaam\":\"Smit\"," +
                      $"\"toestemming\":false," +
                      $"\"geboortedatum\":\"01-01-1971\"" +
                    $"}}" +
                  $"}}," +
                  $"\"ogonebetaling\":{{" +
                    $"\"bedrag\":147.43," +
                    $"\"betaalkenmerk\":\"abcdef1234\"," +
                    $"\"pspid\":\"MyID\"" +
                  $"}}," +
                  $"\"verwerker_taak_id\":\"18af0b6a-967b-4f81-bb8e-a44988e0c2f0\"," +
                  $"\"eigenaar\":\"gzac-sd\"" +
                $"}}" +
              $"}}" +
            $"}}";

        [TestCase(TaskDataJsonTheHague)]
        [TestCase(TaskDataJsonNijmegen)]
        public void Deserialize_CommonTaskData_ValidJson_ReturnsExpectedModel(string testJson)  // Nested objects and enums should be deserialized properly
        {
            // Arrange

            // Act
            CommonTaskData actualResult = this._serializer.Deserialize<CommonTaskData>(testJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.CaseUri, Is.Not.Default);
                Assert.That(actualResult.CaseId, Is.Not.Default);
                Assert.That(actualResult.Title, Is.Not.Default);
                Assert.That(actualResult.Status, Is.Not.Default);
                Assert.That(actualResult.ExpirationDate, Is.Not.Default);
                Assert.That(actualResult.Identification, Is.Not.Default);
            });
        }
        
        [TestCase("null")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("\"null\"")]
        [TestCase("\"true\"")]
        [TestCase("\"false\"")]
        public void Deserialize_DecisionType_ValidJson_Booleans_ReturnsExpectedModel(string publicationIndicationValue)  // Different boolean values
        {
            // Arrange
            const string testJson =
                $"{{" +
                  $"\"catalogus\":\"https://openzaak.test.notifynl.nl/catalogi/api/v1/catalogussen/8399feb6-1349-401c-8c07-f6a49209089a\", " +
                  $"\"publicatieIndicatie\": {{0}}, " +
                  $"\"beginGeldigheid\":\"2001-01-04\", " +
                  $"\"omschrijving\":\"Omschrijving besluit a\", " +
                  $"\"omschrijvingGeneriek\":\"Omschrijving besluit generiek\", " +
                  $"\"besluitcategorie\":\"Categorie1\", " +
                  $"\"reactietermijn\":\"P1Y1M1D\", " +
                  $"\"publicatietekst\":\"Publicatie tekst\", " +
                  $"\"publicatietermijn\":\"P1Y1M1D\", " +
                  $"\"toelichting\":\"Toelichting besluit\", " +
                  $"\"informatieobjecttypen\": [\"https://openzaak.test.notifynl.nl/catalogi/api/v1/informatieobjecttypen/5e11506b-d685-4db1-908a-5c2e472c81e6\"]" +
                $"}}";

            // Act
            DecisionType actualResult = this._serializer.Deserialize<DecisionType>(testJson.Replace("{0}", publicationIndicationValue));

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [Test]
        public void Deserialize_Document_ValidJson_ReturnsExpectedModel()
        {
            // Arrange
            const string testJson =
                "{" +
                  "\"url\":\"https://openzaak.test.notifynl.nl/besluiten/api/v1/besluitinformatieobjecten/ced8cd8a-d096-47cf-8352-8381274d852c\"," +
                  "\"informatieobject\":\"https://openzaak.test.notifynl.nl/documenten/api/v1/enkelvoudiginformatieobjecten/8e5cd111-db45-4313-baf2-ea2ad1bcac01\"," +
                  "\"besluit\":\"https://openzaak.test.notifynl.nl/besluiten/api/v1/besluiten/61dcb374-6d41-48dd-b9a4-7fe6ab883ba2\"" +
                "}";

            // Act
            Document actualResult = this._serializer.Deserialize<Document>(testJson);

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [Test]
        public void Deserialize_Documents_ValidJson_ReturnsExpectedModel()
        {
            // Arrange
            const string testJson =
                "[" +
                  "{" +
                    "\"url\":\"https://openzaak.test.notifynl.nl/besluiten/api/v1/besluitinformatieobjecten/ced8cd8a-d096-47cf-8352-8381274d852c\"," +
                    "\"informatieobject\":\"https://openzaak.test.notifynl.nl/documenten/api/v1/enkelvoudiginformatieobjecten/8e5cd111-db45-4313-baf2-ea2ad1bcac01\"," +
                    "\"besluit\":\"https://openzaak.test.notifynl.nl/besluiten/api/v1/besluiten/61dcb374-6d41-48dd-b9a4-7fe6ab883ba2\"" +
                  "}," +
                  "{" +
                    "\"url\":\"https://openzaak.test.notifynl.nl/besluiten/api/v1/besluitinformatieobjecten/ced8cd8a-d096-47cf-8352-8381274d852c\"," +
                    "\"informatieobject\":\"https://openzaak.test.notifynl.nl/documenten/api/v1/enkelvoudiginformatieobjecten/8e5cd111-db45-4313-baf2-ea2ad1bcac01\"," +
                    "\"besluit\":\"https://openzaak.test.notifynl.nl/besluiten/api/v1/besluiten/61dcb374-6d41-48dd-b9a4-7fe6ab883ba2\"" +
                  "}" +
                "]";
            
            // Act
            Documents actualResult = this._serializer.Deserialize<Documents>(testJson);

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [TestCase("[]")]
        [TestCase("[ ]")]
        public void Deserialize_Documents_EmptyArrayJson_ReturnsExpectedModel(string invalidEmptyJson)
        {
            // Act
            Documents actualResult = this._serializer.Deserialize<Documents>(invalidEmptyJson);

            // Assert
            AssertRequiredProperties(actualResult);
        }

        [Test]
        public void Deserialize_ContactMoment_PartiallyValidJson_ReturnsExpectedModel()  // GUID should be deserialized properly
        {
            // Arrange
            const string testJson =
                $"{{" +
                  $"\"uuid\":null," +  // Should be deserialized as default not null
                  $"\"url\":null" +    // Should be deserialized as default not null
                $"}}";
        
            // Act
            ContactMoment actualResult = this._serializer.Deserialize<ContactMoment>(testJson);

            // Assert
            AssertRequiredProperties(actualResult);
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
            string expectedResult =
                $"{{" +
                  $"\"identificatie\":\"\"," +
                  $"\"omschrijving\":\"\"," +
                  $"\"zaaktype\":\"{DefaultValues.Models.EmptyUri}\"," +
                  $"\"registratiedatum\":\"0001-01-01\"" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void Serialize_Case_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new Case
            {
                Identification = TestString,
                Name = TestString,
                CaseTypeUri = new Uri(TestUrl),
                RegistrationDate = new DateOnly(2024, 09, 05)
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            const string expectedResult =
                $"{{" +
                  $"\"identificatie\":\"{TestString}\"," +
                  $"\"omschrijving\":\"{TestString}\"," +
                  $"\"zaaktype\":\"{TestUrl}\"," +
                  $"\"registratiedatum\":\"2024-09-05\"" +
                $"}}";
            
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_CaseType_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: Relatively simple model: empty strings should be "" not null
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new CaseType());

            // Assert
            const string expectedResult =
                "{" +
                  "\"omschrijving\":\"\"," +
                  "\"omschrijvingGeneriek\":\"\"," +
                  "\"zaaktypeIdentificatie\":\"\"," +
                  "\"isEindstatus\":false," +
                  "\"informeren\":false" +
                "}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void Serialize_CaseType_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new CaseType
            {
                Identification = TestString,
                Name = TestString,
                Description = TestString,
                IsFinalStatus = Convert.ToBoolean(TestBoolean),
                IsNotificationExpected = Convert.ToBoolean(TestBoolean)
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            const string expectedResult =
                $"{{" +
                  $"\"omschrijving\":\"{TestString}\"," +
                  $"\"omschrijvingGeneriek\":\"{TestString}\"," +
                  $"\"zaaktypeIdentificatie\":\"{TestString}\"," +
                  $"\"isEindstatus\":{TestBoolean}," +
                  $"\"informeren\":{TestBoolean}" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_TaskObject_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: Very complex model: nested objects (with objects and enums) should be initialized as well
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new TaskObject());

            // Assert
            string expectedResult =
                $"{{" +
                  $"\"record\":{{" +
                    $"\"data\":{{" +
                      $"\"zaak\":\"{DefaultValues.Models.EmptyUri}\"," +
                      $"\"title\":\"\"," +
                      $"\"status\":\"-\"," +
                      $"\"verloopdatum\":\"0001-01-01T00:00:00.0000000\"," +
                      $"\"identificatie\":{{" +
                        $"\"type\":\"-\"," +
                        $"\"value\":\"\"" +
                      $"}}" +
                    $"}}" +
                  $"}}" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
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
                        Title = TestString,
                        Status = TaskStatuses.Open,
                        ExpirationDate = new DateTime(2024, 09, 05, 15, 45, 30, DateTimeKind.Utc),
                        Identification = new Identification
                        {
                            Type = IdTypes.Bsn,
                            Value = TestString
                        }
                    }
                }
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            const string expectedResult =
                $"{{" +
                  $"\"record\":{{" +
                    $"\"data\":{{" +
                      $"\"zaak\":\"{TestUrl}\"," +
                      $"\"title\":\"{TestString}\"," +
                      $"\"status\":\"open\"," +
                      $"\"verloopdatum\":\"2024-09-05T15:45:30.0000000Z\"," +
                      $"\"identificatie\":{{" +
                        $"\"type\":\"bsn\"," +
                        $"\"value\":\"{TestString}\"" +
                      $"}}" +
                    $"}}" +
                  $"}}" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void Serialize_DecisionType_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new DecisionType
            {
                Name = TestString,
                Description = TestString,
                Category = TestString,
                PublicationIndicator = Convert.ToBoolean(TestBoolean),
                PublicationText = TestString,
                Explanation = TestString
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            const string expectedResult =
                $"{{" +
                  $"\"omschrijving\":\"{TestString}\"," +
                  $"\"omschrijvingGeneriek\":\"{TestString}\"," +
                  $"\"besluitcategorie\":\"{TestString}\"," +
                  $"\"publicatieIndicatie\":{TestBoolean}," +
                  $"\"publicatietekst\":\"{TestString}\"," +
                  $"\"toelichting\":\"{TestString}\"" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public void Serialize_Documents_ReturnsExpectedJson()
        {
            // Arrange
            var documents = new Documents
            {
                Results = new List<Document>
                {
                    new() { InfoObjectUri = DefaultValues.Models.EmptyUri },
                    new() { InfoObjectUri = DefaultValues.Models.EmptyUri }
                }
            };

            // Act
            string actualResult = this._serializer.Serialize(documents);

            // Assert
            string expectedResult =
                $"{{" +
                  $"\"results\":[" +
                    $"{{\"informatieobject\":\"{DefaultValues.Models.EmptyUri}\"}}," +
                    $"{{\"informatieobject\":\"{DefaultValues.Models.EmptyUri}\"}}" +
                  $"]" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public void Serialize_CommonTaskData_ReturnsExpectedJson()
        {
            // Arrange
            const string testGuid = "12345678-1234-0000-4321-123456789012";
            const string testUrl = $"https://www.domain.test/{testGuid}";

            var documents = new CommonTaskData
            {
                CaseUri = new Uri(testUrl),
                CaseId = new Guid(testGuid),
                Title = TestString,
                Status = TaskStatuses.Open,
                ExpirationDate = DateTime.MaxValue,
                Identification = new Identification
                {
                    Type = IdTypes.Bsn,
                    Value = "123456789"
                }
            };

            // Act
            string actualResult = this._serializer.Serialize(documents);

            // Assert
            const string expectedResult =
                $"{{" +
                  $"\"CaseUri\":\"{testUrl}\"," +
                  $"\"CaseId\":\"{testGuid}\"," +
                  $"\"Title\":\"text\"," +
                  $"\"Status\":\"open\"," +
                  $"\"ExpirationDate\":\"9999-12-31T23:59:59.9999999\"," +
                  $"\"Identification\":{{" +
                    $"\"type\":\"bsn\"," +
                    $"\"value\":\"123456789\"" +
                  $"}}" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_ContactMoment_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: Simple model: GUID should be handled properly
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new ContactMoment());

            // Assert
            string expectedResult =
                $"{{" +
                  $"\"uuid\":\"{TestGuid}\"," +
                  $"\"url\":\"{DefaultValues.Models.EmptyUri}\"" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
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