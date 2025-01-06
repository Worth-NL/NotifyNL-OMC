// © 2024, Worth Systems.

using Common.Enums.Responses;
using Common.Extensions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Common.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class EnumExtensionsTests
    {
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
    }
}