// © 2026, Worth Systems.

using Common.Extensions;
using NUnit.Framework;

namespace Common.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class GuidExtensionsTests
    {
        private static readonly Guid s_dnsNamespace = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

        [Test]
        public void CreateNameBased_KnownInput_ReturnsRfcVersion5Guid()
        {
            // Act
            Guid actualResult = s_dnsNamespace.CreateNameBased("www.example.com");

            // Assert
            Assert.That(actualResult, Is.EqualTo(new Guid("2ed6657d-e927-568b-95e1-2665a8aea6a2")));
        }

        [Test]
        public void CreateNameBased_SameInput_ReturnsSameGuid()
        {
            // Act
            Guid first = s_dnsNamespace.CreateNameBased("a");
            Guid second = s_dnsNamespace.CreateNameBased("a");

            // Assert
            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void CreateNameBased_DifferentName_ReturnsDifferentGuid()
        {
            // Act & Assert
            Assert.That(s_dnsNamespace.CreateNameBased("a"), Is.Not.EqualTo(s_dnsNamespace.CreateNameBased("b")));
        }
    }
}
