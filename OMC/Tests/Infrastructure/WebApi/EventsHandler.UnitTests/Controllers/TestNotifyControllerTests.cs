// © 2026, Worth Systems.

using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Controllers;

namespace EventsHandler.Tests.Unit.Controllers
{
    [TestFixture]
    public sealed class TestNotifyControllerTests
    {
        #region Test data
        private const string ValidPdfBase64 =
            "JVBERi0xLjAKMSAwIG9iajw8L1R5cGUvQ2F0YWxvZy9QYWdlcyAyIDAgUj4+ZW5kb2JqIDIgMCBvYmo8" +
            "PC9UeXBlL1BhZ2VzL0tpZHNbMyAwIFJdL0NvdW50IDE+PmVuZG9iaiAzIDAgb2JqPDwvVHlwZS9QYWdl" +
            "L01lZGlhQm94WzAgMCAzIDNdPj5lbmRvYmogCnhyZWYgCjAgNCAKMDAwMDAwMDAwMCA2NTUzNSBmIAow" +
            "MDAwMDAwMDA5IDAwMDAwIG4gCjAwMDAwMDAwNTggMDAwMDAgbiAKMDAwMDAwMDExNSAwMDAwMCBuIAp0" +
            "cmFpbGVyIDw8L1NpemUgNC9Sb290IDEgMCBSPj4Kc3RhcnR4cmVmIAoxOTAKJSVFT0Y=";

        private const string ValidRecipientEmail = "test@example.com";
        private const string ValidFileName = "test.pdf";
        #endregion

        #region Download link construction tests
        [Test]
        public void DownloadLink_ValidUuidAndEmail_ProducesCorrectUrl()
        {
            // Arrange
            string uuid = "bbd42e9b-e15e-4954-8e89-4b1b8f2181e5";
            string email = "test@example.com";

            // Act
            string downloadLink = $"https://postguard.eu/download?uuid={uuid}&recipient={Uri.EscapeDataString(email)}";

            // Assert
            Assert.That(downloadLink, Is.EqualTo("https://postguard.eu/download?uuid=bbd42e9b-e15e-4954-8e89-4b1b8f2181e5&recipient=test%40example.com"));
        }

        [Test]
        public void DownloadLink_EmailWithPlusSign_IsCorrectlyEncoded()
        {
            // Arrange
            string uuid = "bbd42e9b-e15e-4954-8e89-4b1b8f2181e5";
            string email = "test+tag@example.com";

            // Act
            string downloadLink = $"https://postguard.eu/download?uuid={uuid}&recipient={Uri.EscapeDataString(email)}";

            // Assert
            Assert.That(downloadLink, Does.Contain("test%2Btag%40example.com"));
        }

        [TestCase("test@example.com", "test%40example.com")]
        [TestCase("user+tag@domain.nl", "user%2Btag%40domain.nl")]
        [TestCase("name.surname@gov.nl", "name.surname%40gov.nl")]
        public void DownloadLink_VariousEmails_AreCorrectlyEncoded(string email, string expectedEncoded)
        {
            // Act
            string encoded = Uri.EscapeDataString(email);

            // Assert
            Assert.That(encoded, Is.EqualTo(expectedEncoded));
        }
        #endregion

        #region SendPostGuardRequest DTO tests
        [Test]
        public void SendPostGuardRequest_DefaultFileName_IsDocumentPdf()
        {
            // Act
            var request = new SendPostGuardRequest { PdfBase64 = ValidPdfBase64 };

            // Assert
            Assert.That(request.FileName, Is.EqualTo("document.pdf"));
        }

        [Test]
        public void SendPostGuardRequest_CustomFileName_IsRetained()
        {
            // Act
            var request = new SendPostGuardRequest
            {
                PdfBase64 = ValidPdfBase64,
                FileName = ValidFileName
            };

            // Assert
            Assert.That(request.FileName, Is.EqualTo(ValidFileName));
        }

        [Test]
        public void SendPostGuardRequest_ValidBase64_DecodesSuccessfully()
        {
            // Arrange
            var request = new SendPostGuardRequest { PdfBase64 = ValidPdfBase64 };

            // Act
            byte[] decoded = Convert.FromBase64String(request.PdfBase64);

            // Assert
            Assert.That(decoded, Is.Not.Empty);
        }

        [Test]
        public void SendPostGuardRequest_InvalidBase64_ThrowsFormatException()
        {
            // Arrange
            var request = new SendPostGuardRequest { PdfBase64 = "this-is-not-valid-base64!!!" };

            // Act & Assert
            Assert.Throws<FormatException>(() => Convert.FromBase64String(request.PdfBase64));
        }

        [TestCase("not-base64-@@##")]
        [TestCase("====")]
        [TestCase("!!!")]
        public void SendPostGuardRequest_InvalidBase64Variants_ThrowsFormatException(string invalidBase64)
        {
            // Arrange
            var request = new SendPostGuardRequest { PdfBase64 = invalidBase64 };

            // Act & Assert
            Assert.Throws<FormatException>(() => Convert.FromBase64String(request.PdfBase64));
        }
        #endregion

        #region OmcConfiguration PostGuard section tests
        [Test]
        public void OmcConfiguration_PostGuard_ComponentExists()
        {
            // Arrange
            var configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act & Assert
            Assert.That(configuration.PostGuard, Is.Not.Null);
            Assert.That(configuration.PostGuard.API, Is.Not.Null);
            Assert.That(configuration.PostGuard.TemplateId, Is.Not.Null);
        }

        [Test]
        public void OmcConfiguration_PostGuard_ApiKey_ReturnsConfiguredValue()
        {
            // Arrange
            var configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act
            string apiKey = configuration.PostGuard.API.Key();

            // Assert
            Assert.That(apiKey, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void OmcConfiguration_PostGuard_PkgUrl_ReturnsConfiguredValue()
        {
            // Arrange
            var configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act
            string pkgUrl = configuration.PostGuard.API.PkgUrl();

            // Assert
            Assert.That(pkgUrl, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void OmcConfiguration_PostGuard_CryptifyUrl_ReturnsConfiguredValue()
        {
            // Arrange
            var configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act
            string cryptifyUrl = configuration.PostGuard.API.CryptifyUrl();

            // Assert
            Assert.That(cryptifyUrl, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void OmcConfiguration_PostGuard_TemplateId_ReturnsConfiguredValue()
        {
            // Arrange
            var configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act
            Guid templateId = configuration.PostGuard.TemplateId.SendPostGuardPdf();

            // Assert
            Assert.That(templateId, Is.Not.EqualTo(Guid.Empty));
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1).Dispose();
        }
        #endregion
    }
}