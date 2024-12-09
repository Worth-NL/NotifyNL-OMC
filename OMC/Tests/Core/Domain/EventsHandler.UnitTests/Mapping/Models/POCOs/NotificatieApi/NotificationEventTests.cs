// © 2023, Worth Systems.

using System.Text.Json;
using Common.Constants;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Tests.Utilities._TestHelpers;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Tests.Unit.Mapping.Models.POCOs.NotificatieApi
{
    [TestFixture]
    public sealed class NotificationEventTests
    {
        #region Test data
        private const string ValidCaseJson =
            $"{{" +
              $"\"actie\":\"create\"," +
              $"\"kanaal\":\"zaken\"," +
              $"\"resource\":\"status\"," +
              $"\"kenmerken\":{{" +
                // Cases
                $"\"zaaktype\":\"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\"," +
                $"\"bronorganisatie\":\"{NotificationEventHandler.SourceOrganization_Real_TheHague}\"," +
                $"\"vertrouwelijkheidaanduiding\":\"openbaar\"," +
                // Objects
                $"\"objectType\":null," +
                // Decisions
                $"\"besluittype\":null," +
                $"\"verantwoordelijkeOrganisatie\":null" +
              $"}}," +
              $"\"hoofdObject\":\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\"," +
              $"\"resourceUrl\":\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\"," +
              $"\"aanmaakdatum\":\"2023-09-22T11:41:46.052Z\"" +
            "}";

        private const string ValidDecisionJson =
            $"{{" +
              $"\"actie\":\"create\"," +
              $"\"kanaal\":\"besluiten\"," +
              $"\"resource\":\"besluitinformatieobject\"," +
              $"\"kenmerken\":{{" +
                // Cases
                $"\"zaaktype\":null," +
                $"\"bronorganisatie\":null," +
                $"\"vertrouwelijkheidaanduiding\":null," +
                // Objects
                $"\"objectType\":null," +
                // Decisions
                $"\"besluittype\":\"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/besluittypen/7002077e-0358-4301-8aac-5b440093f214\"," +
                $"\"verantwoordelijkeOrganisatie\":\"{NotificationEventHandler.ResponsibleOrganization_Real_TheHague}\"" +
              $"}}," +
              $"\"hoofdObject\":\"https://openzaak.test.denhaag.opengem.nl/besluiten/api/v1/besluiten/a5300781-943f-49e4-a6c2-c0ca4516936c\"," +
              $"\"resourceUrl\":\"https://openzaak.test.denhaag.opengem.nl/besluiten/api/v1/besluiten/a5300781-943f-49e4-a6c2-c0ca4516936c\"," +
              $"\"aanmaakdatum\":\"2023-10-05T08:52:02.273Z\"" +
            "}";

        private static readonly string s_customJson =
            $"{{" +
              $"\"actie\": 0, " +  // Invalid value
              $"\"kanaal\": 1, " +
              $"\"resource\": 3, " +
              $"\"kenmerken\": {{" +
                // Cases
                $"\"zaaktype\": null, " +
                $"\"bronorganisatie\": null, " +
                $"\"vertrouwelijkheidaanduiding\": null, " +
                // Objects
                $"\"objectType\": null, " +
                // Decisions
                $"\"besluittype\": null, " +
                $"\"verantwoordelijkeOrganisatie\": null" +
              $"}}," +
              $"\"hoofdObject\": \"{CommonValues.Default.Models.EmptyUri}\", " +
              $"\"resourceUrl\": \"{CommonValues.Default.Models.EmptyUri}\", " +
              $"\"aanmaakdatum\": \"0001-01-01T00:00:00\"" +
            $"}}";

        private static readonly string s_unexpectedJson =
            $"{{" +
              $"\"actie\": 0, " +  // Invalid value
              $"\"kanaal\": 1, " +
              $"\"resource\": 3, " +
              $"\"kenmerken\": {{" +
                // Cases
                $"\"zaaktype\": null, " +
                $"\"bronorganisatie\": null, " +
                $"\"vertrouwelijkheidaanduiding\": null, " +
                // Objects
                $"\"objectType\": null, " +
                // Decisions
                $"\"besluittype\": null, " +
                $"\"verantwoordelijkeOrganisatie\": null" +
              $"}}," +
              $"\"hoofdObject\": \"{CommonValues.Default.Models.EmptyUri}\", " +
              $"\"resourceUrl\": \"{CommonValues.Default.Models.EmptyUri}\", " +
              $"\"aanmaakdatum\": \"0001-01-01T00:00:00\", " +
              // Orphans (event)
              $"\"{NotificationEventHandler.Orphan_Test_Property_3}\": \"{NotificationEventHandler.Orphan_Test_Value_3}\"" +
            $"}}";
        #endregion

        #region Serialization
        [Test]
        public void Serialization_Object_IntoCaseJson()
        {
            // Arrange
            NotificationEvent testObject = NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague()
                .Deserialized();

            // Act
            string actualJson = JsonSerializer.Serialize(testObject);

            // Assert
            Assert.That(actualJson, Is.EqualTo(ValidCaseJson));
        }

        [Test]
        public void Serialization_Object_IntoDecisionJson()
        {
            // Arrange
            NotificationEvent testObject = NotificationEventHandler.GetNotification_Real_DecisionMadeScenario_TheHague()
                .Deserialized();

            // Act
            string actualJson = JsonSerializer.Serialize(testObject);

            // Assert
            Assert.That(actualJson, Is.EqualTo(ValidDecisionJson));
        }
        #endregion

        #region Deserialization
        [Test]
        public void Deserialization_Valid_Json_IntoObject_WithoutOrphans()
        {
            // Act
            NotificationEvent actualObject = JsonSerializer.Deserialize<NotificationEvent>(s_customJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualObject.Action, Is.EqualTo(Actions.Unknown));
                Assert.That(actualObject.Channel, Is.EqualTo(Channels.Cases));
                Assert.That(actualObject.Resource, Is.EqualTo(Resources.Object));
                // NOTE: EventAttributes are covered by other unit tests
                Assert.That(actualObject.MainObjectUri, Is.EqualTo(CommonValues.Default.Models.EmptyUri));
                Assert.That(actualObject.ResourceUri, Is.EqualTo(CommonValues.Default.Models.EmptyUri));
                Assert.That(actualObject.CreateDate, Is.EqualTo(default(DateTime)));
                // Orphans
                Assert.That(actualObject.Orphans, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void Deserialization_Unexpected_Json_IntoObject_WithOrphans()
        {
            // Act
            NotificationEvent actualObject = JsonSerializer.Deserialize<NotificationEvent>(s_unexpectedJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualObject.Action, Is.EqualTo(Actions.Unknown));
                Assert.That(actualObject.Channel, Is.EqualTo(Channels.Cases));
                Assert.That(actualObject.Resource, Is.EqualTo(Resources.Object));
                // NOTE: EventAttributes are covered by other unit tests
                Assert.That(actualObject.MainObjectUri, Is.EqualTo(CommonValues.Default.Models.EmptyUri));
                Assert.That(actualObject.ResourceUri, Is.EqualTo(CommonValues.Default.Models.EmptyUri));
                Assert.That(actualObject.CreateDate, Is.EqualTo(default(DateTime)));
                // Orphans
                Assert.That(actualObject.Orphans, Has.Count.EqualTo(1));
                Assert.Multiple(() =>
                {
                    KeyValuePair<string, object> keyValuePair = actualObject.Orphans.First();

                    Assert.That(keyValuePair.Key, Is.EqualTo(NotificationEventHandler.Orphan_Test_Property_3));
                    Assert.That(keyValuePair.Value.ToString(), Is.EqualTo(NotificationEventHandler.Orphan_Test_Value_3));
                });
            });
        }
        #endregion

        #region Validation
        [Test]
        public void IsInvalidEvent_InvalidModel_ReturnsTrue()
        {
            // Arrange
            NotificationEvent notification = JsonSerializer.Deserialize<NotificationEvent>(s_customJson);

            // Act
            bool actualResult = notification.IsInvalidEvent(out int[] invalidPropertiesIndices);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Is.True);
                Assert.That(invalidPropertiesIndices, Has.Length.EqualTo(3));
            });
        }

        [Test]
        public void IsInvalidEvent_ValidModel_ReturnsFalse()
        {
            // Arrange
            NotificationEvent notification = JsonSerializer.Deserialize<NotificationEvent>(ValidCaseJson);

            // Act
            bool actualResult = notification.IsInvalidEvent(out int[] invalidPropertiesIndices);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Is.False);
                Assert.That(invalidPropertiesIndices, Has.Length.Zero);
            });
        }
        #endregion
    }
}