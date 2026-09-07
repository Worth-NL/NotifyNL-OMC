// © 2024, Worth Systems.

using Common.Versioning.Models;
using NUnit.Framework;

namespace Common.Tests.Unit.Versioning
{
    [TestFixture]
    public sealed class OmcVersionTests
    {
        [SetUp]
        public void Reset()
        {
            OmcVersion.SetVersion(0, 0, 0);
        }

        [Test]
        public void SetVersion_GetNetVersion_ReturnsExpectedFormat()
        {
            OmcVersion.SetVersion(2, 0, 1);
            Assert.That(OmcVersion.GetNetVersion(), Is.EqualTo("2.01"));
        }

        [Test]
        public void SetVersion_GetExpandedVersion_ReturnsExpectedFormat()
        {
            OmcVersion.SetVersion(2, 0, 1);
            Assert.That(OmcVersion.GetExpandedVersion(), Is.EqualTo("2.0.1"));
        }

        [Test]
        public void SetVersion_ToString_ReturnsExpandedVersion()
        {
            OmcVersion.SetVersion(1, 17, 19);
            Assert.That(new OmcVersion().ToString(), Is.EqualTo("1.17.19"));
        }

        [Test]
        public void SetVersion_MultiDigitMinorAndPatch_FormatsCorrectly()
        {
            OmcVersion.SetVersion(1, 10, 5);
            Assert.Multiple(() =>
            {
                Assert.That(OmcVersion.GetNetVersion(), Is.EqualTo("1.105"));
                Assert.That(OmcVersion.GetExpandedVersion(), Is.EqualTo("1.10.5"));
            });
        }

        [Test]
        public void Default_BeforeSetVersion_ReturnsZeros()
        {
            Assert.Multiple(() =>
            {
                Assert.That(OmcVersion.GetNetVersion(), Is.EqualTo("0.00"));
                Assert.That(OmcVersion.GetExpandedVersion(), Is.EqualTo("0.0.0"));
            });
        }
    }
}
