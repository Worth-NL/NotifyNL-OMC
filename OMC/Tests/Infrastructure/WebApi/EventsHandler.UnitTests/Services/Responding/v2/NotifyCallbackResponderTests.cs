// © 2026, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.v2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.MOBB.Interfaces;
using WebQueries.Print.Interfaces;
using WebQueries.MOBB.Models;
using WebQueries.Register.Interfaces;
using WebQueries.Tracing;
using ZgwModels.Enums;
using ZgwModels.Mapping.Enums.NotifyNL;
using ZgwModels.Mapping.Models.POCOs.NotifyNL;
using ZgwModels.Serialization.Interfaces;

namespace EventsHandler.Tests.Unit.Services.Responding.v2
{
    [TestFixture]
    public sealed class NotifyCallbackResponderTests
    {
        private Mock<ISerializationService> _mockedSerializer = null!;
        private Mock<ITelemetryService> _mockedTelemetry = null!;
        private Mock<INotifyService<NotifyData>> _mockedNotifyService = null!;
        private Mock<IMessageBoxScenario> _mockedMessageBoxScenario = null!;
        private Mock<IPrintScenario> _mockedPrintScenario = null!;
        private TraceEmitter _traceEmitter = null!;

        private GeneralResponder _responder = null!;

        [SetUp]
        public void InitializeTests()
        {
            this._mockedSerializer = new Mock<ISerializationService>(MockBehavior.Strict);
            this._mockedTelemetry = new Mock<ITelemetryService>(MockBehavior.Strict);
            this._mockedNotifyService = new Mock<INotifyService<NotifyData>>(MockBehavior.Strict);
            this._mockedMessageBoxScenario = new Mock<IMessageBoxScenario>(MockBehavior.Strict);
            this._mockedPrintScenario = new Mock<IPrintScenario>(MockBehavior.Strict);

            this._mockedNotifyService
                .Setup(mock => mock.GetNotificationDataAsync(It.IsAny<NotifyData>(), It.IsAny<Guid>()))
                .ReturnsAsync(NotificationData.Failure("not needed for this test"));

            // NOTE: "UxMessages" (used by DetermineUserMessageSubject/Body for the generic UX fallback
            // text) is only ever configured via appsettings.json, never via environment variables - so
            // this needs a preset with a real (valid) appsettings.json, unlike MessageBoxScenarioImplementation's
            // tests which only touch ZGW/Notify/BRP settings and work fine with ValidEnvironment_v2.
            OmcConfiguration configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.BothValid_v2);

            this._traceEmitter = new TraceEmitter();

            this._responder = new NotifyCallbackResponder(
                configuration,
                this._mockedSerializer.Object,
                this._mockedTelemetry.Object,
                this._mockedNotifyService.Object,
                this._mockedMessageBoxScenario.Object,
                this._mockedPrintScenario.Object,
                this._traceEmitter);
        }

        #region Test data
        private static readonly Guid s_messageId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid s_partyId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        /// <summary>
        /// A real GZip-compressed reference containing a "MessageId" property, so that the (non-mockable)
        /// raw JSON peek in <see cref="GeneralResponder.IsMessageBoxCallbackAsync"/> recognizes it as a
        /// MOBB / Berichtenbox reference. The actual deserialized shape is controlled separately via the
        /// mocked <see cref="ISerializationService.Deserialize{TModel}"/>.
        /// </summary>
        private static readonly string s_compressedMessageBoxReference =
            $"{{\"MessageId\":\"{s_messageId}\"}}".CompressGZipAsync(CancellationToken.None).GetAwaiter().GetResult();

        /// <summary>
        /// A real GZip-compressed reference WITHOUT a "MessageId" property, representing the standard
        /// (non-MOBB) <see cref="NotifyReference"/> shape - used to confirm the original callback path is
        /// still taken when a callback isn't for the MOBB / Berichtenbox scenario.
        /// </summary>
        private static readonly string s_compressedStandardReference =
            "{}".CompressGZipAsync(CancellationToken.None).GetAwaiter().GetResult();
        #endregion

