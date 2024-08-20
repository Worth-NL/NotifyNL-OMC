// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Utilities._TestHelpers;
using System.Text.Json;

namespace EventsHandler.UnitTests.Mapping.Models.POCOs.NotificatieApi
{
    [TestFixture]
    public sealed class EventAttributesTests
    {
        #region Test data
        private static readonly Uri s_testUri = new("https://www.test.fr/");

        private const string DefaultJson =
            "{" +
               // Cases
               "\"zaaktype\":null," +
               "\"bronorganisatie\":null," +
               "\"vertrouwelijkheidaanduiding\":null," +
               // Objects
               "\"objectType\":null," +
               // Decisions
               "\"besluittype\":null," +
               "\"verantwoordelijkeOrganisatie\":null" +
            "}";

        private static readonly string s_validJson =
            "{" +
               // Cases
               $"\"zaaktype\": \"{s_testUri}\", " +
               $"\"bronorganisatie\": \"{NotificationEventHandler.SourceOrganization_Real_TheHague}\", " +
                "\"vertrouwelijkheidaanduiding\": 2, " +
               // Objects
               $"\"objectType\": \"{s_testUri}\", " +
               // Decisions
               $"\"besluittype\": \"{s_testUri}\", " +
               $"\"verantwoordelijkeOrganisatie\": \"{NotificationEventHandler.ResponsibleOrganization_Real_TheHague}\"" +
            "}";

        private static readonly string s_unexpectedJson =
            "{" +
               // Cases
               $"\"zaaktype\": \"{DefaultValues.Models.EmptyUri}\", " +
               $"\"bronorganisatie\": \"{NotificationEventHandler.SourceOrganization_Real_TheHague}\", " +
                "\"vertrouwelijkheidaanduiding\": 2, " +
               // Objects
               $"\"objectType\": \"{DefaultValues.Models.EmptyUri}\", " +
               // Decisions
               $"\"besluittype\": \"{DefaultValues.Models.EmptyUri}\", " +
               $"\"verantwoordelijkeOrganisatie\": \"{NotificationEventHandler.ResponsibleOrganization_Real_TheHague}\", " +
               // Orphans
               $"\"{NotificationEventHandler.Orphan_Test_Property_1}\": \"{NotificationEventHandler.Orphan_Test_Value_1}\", " +
               $"\"{NotificationEventHandler.Orphan_Test_Property_2}\": \"{NotificationEventHandler.Orphan_Test_Value_2}\"" +
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
            EventAttributes actualObject = JsonSerializer.Deserialize<EventAttributes>(s_validJson);

            // Assert
            Assert.Multiple(() =>
            {
                // Cases
                Assert.That(actualObject.CaseTypeUri, Is.EqualTo(s_testUri));
                Assert.That(actualObject.SourceOrganization, Is.EqualTo(NotificationEventHandler.SourceOrganization_Real_TheHague));
                Assert.That(actualObject.ConfidentialityNotice, Is.EqualTo(PrivacyNotices.NonConfidential));
                // Objects
                Assert.That(actualObject.ObjectTypeUri, Is.EqualTo(s_testUri));
                // Decisions
                Assert.That(actualObject.DecisionTypeUri, Is.EqualTo(s_testUri));
                Assert.That(actualObject.ResponsibleOrganization, Is.EqualTo(NotificationEventHandler.ResponsibleOrganization_Real_TheHague));
                // Orphans
                Assert.That(actualObject.Orphans, Has.Count.Zero);
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
                // Cases
                Assert.That(actualObject.CaseTypeUri, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.SourceOrganization, Is.EqualTo(NotificationEventHandler.SourceOrganization_Real_TheHague));
                Assert.That(actualObject.ConfidentialityNotice, Is.EqualTo(PrivacyNotices.NonConfidential));
                // Objects
                Assert.That(actualObject.ObjectTypeUri, Is.EqualTo(DefaultValues.Models.EmptyUri));
                // Decisions
                Assert.That(actualObject.DecisionTypeUri, Is.EqualTo(DefaultValues.Models.EmptyUri));
                Assert.That(actualObject.ResponsibleOrganization, Is.EqualTo(NotificationEventHandler.ResponsibleOrganization_Real_TheHague));
                // Orphans
                Assert.That(actualObject.Orphans, Has.Count.EqualTo(2));
                Assert.Multiple(() =>
                {
                    KeyValuePair<string, object> firstKeyValuePair = actualObject.Orphans.First();

                    Assert.That(firstKeyValuePair.Key, Is.EqualTo(NotificationEventHandler.Orphan_Test_Property_1));
                    Assert.That(firstKeyValuePair.Value.ToString(), Is.EqualTo(NotificationEventHandler.Orphan_Test_Value_1.ToString()));

                    KeyValuePair<string, object> lastKeyValuePair = actualObject.Orphans.Last();

                    Assert.That(lastKeyValuePair.Key, Is.EqualTo(NotificationEventHandler.Orphan_Test_Property_2));
                    Assert.That(lastKeyValuePair.Value.ToString(), Is.EqualTo(NotificationEventHandler.Orphan_Test_Value_2.ToString()));
                });
            });
        }
        #endregion

        #region Validation
        [Test]
        public void IsInvalidCase_InvalidModel_ReturnsTrue()
        {
            // Arrange
            EventAttributes eventAttributes = JsonSerializer.Deserialize<EventAttributes>(DefaultJson);

            // Act
            bool actualResult = EventAttributes.IsInvalidCase(eventAttributes);

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void IsInvalidCase_ValidModel_ReturnsFalse()
        {
            // Arrange
            EventAttributes eventAttributes = JsonSerializer.Deserialize<EventAttributes>(s_validJson);

            // Act
            bool actualResult = EventAttributes.IsInvalidCase(eventAttributes);

            // Assert
            Assert.That(actualResult, Is.False);
        }

        [Test]
        public void IsInvalidObject_InvalidModel_ReturnsTrue()
        {
            // Arrange
            EventAttributes eventAttributes = JsonSerializer.Deserialize<EventAttributes>(DefaultJson);

            // Act
            bool actualResult = EventAttributes.IsInvalidObject(eventAttributes);

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void IsInvalidObject_ValidModel_ReturnsFalse()
        {
            // Arrange
            EventAttributes eventAttributes = JsonSerializer.Deserialize<EventAttributes>(s_validJson);

            // Act
            bool actualResult = EventAttributes.IsInvalidObject(eventAttributes);

            // Assert
            Assert.That(actualResult, Is.False);
        }

        [Test]
        public void IsInvalidDecision_InvalidModel_ReturnsTrue()
        {
            // Arrange
            EventAttributes eventAttributes = JsonSerializer.Deserialize<EventAttributes>(DefaultJson);

            // Act
            bool actualResult = EventAttributes.IsInvalidDecision(eventAttributes);

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void IsInvalidDecision_ValidModel_ReturnsFalse()
        {
            // Arrange
            EventAttributes eventAttributes = JsonSerializer.Deserialize<EventAttributes>(s_validJson);

            // Act
            bool actualResult = EventAttributes.IsInvalidDecision(eventAttributes);

            // Assert
            Assert.That(actualResult, Is.False);
        }
        #endregion
    }
}