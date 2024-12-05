// © 2024, Worth Systems.

using Common.Enums.Responses;
using Common.Enums.Validation;
using Common.Extensions;
using Microsoft.Extensions.Logging;
using ZhvModels.Mapping.Enums.Objecten;

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
    }
}