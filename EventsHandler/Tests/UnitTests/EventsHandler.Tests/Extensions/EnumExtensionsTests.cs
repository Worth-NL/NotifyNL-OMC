// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Enums.v2;
using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;
using EventsHandler.Extensions;

namespace EventsHandler.UnitTests.Extensions
{
    [TestFixture]
    public sealed class EnumExtensionsTests
    {
        #region ConvertToNotifyMethod
        [TestCase(NotificationTypes.Email, NotifyMethods.Email)]
        [TestCase(NotificationTypes.Sms, NotifyMethods.Sms)]
        public void ConvertToNotifyMethod_ForValidEnum_ReturnsExpectedConvertedValue(NotificationTypes testStartValue, NotifyMethods expectedEndValue)
        {
            // Act
            NotifyMethods actualValue = testStartValue.ConvertToNotifyMethod();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }

        [TestCase(NotificationTypes.Unknown, NotifyMethods.None)]
        [TestCase((NotificationTypes)666, NotifyMethods.None)]
        public void ConvertToNotifyMethod_ForInvalidEnum_ReturnsExpectedConvertedValue(NotificationTypes testStartValue, NotifyMethods expectedEndValue)
        {
            // Act
            NotifyMethods actualValue = testStartValue.ConvertToNotifyMethod();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }
        #endregion

        #region ConvertToNotifyStatus
        // Success
        [TestCase(DeliveryStatuses.Delivered, NotifyStatuses.Success)]
        // Failures
        [TestCase(DeliveryStatuses.PermanentFailure, NotifyStatuses.Failure)]
        [TestCase(DeliveryStatuses.TemporaryFailure, NotifyStatuses.Failure)]
        [TestCase(DeliveryStatuses.TechnicalFailure, NotifyStatuses.Failure)]
        // Info
        [TestCase(DeliveryStatuses.Created, NotifyStatuses.Info)]
        [TestCase(DeliveryStatuses.Sending, NotifyStatuses.Info)]
        [TestCase(DeliveryStatuses.Pending, NotifyStatuses.Info)]
        [TestCase(DeliveryStatuses.Sent, NotifyStatuses.Info)]
        [TestCase(DeliveryStatuses.Accepted, NotifyStatuses.Info)]
        [TestCase(DeliveryStatuses.Received, NotifyStatuses.Info)]
        [TestCase(DeliveryStatuses.Cancelled, NotifyStatuses.Info)]
        public void ConvertToNotifyStatus_ForValidEnum_ReturnsExpectedConvertedValue(DeliveryStatuses testStartValue, NotifyStatuses expectedEndValue)
        {
            // Act
            NotifyStatuses actualValue = testStartValue.ConvertToNotifyStatus();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }

        [TestCase(DeliveryStatuses.Unknown, NotifyStatuses.Unknown)]
        [TestCase((DeliveryStatuses)666, NotifyStatuses.Unknown)]
        public void ConvertToNotifyStatus_ForInvalidEnum_ReturnsExpectedConvertedValue(DeliveryStatuses testStartValue, NotifyStatuses expectedEndValue)
        {
            // Act
            NotifyStatuses actualValue = testStartValue.ConvertToNotifyStatus();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }
        #endregion
    }
}