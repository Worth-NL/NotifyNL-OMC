// © 2026, Worth Systems.

using NUnit.Framework;
using ZgwModels.Enums;
using ZgwModels.Extensions;

namespace ZgwModels.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class NotifyMethodsExtensionsTests
    {
        // Post is recorded in Dutch, capitalised to match the casing of the other channel values.
        [TestCase(NotifyMethods.Letter, "Brief")]
        // The rest keep the value they have always written - "Mobb" in particular is the established
        // abbreviation for MijnOverheid Berichtenbox, and live flows already write these.
        [TestCase(NotifyMethods.Email, "Email")]
        [TestCase(NotifyMethods.Sms, "Sms")]
        [TestCase(NotifyMethods.Mobb, "Mobb")]
        [TestCase(NotifyMethods.None, "None")]
        public void ToKanaal_ReturnsTheValueRecordedOnTheKlantcontact(NotifyMethods testMethod, string expectedKanaal)
        {
            // Act
            string actualResult = testMethod.ToKanaal();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedKanaal));
        }
    }
}
