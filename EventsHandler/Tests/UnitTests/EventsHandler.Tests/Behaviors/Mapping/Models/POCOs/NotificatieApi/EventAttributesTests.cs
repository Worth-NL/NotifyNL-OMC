// © 2023, Worth Systems.

using System.Text.Json;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Constants;

namespace EventsHandler.UnitTests.Behaviors.Mapping.Models.POCOs.NotificatieApi
{
    [TestFixture]
    public sealed class EventAttributesTests
    {
        #region Test data
        private const string DefaultJson =
            "{" +
               "\"objectType\":null," +
               "\"zaaktype\":null," +
               "\"bronorganisatie\":null," +
               "\"vertrouwelijkheidaanduiding\":null" +
            "}";

        private static readonly string s_customJson =
            "{" +
               $"\"objectType\":\"{DefaultValues.Models.EmptyUri}\"," +
               $"\"zaaktype\":\"{DefaultValues.Models.EmptyUri}\"," +
                "\"bronorganisatie\":\"123456789\"," +
                "\"vertrouwelijkheidaanduiding\":2" +
            "}";

        private static readonly string s_unexpectedJson =
            "{" +
               $"\"objectType\":\"{DefaultValues.Models.EmptyUri}\"," +
               $"\"zaaktype\":\"{DefaultValues.Models.EmptyUri}\"," +
                "\"bronorganisatie\":\"123456789\"," +
                "\"vertrouwelijkheidaanduiding\":2," +
                "\"test1\":\"abc\"," +
                "\"test2\":\"xyz\"" +
            "}";
        #endregion

        #region Serialization
        [Test]
        public void Serialization_Object_IntoJson()
        {
            // Arrange
            var testObject = new EventAttributes();

            // Act
            string actualJson = JsonSerializer.Serialize(testObject);

            // Assert
            Assert.That(actualJson, Is.EqualTo(DefaultJson));
        }
        #endregion

        #region Deserialization
        [Test]
        public void Deserialization_Valid_Json_IntoObject_WithoutOrphans()
        {
            // Act
            EventAttributes actualObject = JsonSerializer.Deserialize<EventAttributes>(s_customJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualObject.ObjectType, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.CaseTypeUri, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.SourceOrganization, Is.EqualTo("123456789"));
                Assert.That(actualObject.ConfidentialityNotice, Is.EqualTo(PrivacyNotices.NonConfidential));
                Assert.That(actualObject.Orphans, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void Deserialization_Unexpected_Json_IntoObject_WithOrphans()
        {
            // Act
            EventAttributes actualObject = JsonSerializer.Deserialize<EventAttributes>(s_unexpectedJson);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualObject.ObjectType, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.CaseTypeUri, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.SourceOrganization, Is.EqualTo("123456789"));
                Assert.That(actualObject.ConfidentialityNotice, Is.EqualTo(PrivacyNotices.NonConfidential));
                Assert.That(actualObject.Orphans, Has.Count.EqualTo(2));
                Assert.Multiple(() =>
                {
                    KeyValuePair<string, object> firstKeyValuePair = actualObject.Orphans.First();

                    Assert.That(firstKeyValuePair.Key, Is.EqualTo("test1"));
                    Assert.That(firstKeyValuePair.Value.ToString(), Is.EqualTo("abc"));

                    KeyValuePair<string, object> lastKeyValuePair = actualObject.Orphans.Last();

                    Assert.That(lastKeyValuePair.Key, Is.EqualTo("test2"));
                    Assert.That(lastKeyValuePair.Value.ToString(), Is.EqualTo("xyz"));
                });
            });
        }
        #endregion
    }
}