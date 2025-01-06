// © 2024, Worth Systems.

using Common.Extensions;
using NUnit.Framework;
using ZhvModels.Enums;
using ZhvModels.Mapping.Enums.Objecten;

namespace ZhvModels.Tests.Unit.Extensions
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
    }
}