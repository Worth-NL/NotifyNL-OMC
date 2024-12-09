// © 2024, Worth Systems.

using NUnit.Framework;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotifyNL;

namespace ZhvModels.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class DeliveryStatusesExtensionsTests
    {
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