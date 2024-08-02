// © 2024, Worth Systems.

using EventsHandler.Extensions;
using System.Reflection;

namespace EventsHandler.UnitTests.Extensions
{
    [TestFixture]
    public sealed class ReflectionExtensionsTests
    {
        public int? TestProperty { get; private set; }

        #region NotInitializedProperty
        [Test]
        public void NotInitializedProperty_ForUninitialized_NullableProperty_ReturnsTrue()
        {
            // Arrange
            this.TestProperty = default;

            PropertyInfo propertyInfo = GetPropertyInfo();

            // Act
            bool actualResult = this.NotInitializedProperty(propertyInfo);

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void NotInitializedProperty_ForInitialized_NullableProperty_ReturnsFalse()
        {
            // Arrange
            this.TestProperty = 42;

            PropertyInfo propertyInfo = GetPropertyInfo();

            // Act
            bool actualResult = this.NotInitializedProperty(propertyInfo);

            // Assert
            Assert.That(actualResult, Is.False);
        }
        #endregion

        #region GetPropertyValue
        [Test]
        public void GetPropertyValue_UninitializedProperty_ReturnsDefaultValue()
        {
            // Arrange
            this.TestProperty = default;

            PropertyInfo propertyInfo = GetPropertyInfo();

            // Act
            object? actualValue = this.GetPropertyValue(propertyInfo);

            // Assert
            Assert.That(actualValue, Is.Null);
        }

        [Test]
        public void GetPropertyValue_InitializedProperty_ReturnsSetValue()
        {
            // Arrange
            const int expectedValue = 8;

            this.TestProperty = expectedValue;

            PropertyInfo propertyInfo = GetPropertyInfo();

            // Act
            object? actualValue = this.GetPropertyValue(propertyInfo);

            // Assert
            Assert.That(actualValue, Is.TypeOf<int>());
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
        #endregion

        private static PropertyInfo GetPropertyInfo()
        {
            return typeof(ReflectionExtensionsTests).GetProperty(nameof(TestProperty))!;
        }
    }
}