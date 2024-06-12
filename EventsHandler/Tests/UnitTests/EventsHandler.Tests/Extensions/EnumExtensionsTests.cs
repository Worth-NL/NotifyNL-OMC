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

        #region ConvertToFeedbackStatus
        // Success
        [TestCase(DeliveryStatuses.Delivered, FeedbackTypes.Success)]
        // Failures
        [TestCase(DeliveryStatuses.PermanentFailure, FeedbackTypes.Failure)]
        [TestCase(DeliveryStatuses.TemporaryFailure, FeedbackTypes.Failure)]
        [TestCase(DeliveryStatuses.TechnicalFailure, FeedbackTypes.Failure)]
        // Info
        [TestCase(DeliveryStatuses.Created, FeedbackTypes.Info)]
        [TestCase(DeliveryStatuses.Sending, FeedbackTypes.Info)]
        [TestCase(DeliveryStatuses.Pending, FeedbackTypes.Info)]
        [TestCase(DeliveryStatuses.Sent, FeedbackTypes.Info)]
        [TestCase(DeliveryStatuses.Accepted, FeedbackTypes.Info)]
        [TestCase(DeliveryStatuses.Received, FeedbackTypes.Info)]
        [TestCase(DeliveryStatuses.Cancelled, FeedbackTypes.Info)]
        public void ConvertToNotifyStatus_ForValidEnum_ReturnsExpectedConvertedValue(DeliveryStatuses testStartValue, FeedbackTypes expectedEndValue)
        {
            // Act
            FeedbackTypes actualValue = testStartValue.ConvertToFeedbackStatus();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }

        [TestCase(DeliveryStatuses.Unknown, FeedbackTypes.Unknown)]
        [TestCase((DeliveryStatuses)666, FeedbackTypes.Unknown)]
        public void ConvertToNotifyStatus_ForInvalidEnum_ReturnsExpectedConvertedValue(DeliveryStatuses testStartValue, FeedbackTypes expectedEndValue)
        {
            // Act
            FeedbackTypes actualValue = testStartValue.ConvertToFeedbackStatus();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }
        #endregion
    }
}