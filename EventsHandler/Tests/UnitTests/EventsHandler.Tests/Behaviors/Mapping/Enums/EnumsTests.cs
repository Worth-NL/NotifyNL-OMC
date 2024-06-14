// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using System.Text.Json;

namespace EventsHandler.UnitTests.Behaviors.Mapping.Enums
{
    [TestFixture]
    public sealed class EnumsTests
    {
        #region Serialization
        // Actions
        [TestCase(typeof(Actions), Actions.Create, "create")]
        [TestCase(typeof(Actions), Actions.Update, "update")]
        [TestCase(typeof(Actions), Actions.Destroy, "destroy")]
        // Channels
        [TestCase(typeof(Channels), Channels.Cases, "zaken")]
        [TestCase(typeof(Channels), Channels.Objects, "objecten")]
        [TestCase(typeof(Channels), Channels.Decisions, "besluiten")]
        // Privacy notices
        [TestCase(typeof(PrivacyNotices), PrivacyNotices.Confidential, "vertrouwelijk")]
        [TestCase(typeof(PrivacyNotices), PrivacyNotices.NonConfidential, "openbaar")]
        // Resources
        [TestCase(typeof(Resources), Resources.Case, "zaak")]
        [TestCase(typeof(Resources), Resources.Object, "object")]
        [TestCase(typeof(Resources), Resources.Status, "status")]
        [TestCase(typeof(Resources), Resources.Decision, "besluit")]
        // Distribution channels
        [TestCase(typeof(DistributionChannels), DistributionChannels.None, "geen")]
        [TestCase(typeof(DistributionChannels), DistributionChannels.Sms, "sms")]
        [TestCase(typeof(DistributionChannels), DistributionChannels.Email, "email")]
        [TestCase(typeof(DistributionChannels), DistributionChannels.Both, "beiden")]
        public void JsonSerializer_CustomEnumSerialization_FromEnglishEnum_ToDutchJson(Type testEnumType, int testEnumValue, string expectedJsonValue)
        {
            // Arrange
            Array allEnumValues = Enum.GetValues(testEnumType);
            object? specificEnumValue = allEnumValues.GetValue(testEnumValue);

            string expectedJson = $"\"{expectedJsonValue}\"";

            // Act
            string actualJson = JsonSerializer.Serialize(specificEnumValue);

            // Assert
            Assert.That(actualJson, Is.EqualTo(expectedJson));
        }
        #endregion

        #region Deserialization
        // Actions
        [TestCase("create", typeof(Actions), Actions.Create)]
        [TestCase("update", typeof(Actions), Actions.Update)]
        [TestCase("destroy", typeof(Actions), Actions.Destroy)]
        // Channels
        [TestCase("zaken", typeof(Channels), Channels.Cases)]
        [TestCase("objecten", typeof(Channels), Channels.Objects)]
        [TestCase("besluiten", typeof(Channels), Channels.Decisions)]
        // Privacy notices
        [TestCase("vertrouwelijk", typeof(PrivacyNotices), PrivacyNotices.Confidential)]
        [TestCase("openbaar", typeof(PrivacyNotices), PrivacyNotices.NonConfidential)]
        // Resources
        [TestCase("zaak", typeof(Resources), Resources.Case)]
        [TestCase("object", typeof(Resources), Resources.Object)]
        [TestCase("status", typeof(Resources), Resources.Status)]
        [TestCase("besluit", typeof(Resources), Resources.Decision)]
        // Distribution channels
        [TestCase("geen", typeof(DistributionChannels), DistributionChannels.None)]
        [TestCase("sms", typeof(DistributionChannels), DistributionChannels.Sms)]
        [TestCase("email", typeof(DistributionChannels), DistributionChannels.Email)]
        [TestCase("beiden", typeof(DistributionChannels), DistributionChannels.Both)]
        public void JsonSerializer_CustomEnumSerialization_FromDutchJson_ToEnglishEnum(string testJsonValue, Type testEnumType, int expectedEnumValue)
        {
            // Arrange
            string testJson = $"\"{testJsonValue}\"";

            // Act
            object? actualEnumValue = JsonSerializer.Deserialize(testJson, testEnumType);

            // Assert
            Assert.That(Convert.ToInt32(actualEnumValue), Is.EqualTo(expectedEnumValue));
        }

        [TestCase("?", typeof(Actions), Actions.Unknown)]
        [TestCase("test", typeof(Channels), Channels.Unknown)]
        [TestCase("", typeof(PrivacyNotices), PrivacyNotices.Unknown)]
        [TestCase("123", typeof(Resources), Resources.Unknown)]
        [TestCase("$#%", typeof(DistributionChannels), DistributionChannels.Unknown)]
        public void JsonSerializer_CustomEnumSerialization_FromUndefinedOption_ToDefaultEnum(string testJsonValue, Type testEnumType, int expectedEnumValue)
        {
            // Arrange
            string testJson = $"\"{testJsonValue}\"";

            // Act
            object? actualEnumValue = JsonSerializer.Deserialize(testJson, testEnumType);

            // Assert
            Assert.That(Convert.ToInt32(actualEnumValue), Is.EqualTo(expectedEnumValue));
        }
        #endregion
    }
}