// © 2023, Worth Systems.

using System.Text.Json;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Constants;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.UnitTests.Behaviors.Mapping.Models.POCOs.NotificatieApi
{
    [TestFixture]
    public sealed class NotificationEventTests
    {
        #region Test data
        private const string ValidJson =
            "{" +
                "\"actie\":\"create\"," +
                "\"kanaal\":\"zaken\"," +
                "\"resource\":\"status\"," +
                "\"kenmerken\":{" +
                    "\"objectType\":\"http://0.0.0.0:0/\"," +
                    "\"zaaktype\":\"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\"," +
                    "\"bronorganisatie\":\"286130270\"," +
                    "\"vertrouwelijkheidaanduiding\":\"openbaar\"" +
                "}," +
                "\"hoofdObject\":\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\"," +
                "\"resourceUrl\":\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\"," +
                "\"aanmaakdatum\":\"2023-09-22T11:41:46.052Z\"" +
            "}";

        private static readonly string s_customJson =
            "{" +
                "\"actie\":0," +
                "\"kanaal\":1," +
                "\"resource\":2," +
                "\"kenmerken\":" +
                "{" +
                    "\"objectType\":null," +
                    "\"zaaktype\":null," +
                    "\"bronorganisatie\":null," +
                    "\"vertrouwelijkheidaanduiding\":null" +
                "}," +
               $"\"hoofdObject\":\"{DefaultValues.Models.EmptyUri.AbsoluteUri}\"," +
               $"\"resourceUrl\":\"{DefaultValues.Models.EmptyUri.AbsoluteUri}\"," +
                "\"aanmaakdatum\":\"0001-01-01T00:00:00\"" +
            "}";

        private static readonly string s_unexpectedJson =
            "{" +
                "\"actie\":0," +
                "\"kanaal\":1," +
                "\"resource\":2," +
                "\"kenmerken\":" +
                "{" +
                    "\"objectType\":null," +
                    "\"zaaktype\":null," +
                    "\"bronorganisatie\":null," +
                    "\"vertrouwelijkheidaanduiding\":null" +
                "}," +
               $"\"hoofdObject\":\"{DefaultValues.Models.EmptyUri.AbsoluteUri}\"," +
               $"\"resourceUrl\":\"{DefaultValues.Models.EmptyUri.AbsoluteUri}\"," +
                "\"aanmaakdatum\":\"0001-01-01T00:00:00\"," +
                "\"test\":\"xyz\"" +
            "}";
        #endregion

        #region Serialization
        [Test]
        public void Serialization_Object_IntoJson()
        {
            // Arrange
            NotificationEvent testObject = NotificationEventHandler.GetNotification_Real_TheHague();

            // Act
            string actualJson = JsonSerializer.Serialize(testObject);

            // Assert
            Assert.That(actualJson, Is.EqualTo(ValidJson));
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
                Assert.That(actualObject.MainObject, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.ResourceUrl, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.CreateDate, Is.EqualTo(default(DateTime)));
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
                Assert.That(actualObject.MainObject, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.ResourceUrl, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.CreateDate, Is.EqualTo(default(DateTime)));
                Assert.That(actualObject.Orphans, Has.Count.EqualTo(1));
                Assert.Multiple(() =>
                {
                    KeyValuePair<string, object> keyValuePair = actualObject.Orphans.First();

                    Assert.That(keyValuePair.Key, Is.EqualTo("test"));
                    Assert.That(keyValuePair.Value.ToString(), Is.EqualTo("xyz"));
                });
            });
        }

        // TODO: Add tests for [Required] properties
        #endregion
    }
}