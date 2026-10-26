// © 2026, Worth Systems.

using NUnit.Framework;
using WebQueries.Tracing;

namespace WebQueries.Tests.Unit.Tracing
{
    /// <summary>
    /// The trace identifier travels inside the reference handed to "Notify NL", and that reference has to be the same
    /// on every delivery of a notification for the check against sending it twice (#247) to find it.
    /// </summary>
    [TestFixture]
    public sealed class TraceContextSeedTests
    {
        [TearDown]
        public void TearDown() => TraceContext.Clear();

        [Test]
        public void Start_SameSeed_GivesTheSameTraceId()
        {
            string first = TraceContext.Start(new TraceEmitter(), seed: "{\"kanaal\":\"zaken\"}");
            string second = TraceContext.Start(new TraceEmitter(), seed: "{\"kanaal\":\"zaken\"}");

            Assert.Multiple(() =>
            {
                Assert.That(second, Is.EqualTo(first));
                Assert.That(first, Has.Length.EqualTo(12));
                Assert.That(TraceContext.CurrentTraceId, Is.EqualTo(first));
            });
        }

        [Test]
        public void Start_DifferentSeed_GivesADifferentTraceId()
        {
            string first = TraceContext.Start(new TraceEmitter(), seed: "{\"kanaal\":\"zaken\"}");
            string second = TraceContext.Start(new TraceEmitter(), seed: "{\"kanaal\":\"besluiten\"}");

            Assert.That(second, Is.Not.EqualTo(first));
        }

        [Test]
        public void Start_NoSeed_GivesARandomTraceId()
        {
            string first = TraceContext.Start(new TraceEmitter());
            string second = TraceContext.Start(new TraceEmitter());

            Assert.That(second, Is.Not.EqualTo(first));
        }
    }
}
