// © 2024, Worth Systems.

using NUnit.Framework;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotifyNL;

namespace ZhvModels.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class NotificationTypesExtensionsTests
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
    }
}