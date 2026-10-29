// © 2026, Worth Systems.

using Sentry;
using Sentry.Protocol;

namespace EventsHandler.Tests.Unit.Services
{
    /// <summary>
    /// The safety net in front of Sentry: whatever code sends there, a BSN or KVK number never leaves with it.
    /// </summary>
    [TestFixture]
    public sealed class SentryScrubbingTests
    {
        private const string PartySearch =
            "GET https://openklant.test/partijen?partijIdentificator__codeSoortObjectId=bsn&partijIdentificator__objectId=123456789";

        [Test]
        public void ScrubPersonalData_Event_MasksTheBsnInTheMessageAndTheExceptions()
        {
            // Arrange
            var sentryEvent = new SentryEvent
            {
                Message = new SentryMessage { Message = PartySearch, Formatted = PartySearch },
                SentryExceptions = [new SentryException { Value = $"Party not found | URI: {PartySearch}" }]
            };

            // Act
            SentryEvent result = Program.ScrubPersonalData(sentryEvent);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Message!.Message, Does.EndWith("partijIdentificator__objectId=[…]"));
                Assert.That(result.Message.Formatted, Does.Not.Contain("123456789"));
                Assert.That(result.SentryExceptions!.Single().Value, Does.StartWith("Party not found | URI: GET https://openklant.test/partijen"));
                Assert.That(result.SentryExceptions!.Single().Value, Does.Not.Contain("123456789"));
            });
        }

        [Test]
        public void ScrubPersonalData_Breadcrumb_MasksTheBsnInTheMessageAndData()
        {
            // Arrange
            var breadcrumb = new Breadcrumb(PartySearch, "http", new Dictionary<string, string> { ["url"] = PartySearch }, "http.client", BreadcrumbLevel.Error);

            // Act
            Breadcrumb result = Program.ScrubPersonalData(breadcrumb);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Message, Does.Not.Contain("123456789"));
                Assert.That(result.Data!["url"], Does.EndWith("partijIdentificator__objectId=[…]"));
                Assert.That(result.Type, Is.EqualTo("http"));
                Assert.That(result.Category, Is.EqualTo("http.client"));
                Assert.That(result.Level, Is.EqualTo(BreadcrumbLevel.Error));
            });
        }
    }
}
