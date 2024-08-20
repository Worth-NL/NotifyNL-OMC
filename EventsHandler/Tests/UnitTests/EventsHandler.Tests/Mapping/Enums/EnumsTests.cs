// © 2023, Worth Systems.

using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.NotifyNL;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Enums.OpenZaak;
using System.Text.Json;

namespace EventsHandler.UnitTests.Mapping.Enums
{
    [TestFixture]
    public sealed class EnumsTests
    {
        #region Serialization
        // NotificatieApi: Actions
        [TestCase(typeof(Actions), Actions.Create, "create")]
        [TestCase(typeof(Actions), Actions.Update, "update")]
        [TestCase(typeof(Actions), Actions.Destroy, "destroy")]
        // NotificatieApi: Channels
        [TestCase(typeof(Channels), Channels.Cases, "zaken")]
        [TestCase(typeof(Channels), Channels.Objects, "objecten")]
        [TestCase(typeof(Channels), Channels.Decisions, "besluiten")]
        // NotificatieApi: Privacy notices
        [TestCase(typeof(PrivacyNotices), PrivacyNotices.Confidential, "vertrouwelijk")]
        [TestCase(typeof(PrivacyNotices), PrivacyNotices.NonConfidential, "openbaar")]
        // NotificatieApi: Resources
        [TestCase(typeof(Resources), Resources.Status, "status")]
        [TestCase(typeof(Resources), Resources.Object, "object")]
        [TestCase(typeof(Resources), Resources.Decision, "besluitinformatieobject")]
        // OpenKlant: Distribution channels
        [TestCase(typeof(DistributionChannels), DistributionChannels.None, "geen")]
        [TestCase(typeof(DistributionChannels), DistributionChannels.Email, "email")]
        [TestCase(typeof(DistributionChannels), DistributionChannels.Sms, "sms")]
        [TestCase(typeof(DistributionChannels), DistributionChannels.Both, "beiden")]
        // OpenZaak: MessageStatus
        [TestCase(typeof(MessageStatus), MessageStatus.Definitive, "definitief")]
        // Objecten: IdTypes
        [TestCase(typeof(IdTypes), IdTypes.Bsn, "bsn")]
        // Objecten: TaskStatuses
        [TestCase(typeof(TaskStatuses), TaskStatuses.Open, "open")]
        [TestCase(typeof(TaskStatuses), TaskStatuses.Closed, "gesloten")]
        // NotifyNL: Notification types
        [TestCase(typeof(NotificationTypes), NotificationTypes.Email, "email")]
        [TestCase(typeof(NotificationTypes), NotificationTypes.Sms, "sms")]
        // NotifyNL: Delivery statuses
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Created, "created")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Sending, "sending")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Delivered, "delivered")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Pending, "pending")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Sent, "sent")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Accepted, "accepted")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Received, "received")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.Cancelled, "cancelled")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.PermanentFailure, "permanent-failure")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.TemporaryFailure, "temporary-failure")]
        [TestCase(typeof(DeliveryStatuses), DeliveryStatuses.TechnicalFailure, "technical-failure")]
        public void JsonSerializer_CustomEnumSerialization_FromEnglishEnum_ToDutchJson(Type testEnumType, int testEnumValue, string expectedJsonValue)
        {
            // Arrange
            Array allEnumValues = Enum.GetValues(testEnumType);
            object? specificEnumValue = allEnumValues.GetValue(testEnumValue);

            // Act
            string actualJson = JsonSerializer.Serialize(specificEnumValue);

            // Assert
            Assert.That(actualJson, Is.EqualTo($"\"{expectedJsonValue}\""));
        }
        #endregion

        #region Deserialization
        // NotificatieApi: Actions
        [TestCase("create", typeof(Actions), Actions.Create)]
        [TestCase("update", typeof(Actions), Actions.Update)]
        [TestCase("destroy", typeof(Actions), Actions.Destroy)]
        // NotificatieApi: Channels
        [TestCase("zaken", typeof(Channels), Channels.Cases)]
        [TestCase("objecten", typeof(Channels), Channels.Objects)]
        [TestCase("besluiten", typeof(Channels), Channels.Decisions)]
        // NotificatieApi: Privacy notices
        [TestCase("vertrouwelijk", typeof(PrivacyNotices), PrivacyNotices.Confidential)]
        [TestCase("openbaar", typeof(PrivacyNotices), PrivacyNotices.NonConfidential)]
        // NotificatieApi: Resources
        [TestCase("status", typeof(Resources), Resources.Status)]
        [TestCase("object", typeof(Resources), Resources.Object)]
        [TestCase("besluitinformatieobject", typeof(Resources), Resources.Decision)]
        // OpenKlant: Distribution channels
        [TestCase("geen", typeof(DistributionChannels), DistributionChannels.None)]
        [TestCase("email", typeof(DistributionChannels), DistributionChannels.Email)]
        [TestCase("sms", typeof(DistributionChannels), DistributionChannels.Sms)]
        [TestCase("beiden", typeof(DistributionChannels), DistributionChannels.Both)]
        // OpenZaak: MessageStatus
        [TestCase("definitief", typeof(MessageStatus), MessageStatus.Definitive)]
        // Objecten: IdTypes
        [TestCase("bsn", typeof(IdTypes), IdTypes.Bsn)]
        // Objecten: TaskStatuses
        [TestCase("open", typeof(TaskStatuses), TaskStatuses.Open)]
        [TestCase("gesloten", typeof(TaskStatuses), TaskStatuses.Closed)]
        // NotifyNL: Notification types
        [TestCase("email", typeof(NotificationTypes), NotificationTypes.Email)]
        [TestCase("sms", typeof(NotificationTypes), NotificationTypes.Sms)]
        // NotifyNL: Delivery statuses
        [TestCase("created", typeof(DeliveryStatuses), DeliveryStatuses.Created)]
        [TestCase("sending", typeof(DeliveryStatuses), DeliveryStatuses.Sending)]
        [TestCase("delivered", typeof(DeliveryStatuses), DeliveryStatuses.Delivered)]
        [TestCase("pending", typeof(DeliveryStatuses), DeliveryStatuses.Pending)]
        [TestCase("sent", typeof(DeliveryStatuses), DeliveryStatuses.Sent)]
        [TestCase("accepted", typeof(DeliveryStatuses), DeliveryStatuses.Accepted)]
        [TestCase("received", typeof(DeliveryStatuses), DeliveryStatuses.Received)]
        [TestCase("cancelled", typeof(DeliveryStatuses), DeliveryStatuses.Cancelled)]
        [TestCase("permanent-failure", typeof(DeliveryStatuses), DeliveryStatuses.PermanentFailure)]
        [TestCase("temporary-failure", typeof(DeliveryStatuses), DeliveryStatuses.TemporaryFailure)]
        [TestCase("technical-failure", typeof(DeliveryStatuses), DeliveryStatuses.TechnicalFailure)]
        public void JsonSerializer_CustomEnumSerialization_FromDutchJson_ToEnglishEnum(string testJsonValue, Type testEnumType, int expectedEnumValue)
        {
            // Act
            object? actualEnumValue = JsonSerializer.Deserialize($"\"{testJsonValue}\"", testEnumType);

            // Assert
            Assert.That(Convert.ToInt32(actualEnumValue), Is.EqualTo(expectedEnumValue));
        }

        // NotificatieApi
        [TestCase("?", typeof(Actions), Actions.Unknown)]
        [TestCase("test", typeof(Channels), Channels.Unknown)]
        [TestCase("", typeof(PrivacyNotices), PrivacyNotices.Unknown)]
        [TestCase("123", typeof(Resources), Resources.Unknown)]
        // OpenKlant
        [TestCase("$#%", typeof(DistributionChannels), DistributionChannels.Unknown)]
        // OpenZaak
        [TestCase("!", typeof(MessageStatus), MessageStatus.Unknown)]
        // Objecten
        [TestCase(":", typeof(IdTypes), IdTypes.Unknown)]
        [TestCase(";", typeof(TaskStatuses), TaskStatuses.Unknown)]
        // NotifyNL
        [TestCase("-", typeof(NotificationTypes), NotificationTypes.Unknown)]
        [TestCase(" ", typeof(DeliveryStatuses), DeliveryStatuses.Unknown)]
        public void JsonSerializer_CustomEnumSerialization_FromUndefinedOption_ToDefaultEnum(string testJsonValue, Type testEnumType, int expectedEnumValue)
        {
            // Act
            object? actualEnumValue = JsonSerializer.Deserialize($"\"{testJsonValue}\"", testEnumType);

            // Assert
            Assert.That(Convert.ToInt32(actualEnumValue), Is.EqualTo(expectedEnumValue));
        }
        #endregion
    }
}