        private static DeliveryReceipt BuildDeliveryReceipt(string reference, DeliveryStatuses status, NotificationTypes type)
        {
            return new DeliveryReceipt
            {
                Id = Guid.NewGuid(),
                Reference = reference,
                Recipient = "citizen@example.com",
                Status = status,
                Type = type
            };
        }

        [TestCase(NotificationTypes.Mobb, NotifyMethods.Mobb)]
        [TestCase(NotificationTypes.Email, NotifyMethods.Email)]
        public async Task HandleNotifyCallbackAsync_MessageBoxCallback_Failure_InvokesFallback_WithReportedChannel(
            NotificationTypes callbackType, NotifyMethods expectedChannel)
        {
            // Arrange
            var reference = new MessageBoxNotifyReference { MessageId = s_messageId, PartyId = s_partyId };

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<DeliveryReceipt>(It.IsAny<object>()))
                .Returns(BuildDeliveryReceipt(s_compressedMessageBoxReference, DeliveryStatuses.PermanentFailure, callbackType));

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<MessageBoxNotifyReference>(It.IsAny<object>()))
                .Returns(reference);

            this._mockedTelemetry
                .Setup(mock => mock.ReportMessageBoxCompletionAsync(
                    It.IsAny<MessageBoxNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Success("contactmoment created"));

            this._mockedMessageBoxScenario
                .Setup(mock => mock.HandleDeliveryFailureAsync(It.IsAny<MessageBoxNotifyReference>(), expectedChannel))
                .ReturnsAsync(HttpRequestResponse.Success("fallback attempted"));

            // Act
            IActionResult result = await this._responder.HandleNotifyCallbackAsync(new object());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<ObjectResult>());

                this._mockedMessageBoxScenario.Verify(mock => mock.HandleDeliveryFailureAsync(
                    It.IsAny<MessageBoxNotifyReference>(), expectedChannel), Times.Once);
            });
        }

        [Test]
        public async Task HandleNotifyCallbackAsync_MessageBoxCallback_Success_DoesNotInvokeFallback()
        {
            // Arrange
            var reference = new MessageBoxNotifyReference { MessageId = s_messageId, PartyId = s_partyId };

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<DeliveryReceipt>(It.IsAny<object>()))
                .Returns(BuildDeliveryReceipt(s_compressedMessageBoxReference, DeliveryStatuses.Delivered, NotificationTypes.Mobb));

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<MessageBoxNotifyReference>(It.IsAny<object>()))
                .Returns(reference);

            this._mockedTelemetry
                .Setup(mock => mock.ReportMessageBoxCompletionAsync(
                    It.IsAny<MessageBoxNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Success("contactmoment created"));

            // Act
            await this._responder.HandleNotifyCallbackAsync(new object());

            // Assert
            this._mockedMessageBoxScenario.Verify(mock => mock.HandleDeliveryFailureAsync(
                It.IsAny<MessageBoxNotifyReference>(), It.IsAny<NotifyMethods>()), Times.Never);
        }

        [Test]
        public async Task HandleNotifyCallbackAsync_MessageBoxCallback_FallbackThrows_DoesNotPropagate_StillReturnsTelemetryResult()
        {
            // Arrange
            var reference = new MessageBoxNotifyReference { MessageId = s_messageId, PartyId = s_partyId };

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<DeliveryReceipt>(It.IsAny<object>()))
                .Returns(BuildDeliveryReceipt(s_compressedMessageBoxReference, DeliveryStatuses.TechnicalFailure, NotificationTypes.Mobb));

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<MessageBoxNotifyReference>(It.IsAny<object>()))
                .Returns(reference);

            this._mockedTelemetry
                .Setup(mock => mock.ReportMessageBoxCompletionAsync(
                    It.IsAny<MessageBoxNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Success("contactmoment created"));

            this._mockedMessageBoxScenario
                .Setup(mock => mock.HandleDeliveryFailureAsync(It.IsAny<MessageBoxNotifyReference>(), It.IsAny<NotifyMethods>()))
                .ThrowsAsync(new InvalidOperationException("simulated fallback crash"));

            // Act
            IActionResult result = await this._responder.HandleNotifyCallbackAsync(new object());

            // Assert - the callback's own HTTP response must still reflect the (successful) contactmoment registration
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<ObjectResult>());

                var objectResult = (ObjectResult)result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            });
        }

        [Test]
        public async Task HandleNotifyCallbackAsync_NonMessageBoxCallback_UsesOriginalPath_NeverTouchesMessageBoxScenario()
        {
            // Arrange - a "standard" reference has no "MessageId" property, so IsMessageBoxCallbackAsync's raw peek returns false
            this._mockedSerializer
                .Setup(mock => mock.Deserialize<DeliveryReceipt>(It.IsAny<object>()))
                .Returns(BuildDeliveryReceipt(s_compressedStandardReference, DeliveryStatuses.Delivered, NotificationTypes.Email));

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<NotifyReference>(It.IsAny<object>()))
                .Returns(new NotifyReference());

            this._mockedTelemetry
                .Setup(mock => mock.ReportCompletionAsync(
                    It.IsAny<NotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Success("contactmoment created"));

            // Act
            await this._responder.HandleNotifyCallbackAsync(new object());

            // Assert
            Assert.Multiple(() =>
            {
                this._mockedMessageBoxScenario.Verify(mock => mock.HandleDeliveryFailureAsync(
                    It.IsAny<MessageBoxNotifyReference>(), It.IsAny<NotifyMethods>()), Times.Never);
                this._mockedTelemetry.Verify(mock => mock.ReportCompletionAsync(
                    It.IsAny<NotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
            });
        }

        [TestCase(true, "ok")]
        [TestCase(false, "fail")]
        public async Task HandleNotifyCallbackAsync_NonMessageBoxCallback_TracesContactMomentStage_ByItsOwnRegistrationOutcome_NotByNotifyFeedback(
            bool contactMomentWriteSucceeds, string expectedContactMomentStatus)
        {
            // Arrange - Notify NL reports a successful delivery either way; only the contactmoment
            // write's own outcome should decide the "contactmoment" trace stage's status, so a failed
            // write must still trace "fail" even though the send itself was "Delivered".
            const string traceId = "trace-under-test";
            (Guid _, System.Threading.Channels.ChannelReader<TraceEvent> reader) = this._traceEmitter.Subscribe();

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<DeliveryReceipt>(It.IsAny<object>()))
                .Returns(BuildDeliveryReceipt(s_compressedStandardReference, DeliveryStatuses.Delivered, NotificationTypes.Email));

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<NotifyReference>(It.IsAny<object>()))
                .Returns(new NotifyReference { TraceId = traceId });

            this._mockedTelemetry
                .Setup(mock => mock.ReportCompletionAsync(
                    It.IsAny<NotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(contactMomentWriteSucceeds
                    ? HttpRequestResponse.Success("contactmoment created")
                    : HttpRequestResponse.Failure("contactmoment API rejected the write"));

            // Act
            await this._responder.HandleNotifyCallbackAsync(new object());

            // Assert - drain every event this callback emitted and find the "contactmoment" one
            var emitted = new List<TraceEvent>();
            while (reader.TryRead(out TraceEvent? traceEvent))
            {
                emitted.Add(traceEvent);
            }

            TraceEvent? contactMomentEvent = emitted.SingleOrDefault(e => e.Stage == "contactmoment");
            Assert.That(contactMomentEvent, Is.Not.Null, "Expected a 'contactmoment' trace event to be emitted.");
            Assert.That(contactMomentEvent!.Status, Is.EqualTo(expectedContactMomentStatus));

            // The channel-send stage always reflects Notify's own feedback ("Delivered" => "ok"),
            // independent of whether the contactmoment write succeeded.
            TraceEvent? channelEvent = emitted.SingleOrDefault(e => e.Stage == "notify-email");
            Assert.That(channelEvent, Is.Not.Null, "Expected a 'notify-email' trace event to be emitted.");
            Assert.That(channelEvent!.Status, Is.EqualTo("ok"));
        }
    }
}
