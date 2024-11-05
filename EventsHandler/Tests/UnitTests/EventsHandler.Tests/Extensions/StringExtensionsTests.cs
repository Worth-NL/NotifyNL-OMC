// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Extensions;
using System.Reflection;
using System.Text;
using Guid = System.Guid;

namespace EventsHandler.UnitTests.Extensions
{
    [TestFixture]
    public sealed class StringExtensionsTests
    {
        #region Validation
        [TestCase("", true)]
        [TestCase(" ", false)]
        [TestCase("1", false)]
        [TestCase("abc", false)]
        [TestCase(null, true)]
        public void IsNullOrEmpty_ReturnsExpectedResult(string? testString, bool expectedResult)
        {
            // Act
            bool actualResult = testString.IsNullOrEmpty();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [TestCase("", false)]
        [TestCase(" ", true)]
        [TestCase("1", true)]
        [TestCase("abc", true)]
        [TestCase(null, false)]
        public void IsNotNullOrEmpty_ReturnsExpectedResult(string? testString, bool expectedResult)
        {
            // Act
            bool actualResult = testString.IsNotNullOrEmpty();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        #endregion

        #region Encoding / Decoding (Base64)
        // ReSharper disable StringLiteralTypo
        private const string TestRawString = "This is test example";
        private const string EncodedString = "VGhpcyBpcyB0ZXN0IGV4YW1wbGU=";
        // ReSharper restore StringLiteralTypo

        [Test]
        public void Base64Encode_Empty_ReturnsExpectedString()
        {
            // Act
            string actualResult = Encoding.UTF8.GetBytes(string.Empty).Base64Encode();

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public void Base64Encode_String_ReturnsExpectedString()
        {
            // Act
            string actualResult = Encoding.UTF8.GetBytes(TestRawString).Base64Encode();

            // Assert
            Assert.That(actualResult, Is.EqualTo(EncodedString));  // Confirmed by: https://www.base64encode.org/
        }

        [TestCase("")]
        [TestCase(" ")]
        public void Base64Decode_Empty_ReturnsExpectedString(string invalidTestString)
        {
            // Act
            string actualResult = Encoding.UTF8.GetString(invalidTestString.Base64Decode());

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public void Base64Decode_ReturnsExpectedString()
        {
            // Act
            string actualResult = Encoding.UTF8.GetString(EncodedString.Base64Decode());

            // Assert
            Assert.That(actualResult, Is.EqualTo(TestRawString));  // Confirmed by: https://www.base64decode.org/
        }
        #endregion

        #region Compressing / Decompressing
        // ReSharper disable StringLiteralTypo
        private const string TestReferenceString =
            "{" +
              "\"Notification\":{" +
                "\"actie\":\"create\"," +
                "\"kanaal\":\"zaken\"," +
                "\"resource\":\"status\"," +
                "\"kenmerken\":{" +
                  "\"zaaktype\":\"https://lb.zgw.sandbox-marnix.csp-nijmegen.nl/open-zaak/catalogi/api/v1/zaaktypen/20f1bdac-217c-406c-8a48-0661e8a5ddc2\"," +
                  "\"bronorganisatie\":\"001479179\"," +
                  "\"vertrouwelijkheidaanduiding\":\"-\"," +
                  "\"objectType\":\"http://0.0.0.0:0/\"," +
                  "\"besluittype\":\"http://0.0.0.0:0/\"," +
                  "\"verantwoordelijkeOrganisatie\":\"\"" +
                "}," +
                "\"hoofdObject\":\"https://lb.zgw.sandbox-marnix.csp-nijmegen.nl/open-zaak/zaken/api/v1/zaken/682900a9-4031-480f-9734-e6dff9efd2ec\"," +
                "\"resourceUrl\":\"https://lb.zgw.sandbox-marnix.csp-nijmegen.nl/open-zaak/zaken/api/v1/statussen/a04592bf-663f-4592-8be2-a9ae1d066324\"," +
                "\"aanmaakdatum\":\"2024-09-19T12:52:39.7400000\\u002B00:00\"" +
              "}," +
              "\"CaseId\":\"682900a9-4031-480f-9734-e6dff9efd2ec\"," +
              "\"PartyId\":\"dfbf6cf9-1feb-4f11-98a1-09d80e122bf2\"" +
            "}";

        private const string CompressedEncodedString = "H4sIAAAAAAAACq1SsXLcIBD9F9VGAqSThMqkcmOncLo0K1juuJNAg5DPPo//PYsyGSdFZjKTQAPL231v3/JWPITkrNOQX" +
                                                       "PDF8FaATg6LodARIWFxV1zAA0wUucEFPQUirmGLOoPWBGlbMwj9jDG/U4kbwCW9LhlwSmlZh6qaxvJ2vJYreDOGFzZD9O" +
                                                       "6l1OvCvDvPeERf+qkKC3qWsyvSA1M4ugoWVz2L6mdJX0luxWhAMyk6zRreatZD0zPetgJ7OBijJQkaY/AhHsG7lVrLUjg" +
                                                       "XTadEp+j1GWOKYbvi5M6XEzoDJGxzxvkjIRkhwnhGnZ4+uqAmeLnvgVeZANdpcyn9GUEk4NM1hGh2Hnz8TU/xflecQrDm" +
                                                       "caf6B7P2wXw4lS9tLxXnoMigWrCm55aprm4YtsZahdZI1L+M8muc/hf/jy+x5gBvDkqOlrVtbVk+s35EyUABCkPzqmVDG" +
                                                       "sj6maoYSptJhOSyYVwxoZ6EHA5yqFXZNTyvbxvn8hMne3k27zOseG8o5S+b/QIxve4Jxo621ZZILI6ssUIw1YMgWtNzFJ" +
                                                       "JEy+L9O1gxSL0cAwAA";
        // ReSharper restore StringLiteralTypo

        [Test]
        public async Task CompressGZipAsync_Empty_ReturnsExpectedString()
        {
            // Act
            string actualResult = await string.Empty.CompressGZipAsync(CancellationToken.None);

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public async Task CompressGZipAsync_ReturnsExpectedString()
        {
            // Act
            string compressedReference = await TestReferenceString.CompressGZipAsync(CancellationToken.None);
            string decompressedReference = await compressedReference.DecompressGZipAsync(CancellationToken.None);

            // Assert
            Assert.That(decompressedReference, Is.EqualTo(TestReferenceString));
        }

        [Test]
        public async Task DecompressGZipAsync_Empty_ReturnsExpectedString()
        {
            // Act
            string actualResult = await string.Empty.DecompressGZipAsync(CancellationToken.None);

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public async Task DecompressGZipAsync_ReturnsExpectedString()
        {
            // Act
            string actualResult = await CompressedEncodedString.DecompressGZipAsync(CancellationToken.None);

            // Assert
            Assert.That(actualResult, Is.EqualTo(TestReferenceString));
        }
        #endregion

        #region Conversion
        [Test]
        public void ChangeType_ValidGuidString_ReturnsExpectedGuid()
        {
            // Arrange
            const string validGuidString = "12345678-1234-1234-1234-123456789012";
            Guid expectedResult = new(validGuidString);

            // Act
            Guid actualResult = validGuidString.ChangeType<Guid>();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("12345678-123-1234-123456789012")]
        [TestCase("12345678-1234-1234-12345678901")]
        // ReSharper disable once StringLiteralTypo
        [TestCase("12345678-1234-1234-abcdefghijkl")]
        public void ChangeType_InvalidGuidString_ReturnsDefaultGuid(string invalidGuidString)
        {
            // Act
            Guid actualResult = invalidGuidString.ChangeType<Guid>();

            // Assert
            Assert.That(actualResult, Is.Default);
        }

        [TestCase("https://www.google.com/")]
        [TestCase("ftp://www.aol.com/")]
        public void ChangeType_ValidUriString_ReturnsExpectedUri(string validUriString)
        {
            // Arrange
            Uri expectedResult = new(validUriString);

            // Act
            Uri actualResult = validUriString.ChangeType<Uri>();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("www.something.com")]
        [TestCase("something.gov.com")]
        [TestCase("something")]
        public void ChangeType_InvalidUriString_ReturnsDefaultUri(string invalidUriString)
        {
            // Act
            Uri actualResult = invalidUriString.ChangeType<Uri>();

            // Assert
            Assert.That(actualResult, Is.EqualTo(DefaultValues.Models.EmptyUri));
        }

        [TestCase("1", 1, typeof(int))]
        [TestCase("", 0, typeof(int))]
        [TestCase("1.5", 1.5f, typeof(float))]
        [TestCase("", 0f, typeof(float))]
        [TestCase("true", true, typeof(bool))]
        [TestCase("", false, typeof(bool))]
        [TestCase("xyz", "xyz", typeof(string))]
        [TestCase("", "", typeof(string))]
        public void ChangeType_ValidString_ReturnsValidType(string testString, dynamic expectedResultValue, Type expectedResultType)
        {
            // Arrange
            MethodInfo? method = typeof(StringExtensions)
                .GetMethod(nameof(StringExtensions.ChangeType), BindingFlags.Static | BindingFlags.NonPublic);

            MethodInfo? genericMethod = method?.MakeGenericMethod(expectedResultType);

            // Act
            object? actualResult = genericMethod?.Invoke(null, [testString]);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResultValue));
        }
        #endregion

        #region Modification
        [Test]
        public void Join_EmptyCollection_ReturnsEmptyString()
        {
            // Arrange
            string[] emptyArray = [];

            // Act
            string actualResult = emptyArray.Join();

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public void Join_FullCollection_ReturnsEmptyString()
        {
            // Arrange
            List<string> testList = ["test1", "test2", "test3"];

            // Act
            string actualResult = testList.Join();

            // Assert
            const string expectedResult = "test1, test2, test3";

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        #endregion
    }
}