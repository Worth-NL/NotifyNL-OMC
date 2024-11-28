// © 2024, Worth Systems.

using Common.Constants;
using Common.Extensions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Enums.Objecten.vNijmegen;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.v2;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using EventsHandler.Properties;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Tests.Utilities._TestHelpers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.Tests.Unit.Services.Serialization
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
                  $"\"url\":\"{TestUrl}\"," +
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
        public void Deserialize_CaseRole_ValidJson_Null_ReturnsExpectedModel()  // NOTE: Handles nullable property
        {
            // Arrange
            const string testJson =
                $"{{" +
                  $"\"betrokkene\":null," +  // Should be deserialized as default not null
                  $"\"omschrijvingGeneriek\":\"{TestString}\"," +
                  $"\"betrokkeneIdentificatie\":null" +  // Can be null
                $"}}";

            // Act
            CaseRole actualResult = this._serializer.Deserialize<CaseRole>(testJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.InvolvedPartyUri, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualResult.InitiatorRole, Is.EqualTo(TestString));
                Assert.That(actualResult.Party, Is.Null);
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
              $"\"url\":\"https://objects-api.woweb.app/api/v2/objects/63627e94-4652-403f-8ed5-0cb9addfe9dd\"," +
              $"\"uuid\":\"63627e94-4652-403f-8ed5-0cb9addfe9dd\"," +
              $"\"type\":\"https://objecttypes-api.woweb.app/api/v2/objecttypes/d5c77844-7e00-4908-9839-f18a8ac6a045\"," +
              $"\"record\":{{" +
                $"\"index\":1," +
                $"\"typeVersion\":1," +
                $"\"data\":{{" +
                  $"\"soort\":\"formtaak\"," +
                  $"\"titel\":\"test taak 18-9\"," +
                  $"\"status\":\"open\"," +
                  $"\"eigenaar\":\"vip\"," +
                  $"\"formtaak\":{{" +
                    $"\"formulier\":{{" +
                      $"\"soort\":\"url\"," +
                      $"\"value\":\"https://app6-accp.nijmegen.nl/#/form/ontwikkel/uploadBijlage\"" +
                    $"}}" +
                  $"}}," +
                  $"\"koppeling\":{{" +
                    $"\"uuid\":\"4f30cc08-48b5-490d-9742-fe3e94e17334\"," +
                    $"\"registratie\":\"zaak\"" +
                  $"}}," +
                  $"\"verloopdatum\":\"2024-09-25 00:00:00\"," +
                  $"\"identificatie\":{{" +
                    $"\"type\":\"bsn\"," +
                    $"\"value\":\"232426727\"" +
                  $"}}," +
                  $"\"verwerker_taak_id\":\"bestand\"," +
                  $"\"geometry\":null," +
                  $"\"startAt\":\"2024-09-18\"," +
                  $"\"endAt\":null," +
                  $"\"registrationAt\":\"2024-09-18\"," +
                  $"\"correctionFor\":null," +
                  $"\"correctedBy\":null" +
                $"}}" +
              $"}}" +
            $"}}";

        [TestCase(TaskDataJsonTheHague)]
        [TestCase(TaskDataJsonNijmegen)]
        public void Deserialize_CommonTaskData_ValidJson_ReturnsExpectedModel(string testJson)  // Nested objects and enums should be deserialized properly
        {
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
                  $"\"url\":null" +    // Should be deserialized as default not null
                $"}}";
        
            // Act
            ContactMoment actualResult = this._serializer.Deserialize<ContactMoment>(testJson);

            // Assert
            AssertRequiredProperties(actualResult);
        }
        #endregion

        #region Errors
        internal struct Example : IJsonSerializable
        {
            [JsonRequired]
            [JsonInclude]
            public string Name { get; internal set; }

            [JsonRequired]
            [JsonInclude]
            public int Age { get; internal set; }

            [JsonRequired]
            [JsonInclude]
            public bool IsAdmin { get; internal set; }

            [JsonRequired]
            [JsonInclude]
            public NestedExample Nested { get; private set; }

            [JsonRequired]
            [JsonInclude]
            public NestedExample[] Array { get; private set; }

            [JsonRequired]
            [JsonInclude]
            public List<NestedExample> List { get; private set; }
        }

        internal struct NestedExample : IJsonSerializable
        {
            [JsonRequired]
            [JsonInclude]
            public string Name2 { get; internal set; }

            [JsonRequired]
            [JsonInclude]
            public int Age2 { get; internal set; }

            [JsonRequired]
            [JsonInclude]
            public int IsAdmin2 { get; internal set; }
        }

        [Test]
        public void Deserialize_IJsonSerializable_From_IncompleteJson_ThrowsJsonException_ListsRequiredProperties()
        {
            // Arrange
            const string serialized = "{\"Name\":\"Aliana\",\"Age\":null,\"IsAdmin\":null}";

            // Act & Assert
            Assert.Multiple(() =>
            {
                JsonException? exception = Assert.Throws<JsonException>(() => this._serializer.Deserialize<Example>(serialized));

                const string expectedMessage = "The given JSON cannot be deserialized | " +
                                               "Target model: 'Example.cs' | " +
                                               "Failed: '$.Age' | " +
                                               "Reason: Cannot get the value of a token type 'Null' as a number. | " +
                                               "All required properties: " +
                                                   "'Name', 'Age', 'IsAdmin', " +
                                                   "'Nested.Name2', 'Nested.Age2', 'Nested.IsAdmin2', " +
                                                   "'Array.Name2', 'Array.Age2', 'Array.IsAdmin2', " +
                                                   "'List.Name2', 'List.Age2', 'List.IsAdmin2' | " +
                                               "Source JSON: {\"Name\":\"Aliana\",\"Age\":null,\"IsAdmin\":null}";

                Assert.That(exception?.Message, Is.EqualTo(expectedMessage));
            });
        }

        [Test]
        public void Deserialize_IJsonSerializable_From_EmptyJson_ThrowsJsonException_ListsRequiredProperties()  // NOTE: Simple model
        {
            // Act & Assert
            foreach ((int id, Action deserialization, string? targetName, string failed, string? expectedResult) in GetSerializationTests(DefaultValues.Models.EmptyJson))
            {
                Assert.Multiple(() =>
                {
                    JsonException? exception = Assert.Throws<JsonException>(() => deserialization.Invoke());

                    string expectedMessage =
                       $"The given JSON cannot be deserialized | " +
                       $"Target model: '{targetName}.cs' | " +
                       $"Failed: '{failed}' | " +
                       $"Reason: {ApiResources.Deserialization_ERROR_CannotDeserialize_RequiredProperties} | " +
                       $"All required properties: {expectedResult} | " +
                       $"Source JSON: {{}}";

                    Assert.That(exception?.Message, Is.EqualTo(expectedMessage), message: $"Test #{id}");
                });
            }

            return;

            IEnumerable<(int Id, Action Deserialization, string TargetName, string Failed, string ExpectedResult)> GetSerializationTests(string testJson)
            {
                yield return (1, () => this._serializer.Deserialize<CaseType>(testJson), nameof(CaseType), "omschrijving, omschrijvingGeneriek, zaaktypeIdentificatie", "'omschrijving', 'omschrijvingGeneriek', 'zaaktypeIdentificatie', 'isEindstatus', 'informeren'");
                yield return (2, () => this._serializer.Deserialize<PartyResult>(testJson), nameof(PartyResult), "url, voorkeursDigitaalAdres, partijIdentificatie, _expand", "'url', 'voorkeursDigitaalAdres.uuid', 'partijIdentificatie.contactnaam.voornaam', '_expand.digitaleAdressen.uuid', '_expand.digitaleAdressen.adres', '_expand.digitaleAdressen.soortDigitaalAdres'");
                yield return (3, () => this._serializer.Deserialize<CommonTaskData>(testJson), nameof(CommonTaskData), "record", "'CaseUri', 'CaseId', 'Title', 'Status', 'ExpirationDate', 'Identification.type', 'Identification.value'");
            }
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
                  $"\"url\":\"{DefaultValues.Models.EmptyUri}\"," +
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
                Uri = new Uri(TestUrl),
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
                  $"\"url\":\"{TestUrl}\"," +
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

        [Test]
        public void Serialize_CaseRole_ValidModel_ReturnsExpectedJson()  // Nullable property is accepted
        {
            // Arrange
            var testModel = new CaseRole
            {
                InvolvedPartyUri = new Uri(TestUrl),
                InitiatorRole = TestString,
                Party = null
            };

            // Act
            string actualResult = this._serializer.Serialize(testModel);

            // Assert
            const string expectedResult =
                $"{{" +
                  $"\"betrokkene\":\"{TestUrl}\"," +
                  $"\"omschrijvingGeneriek\":\"{TestString}\"," +
                  $"\"betrokkeneIdentificatie\":null" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_TaskObject_Hague_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: Very complex model: nested objects (with objects and enums) should be initialized as well
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague.TaskObject());

            // Assert
            string expectedResult =
                $"{{" +
                  $"\"record\":{{" +
                    $"\"data\":{{" +
                      $"\"zaak\":\"{DefaultValues.Models.EmptyUri}\"," +
                      $"\"title\":\"\"," +
                      $"\"status\":\"-\"," +
                      $"\"verloopdatum\":\"{DateTime.MinValue.ConvertToDutchDateString()}\"," +
                      $"\"identificatie\":{{" +
                        $"\"type\":\"-\"," +
                        $"\"value\":\"\"" +
                      $"}}" +
                    $"}}" +
                  $"}}" +
                $"}}";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Serialize_TaskObject_Nijmegen_Default_ReturnsExpectedJson(bool isDefault)  // NOTE: 2nd variant of TaskObject JSON schema
        {
            // Act
            string actualResult = this._serializer.Serialize(isDefault ? default : new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen.TaskObject());

            // Assert
            string expectedResult =
                $"{{" +
                  $"\"record\":{{" +
                    $"\"data\":{{" +
                      $"\"titel\":\"\"," +
                      $"\"status\":\"-\"," +
                      $"\"koppeling\":{{" +
                        $"\"uuid\":\"{Guid.Empty}\"," +
                        $"\"registratie\":\"-\"" +
                      $"}}," +
                      $"\"verloopdatum\":\"{DateTime.MinValue.ConvertToDutchDateString()}\"," +
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
        public void Serialize_TaskObject_Hague_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague.TaskObject
            {
                Record = new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague.Record
                {
                    Data = new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague.Data
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
                      $"\"verloopdatum\":\"05-09-2024\"," +
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
        public void Serialize_TaskObject_Nijmegen_ValidModel_ReturnsExpectedJson()
        {
            // Arrange
            var testModel = new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen.TaskObject
            {
                Record = new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen.Record
                {
                    Data = new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen.Data
                    {
                        Title = TestString,
                        Status = TaskStatuses.Open,
                        Coupling = new EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen.Coupling
                        {
                            Id = Guid.Empty,
                            Type = Registrations.Case
                        },
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
            string expectedResult =
                $"{{" +
                  $"\"record\":{{" +
                    $"\"data\":{{" +
                      $"\"titel\":\"{TestString}\"," +
                      $"\"status\":\"open\"," +
                      $"\"koppeling\":{{" +
                        $"\"uuid\":\"{Guid.Empty}\"," +
                        $"\"registratie\":\"zaak\"" +
                      $"}}," +
                      $"\"verloopdatum\":\"05-09-2024\"," +
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
                Results =
                [
                    new Document { InfoObjectUri = DefaultValues.Models.EmptyUri },
                    new Document { InfoObjectUri = DefaultValues.Models.EmptyUri }
                ]
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
                  $"\"ExpirationDate\":\"31-12-9999\"," +
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