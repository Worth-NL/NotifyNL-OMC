// © 2024, Worth Systems.

using Common.Enums.Responses;
using Common.Models.Responses;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Notify.Exceptions;
using NUnit.Framework;
using ZgwModels.Mapping.Enums.NotifyNL;
using ZgwModels.Mapping.Models.POCOs.NotifyNL;
using ZgwModels.Serialization.Interfaces;

namespace EventsHandler.Tests.Unit.Services.Responding
{
    [TestFixture]
    public sealed class GeneralResponderTests
    {
        // Minimal concrete implementation to expose abstract class
        private sealed class TestResponder : GeneralResponder
        {
            public TestResponder(ISerializationService serializer) : base(serializer) { }

            internal override Task<IActionResult> HandleNotifyCallbackAsync(object json)
                => throw new NotImplementedException();
        }

        private IRespondingService _responder = null!;
        private IRespondingService<ProcessingResult> _typedResponder = null!;

        [SetUp]
        public void SetUp()
        {
            ISerializationService serializer = new Mock<ISerializationService>(MockBehavior.Loose).Object;
            TestResponder r = new(serializer);
            _responder = r;
            _typedResponder = r;
        }

        #region GetExceptionResponse(Exception)
        [Test]
        public void GetExceptionResponse_NotifyClientException_InvalidApiKey_Returns403()
        {
            NotifyClientException ex = new("Invalid token: service not found", 403);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_InvalidEmail_Returns400()
        {
            NotifyClientException ex = new("Not a valid email address", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_MissingPhoneNumber_Returns400()
        {
            NotifyClientException ex = new("Number field is required", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_PhoneSymbols_Returns400()
        {
            NotifyClientException ex = new("Must not contain letters or symbols", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_PhoneTooShort_Returns400()
        {
            NotifyClientException ex = new("Not enough digits", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_PhoneTooLong_Returns400()
        {
            NotifyClientException ex = new("Too many digits", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_PhoneFormat_Returns400()
        {
            NotifyClientException ex = new("Please enter mobile number according to the expected format", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_TemplateNotFound_Returns400()
        {
            NotifyClientException ex = new("Template not found", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_MissingPersonalization_Returns400()
        {
            NotifyClientException ex = new("Missing personalisation: name, address", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyClientException_UnknownMessage_Returns400()
        {
            NotifyClientException ex = new("Some unexpected error from Notify", 400);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_NotifyAuthException_Returns403()
        {
            NotifyAuthException ex = new("Access denied");

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public void GetExceptionResponse_GenericException_Returns500()
        {
            Exception ex = new("Something went wrong");

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public void GetExceptionResponse_GenericException_WithInnerException_Returns500()
        {
            Exception inner = new("Inner cause");
            Exception ex = new("Outer message", inner);

            ObjectResult result = _responder.GetExceptionResponse(ex);

            Assert.That(result.StatusCode, Is.EqualTo(500));
        }
        #endregion

        #region GetExceptionResponse(string)
        [Test]
        public void GetExceptionResponse_String_MissingEmailAddress_Returns400()
        {
            ObjectResult result = _responder.GetExceptionResponse("Address field is required");

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_String_MissingPhoneNumber_Returns400()
        {
            ObjectResult result = _responder.GetExceptionResponse("Number field is required");

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_String_GenericError_Returns400()
        {
            ObjectResult result = _responder.GetExceptionResponse("Something failed");

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }
        #endregion

        #region ContainsErrorMessage
        [Test]
        public void ContainsErrorMessage_NonEmptyDict_WithValues_ReturnsTrue()
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Field", new[] { "Error message" } }
            };

            bool result = _responder.ContainsErrorMessage(errors, out string errorMessage);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(errorMessage, Is.EqualTo("Error message"));
            });
        }

        [Test]
        public void ContainsErrorMessage_EmptyDict_ReturnsFalse()
        {
            bool result = _responder.ContainsErrorMessage(new Dictionary<string, string[]>(), out string errorMessage);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(errorMessage, Is.Not.Empty);
            });
        }

        [Test]
        public void ContainsErrorMessage_DictWithEmptyValues_ReturnsFalse()
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Field", Array.Empty<string>() }
            };

            bool result = _responder.ContainsErrorMessage(errors, out string errorMessage);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(errorMessage, Is.Not.Empty);
            });
        }
        #endregion

        #region GetResponse(ProcessingResult)
        [Test]
        public void GetResponse_Success_Returns202()
        {
            ObjectResult result = _typedResponder.GetResponse(ProcessingResult.Success("OK"));

            Assert.That(result.StatusCode, Is.EqualTo(202));
        }

        [Test]
        public void GetResponse_Failure_Returns400()
        {
            ObjectResult result = _typedResponder.GetResponse(ProcessingResult.Failure("Failed"));

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetResponse_Aborted_Returns501()
        {
            ObjectResult result = _typedResponder.GetResponse(ProcessingResult.Aborted("Aborted"));

            Assert.That(result.StatusCode, Is.EqualTo(501));
        }
        #endregion

        #region Static helper methods
        [Test]
        public void GetDeliveryStatusLogMessage_ReturnsNonEmptyString()
        {
            DeliveryReceipt callback = new() { Id = Guid.NewGuid(), Status = DeliveryStatuses.Delivered };

            string result = GeneralResponder.GetDeliveryStatusLogMessage(callback);

            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void GetDeliveryErrorLogMessage_ReturnsNonEmptyString()
        {
            DeliveryReceipt callback = new() { Id = Guid.NewGuid(), Status = DeliveryStatuses.PermanentFailure };
            Exception ex = new("Delivery error");

            string result = GeneralResponder.GetDeliveryErrorLogMessage(callback, ex);

            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void GetDeliveryStatusLogMessage_ContainsCallbackId()
        {
            Guid id = Guid.NewGuid();
            DeliveryReceipt callback = new() { Id = id, Status = DeliveryStatuses.Delivered };

            string result = GeneralResponder.GetDeliveryStatusLogMessage(callback);

            Assert.That(result, Does.Contain(id.ToString()));
        }
        #endregion
    }
}
