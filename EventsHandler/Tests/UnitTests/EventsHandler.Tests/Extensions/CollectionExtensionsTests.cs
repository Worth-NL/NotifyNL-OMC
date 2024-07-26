// © 2023, Worth Systems.

using EventsHandler.Extensions;

namespace EventsHandler.UnitTests.Extensions
{
    [TestFixture]
    public sealed class CollectionExtensionsTests
    {
        #region HasMany<T>(this T[])
        [Test]
        public void HasMany_ForValidArray_ReturnsTrue()
        {
            // Arrange
            int[] testArray = { 1, 2 };

            // Act
            bool actualResult = testArray.HasAny();

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void HasMany_ForEmptyArray_ReturnsFalse()
        {
            // Arrange
            int[] testArray = Array.Empty<int>();

            // Act
            bool actualResult = testArray.HasAny();

            // Assert
            Assert.That(actualResult, Is.False);
        }
        #endregion

        #region HasMany<T>(this ICollection<T>)
        [Test]
        public void HasMany_ForValidCollection_ReturnsTrue()
        {
            // Arrange
            List<int> testArray = new() { 1, 2, 3 };

            // Act
            bool actualResult = testArray.HasAny();

            // Assert
            Assert.That(actualResult, Is.True);
        }

        [Test]
        public void HasMany_ForEmptyCollection_ReturnsFalse()
        {
            // Arrange
            Dictionary<int, string> testArray = new();

            // Act
            bool actualResult = testArray.HasAny();

            // Assert
            Assert.That(actualResult, Is.False);
        }
        #endregion
    }
}