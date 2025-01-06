// © 2023, Worth Systems.

using Common.Extensions;
using NUnit.Framework;

namespace Common.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class CollectionExtensionsTests
    {
        #region HasAny<T>(this T[])
        [Test]
        public void HasAny_ForValidArray_ReturnsTrue()
        {
            // Arrange
            int[] testArray = [1, 2];

            // Act
            bool actualResult = testArray.HasAny();

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void HasAny_ForEmptyArray_ReturnsFalse()
        {
            // Arrange
            int[] testArray = [];

            // Act
            bool actualResult = testArray.HasAny();

            // Assert
            Assert.That(actualResult, Is.False);
        }
        #endregion

        #region HasAny<T>(this ICollection)
        [Test]
        public void HasAny_ForValidCollection_ReturnsTrue()
        {
            // Arrange
            List<int> testArray = [1, 2, 3];

            // Act
            bool actualResult = testArray.HasAny();

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void HasAny_ForEmptyCollection_ReturnsFalse()
        {
            // Act
            bool actualResult = new Dictionary<int, string>().HasAny();

            // Assert
            Assert.That(actualResult, Is.False);
        }
        #endregion

        #region IsEmpty<T>(this T[])
        [Test]
        public void IsEmpty_ForValidArray_ReturnsFalse()
        {
            // Arrange
            int[] testArray = [1, 2];

            // Act
            bool actualResult = testArray.IsEmpty();

            // Assert
            Assert.That(actualResult, Is.False);
        }

        [Test]
        public void IsEmpty_ForEmptyArray_ReturnsTrue()
        {
            // Arrange
            int[] testArray = [];

            // Act
            bool actualResult = testArray.IsEmpty();

            // Assert
            Assert.That(actualResult, Is.True);
        }
        #endregion

        #region IsEmpty<T>(this ICollection)
        [Test]
        public void IsEmpty_ForValidCollection_ReturnsFalse()
        {
            // Arrange
            List<int> testArray = [1, 2, 3];

            // Act
            bool actualResult = testArray.IsEmpty();

            // Assert
            Assert.That(actualResult, Is.False);
        }

        [Test]
        public void IsEmpty_ForEmptyCollection_ReturnsTrue()
        {
            // Act
            bool actualResult = new Dictionary<int, string>().IsEmpty();

            // Assert
            Assert.That(actualResult, Is.True);
        }
        #endregion
    }
}