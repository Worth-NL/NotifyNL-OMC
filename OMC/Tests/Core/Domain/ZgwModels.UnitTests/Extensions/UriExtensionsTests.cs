// © 2024, Worth Systems.

using Common.Constants;
using NUnit.Framework;
using ZgwModels.Extensions;

namespace ZgwModels.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class UriExtensionsTests
    {
        #region GetGuid
        [Test]
        public void GetGuid_ForMissingUri_ReturnsEmptyGuid()
        {
            // Act
            Guid actualGuid = UriExtensions.GetGuid(null);

            // Assert
            Assert.That(actualGuid, Is.Empty);
        }

        [Test]
        public void GetGuid_ForDefaultUri_ReturnsEmptyGuid()
        {
            // Act
            Guid actualGuid = CommonValues.Default.Models.EmptyUri.GetGuid();

            // Assert
            Assert.That(actualGuid, Is.Empty);
        }

        [Test]
        public void GetGuid_ForInvalidUri_ReturnsEmptyGuid()
        {
            // Act
            Guid actualGuid = new Uri("https://www.google.com/").GetGuid();

            // Assert
            Assert.That(actualGuid, Is.Empty);
        }

        [Test]
        public void GetGuid_ForValidUri_ReturnsExtractedGuid()
        {
            // Arrange
            var expectedGuid = new Guid("12345678-1234-1234-1234-123456789012");

            // Act
            Guid actualGuid = new Uri($"https://www.google.com/{expectedGuid}").GetGuid();

            // Assert
            Assert.That(actualGuid, Is.EqualTo(expectedGuid));
        }

        [Test]
        public void GetGuid_ForUriWithMultipleGuids_ReturnsLastGuid()
        {
            var firstGuid = new Guid("11111111-1111-1111-1111-111111111111");
            var lastGuid = new Guid("22222222-2222-2222-2222-222222222222");

            Guid actualGuid = new Uri($"https://www.example.com/{firstGuid}/items/{lastGuid}").GetGuid();

            Assert.That(actualGuid, Is.EqualTo(lastGuid));
        }
        #endregion

        #region IsNullOrDefault / IsNotNullOrDefault
        [Test]
        public void IsNullOrDefault_ForNullUri_ReturnsTrue()
        {
            Uri? uri = null;

            Assert.That(uri.IsNullOrDefault(), Is.True);
        }

        [Test]
        public void IsNullOrDefault_ForDefaultUri_ReturnsTrue()
        {
            Assert.That(CommonValues.Default.Models.EmptyUri.IsNullOrDefault(), Is.True);
        }

        [Test]
        public void IsNullOrDefault_ForValidUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/").IsNullOrDefault(), Is.False);
        }

        [Test]
        public void IsNotNullOrDefault_ForNullUri_ReturnsFalse()
        {
            Uri? uri = null;

            Assert.That(uri.IsNotNullOrDefault(), Is.False);
        }

        [Test]
        public void IsNotNullOrDefault_ForValidUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/").IsNotNullOrDefault(), Is.True);
        }
        #endregion

        #region IsNotCase / IsNotStatus / IsNotStatusType / IsNotCaseType
        [Test]
        public void IsNotCase_ForCaseUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotCase(), Is.False);
        }

        [Test]
        public void IsNotCase_ForNonCaseUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/statussen/123").IsNotCase(), Is.True);
        }

        [Test]
        public void IsNotCase_ForNullUri_ReturnsTrue()
        {
            Uri? uri = null;
            Assert.That(uri.IsNotCase(), Is.True);
        }

        [Test]
        public void IsNotStatus_ForStatusUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/statussen/123").IsNotStatus(), Is.False);
        }

        [Test]
        public void IsNotStatus_ForNonStatusUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotStatus(), Is.True);
        }

        [Test]
        public void IsNotStatusType_ForStatusTypeUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/statustypen/123").IsNotStatusType(), Is.False);
        }

        [Test]
        public void IsNotStatusType_ForNonStatusTypeUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotStatusType(), Is.True);
        }

        [Test]
        public void IsNotCaseType_ForCaseTypeUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/zaaktypen/123").IsNotCaseType(), Is.False);
        }

        [Test]
        public void IsNotCaseType_ForNonCaseTypeUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotCaseType(), Is.True);
        }
        #endregion

        #region IsNotResultType / IsNotParty / IsNotObject / IsNotDecisionResource
        [Test]
        public void IsNotResultType_ForResultTypeUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/resultaattypen/123").IsNotResultType(), Is.False);
        }

        [Test]
        public void IsNotResultType_ForOtherUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotResultType(), Is.True);
        }

        [Test]
        public void IsNotParty_ForPartyUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/partijen/123").IsNotParty(), Is.False);
        }

        [Test]
        public void IsNotParty_ForOtherUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotParty(), Is.True);
        }

        [Test]
        public void IsNotObject_ForObjectUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/objects/123").IsNotObject(), Is.False);
        }

        [Test]
        public void IsNotObject_ForOtherUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotObject(), Is.True);
        }

        [Test]
        public void IsNotDecisionResource_ForDecisionResourceUri_ReturnsFalse()
        {
            Assert.That(new Uri("https://example.com/besluitinformatieobjecten/123").IsNotDecisionResource(), Is.False);
        }

        [Test]
        public void IsNotDecisionResource_ForOtherUri_ReturnsTrue()
        {
            Assert.That(new Uri("https://example.com/zaken/123").IsNotDecisionResource(), Is.True);
        }
        #endregion
    }
}