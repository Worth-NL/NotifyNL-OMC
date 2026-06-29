// © 2024, Worth Systems.

using Moq;
using NUnit.Framework;
using WebQueries.DataSending;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using ZgwModels.Enums;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.DataSending
{
    [TestFixture]
    public sealed class NotifyServiceTests
    {
        private Mock<INotifyClient> _notifyClientMock = null!;
        private Mock<IHttpClientFactory<INotifyClient, string>> _factoryMock = null!;
        private Mock<ISerializationService> _serializerMock = null!;
        private INotifyService<NotifyData> _service = null!;

        private static readonly NotificationEvent s_notification = new()
        {
            Channel = Channels.Cases,
            Attributes = new EventAttributes { SourceOrganization = "001122334" }
        };

        private static readonly NotifyData s_emailData = new(
            NotifyMethods.Email,
            "test@example.com",
            Guid.NewGuid(),
            new Dictionary<string, object> { ["name"] = "Test" },
            new NotifyReference { CaseId = Guid.NewGuid(), PartyId = Guid.NewGuid(), Notification = s_notification });

        private static readonly NotifyData s_smsData = new(
            NotifyMethods.Sms,
            "0612345678",
            Guid.NewGuid(),
            new Dictionary<string, object>(),
            new NotifyReference { CaseId = Guid.NewGuid(), PartyId = Guid.NewGuid(), Notification = s_notification });

        private static readonly NotifyData s_letterData = new(
            NotifyMethods.Letter,
            string.Empty,
            Guid.NewGuid(),
            new Dictionary<string, object>(),
            new NotifyReference { CaseId = Guid.NewGuid(), PartyId = Guid.NewGuid(), Notification = s_notification });

        [SetUp]
        public void SetUp()
        {
            _notifyClientMock = new Mock<INotifyClient>(MockBehavior.Strict);
            _factoryMock = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            _serializerMock = new Mock<ISerializationService>(MockBehavior.Strict);

            _factoryMock
                .Setup(f => f.GetHttpClient(It.IsAny<string>()))
                .Returns(_notifyClientMock.Object);

            _serializerMock
                .Setup(s => s.Serialize(It.IsAny<NotifyReference>()))
                .Returns("{}");

            NotifyService concrete = new(_factoryMock.Object, _serializerMock.Object);
            _service = concrete;

            // Reset the static cached client between tests
            ((IDisposable)concrete).Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            ((IDisposable)_service).Dispose();
        }

        #region SendEmailAsync
        [Test]
        public async Task SendEmailAsync_WhenClientSucceeds_ReturnsSuccessAsync()
        {
            _notifyClientMock
                .Setup(c => c.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            NotifySendResponse result = await _service.SendEmailAsync(s_emailData);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task SendEmailAsync_WhenClientFails_ReturnsFailureAsync()
        {
            _notifyClientMock
                .Setup(c => c.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("API error"));

            NotifySendResponse result = await _service.SendEmailAsync(s_emailData);

            Assert.That(result.IsFailure, Is.True);
        }
        #endregion

        #region SendSmsAsync
        [Test]
        public async Task SendSmsAsync_WithLeadingZero_ConvertsToNlCountryCodeAsync()
        {
            string? capturedNumber = null;

            _notifyClientMock
                .Setup(c => c.SendSmsAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .Callback<string, string, Dictionary<string, object>, string>((num, _, _, _) => capturedNumber = num)
                .ReturnsAsync(NotifySendResponse.Success());

            await _service.SendSmsAsync(s_smsData);

            Assert.That(capturedNumber, Does.StartWith("+31"));
        }

        [Test]
        public async Task SendSmsAsync_WhenClientSucceeds_ReturnsSuccessAsync()
        {
            _notifyClientMock
                .Setup(c => c.SendSmsAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            NotifySendResponse result = await _service.SendSmsAsync(s_smsData);

            Assert.That(result.IsSuccess, Is.True);
        }
        #endregion

        #region SendLetterAsync
        [Test]
        public async Task SendLetterAsync_WhenClientSucceeds_ReturnsSuccessAsync()
        {
            _notifyClientMock
                .Setup(c => c.SendLetterAsync(
                    It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            NotifySendResponse result = await _service.SendLetterAsync(s_letterData);

            Assert.That(result.IsSuccess, Is.True);
        }
        #endregion

        #region GenerateTemplatePreviewAsync
        [Test]
        public async Task GenerateTemplatePreviewAsync_WhenClientSucceeds_ReturnsSuccessAsync()
        {
            _notifyClientMock
                .Setup(c => c.GenerateTemplatePreviewAsync(
                    It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(NotifyTemplateResponse.Success("Subject", "Body"));

            NotifyTemplateResponse result = await _service.GenerateTemplatePreviewAsync(s_emailData);

            Assert.That(result.IsSuccess, Is.True);
        }
        #endregion

        #region GetNotificationDataAsync
        [Test]
        public async Task GetNotificationDataAsync_WhenClientFails_ReturnsFailureAsync()
        {
            Guid notificationId = Guid.NewGuid();

            _notifyClientMock
                .Setup(c => c.GetNotificationDataAsync(notificationId))
                .ReturnsAsync(NotificationData.Failure("not found"));

            NotificationData result = await _service.GetNotificationDataAsync(s_emailData, notificationId);

            Assert.That(result.IsSuccess, Is.False);
        }
        #endregion
    }
}
