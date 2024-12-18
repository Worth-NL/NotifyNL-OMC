﻿// © 2024, Worth Systems.

using Common.Extensions;
using NUnit.Framework;
using System.Reflection;

namespace Common.Tests.Unit.Extensions
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
            TestProperty = default;

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
            TestProperty = 42;

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
            TestProperty = default;

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

            TestProperty = expectedValue;

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