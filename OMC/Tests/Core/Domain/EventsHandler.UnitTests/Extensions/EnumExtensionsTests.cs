// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.Responding.Enums.v2;
using Microsoft.Extensions.Logging;

namespace EventsHandler.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class EnumExtensionsTests
    {
        #region GetEnumName
        [Test]
        public void GetEnumName_WithJsonPropertyNameAttribute_ReturnsCustomEnumOptionName()
        {
            // Act
            string actualResult = IdTypes.Bsn.GetEnumName();  // NOTE: Contains [JsonPropertyName] attribute

            // Assert
            Assert.That(actualResult, Is.EqualTo("bsn"));
        }

        [Test]
        public void GetEnumName_WithoutJsonPropertyNameAttribute_ReturnsDefaultEnumOptionName()
        {
            // Act
            string actualResult = HealthCheck.OK_Valid.GetEnumName();  // NOTE: Doesn't contain [JsonPropertyName] attribute

            // Assert
            Assert.That(actualResult, Is.EqualTo("OK_Valid"));
        }

        [Test]
        public void GetEnumName_NotDefined_ReturnsDefaultEnumOptionName()
        {
            // Arrange
            const int testNumber = 999;

            // Act
            string actualResult = ((IdTypes)testNumber).GetEnumName();  // NOTE: This enum option is not defined in the target enum

            // Assert
            Assert.That(actualResult, Is.EqualTo(testNumber.ToString()));
        }
        #endregion

        #region ConvertToLogLevel
        [TestCase(ProcessingStatus.Success, LogLevel.Information)]
        [TestCase(ProcessingStatus.Skipped, LogLevel.Warning)]
        [TestCase(ProcessingStatus.Aborted, LogLevel.Warning)]
        [TestCase(ProcessingStatus.NotPossible, LogLevel.Error)]
        [TestCase(ProcessingStatus.Failure, LogLevel.Error)]
        public void ConvertToLogLevel_ForValidEnum_ReturnsExpectedConvertedValue(ProcessingStatus testStartValue, LogLevel expectedEndValue)
        {
            // Act
            LogLevel actualValue = testStartValue.ConvertToLogLevel();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }

        [TestCase((ProcessingStatus)666, LogLevel.None)]
        public void ConvertToLogLevel_ForInvalidEnum_ReturnsExpectedConvertedValue(ProcessingStatus testStartValue, LogLevel expectedEndValue)
        {
            // Act
            LogLevel actualValue = testStartValue.ConvertToLogLevel();

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedEndValue));
        }
        #endregion

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