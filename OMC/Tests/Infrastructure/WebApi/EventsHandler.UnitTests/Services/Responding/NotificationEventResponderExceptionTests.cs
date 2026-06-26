// © 2024, Worth Systems.

using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Enums;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace EventsHandler.Tests.Unit.Services.Responding
{
    [TestFixture]
    public sealed class NotificationEventResponderExceptionTests
    {
        private Mock<IDetailsBuilder> _builderMock = null!;
        private IRespondingService _responder = null!;

        private static readonly ErrorDetails s_errorDetails = new("Error", "cases", ["reason"]);
        private static readonly UnknownDetails s_unknownDetails = new();

        [SetUp]
        public void SetUp()
        {
            _builderMock = new Mock<IDetailsBuilder>(MockBehavior.Loose);

            _builderMock
                .Setup(b => b.Get<ErrorDetails>(It.IsAny<Reasons>(), It.IsAny<string>()))
                .Returns(s_errorDetails);

            _builderMock
                .Setup(b => b.Get<UnknownDetails>(It.IsAny<Reasons>()))
                .Returns(s_unknownDetails);

            _responder = new NotificationEventResponder(_builderMock.Object);
        }

        #region GetExceptionResponse(Exception)
        [Test]
        public void GetExceptionResponse_HttpRequestException_Returns400()
        {
            ObjectResult result = _responder.GetExceptionResponse(new HttpRequestException("HTTP Request failed"));

            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetExceptionResponse_GenericException_FallsThrough_Returns422()
        {
            ObjectResult result = _responder.GetExceptionResponse(new Exception("Some JSON deserialization error at property X"));

            Assert.That(result.StatusCode, Is.EqualTo(422));
        }

        [Test]
        public void GetExceptionResponse_GenericException_InvalidMessage_Returns422()
        {
            ObjectResult result = _responder.GetExceptionResponse(new Exception("The JSON value could not be converted"));

            Assert.That(result.StatusCode, Is.EqualTo(422));
        }
        #endregion

        #region GetExceptionResponse(string)
        [Test]
        public void GetExceptionResponse_String_DeserializationMissingProperty_Returns422()
        {
            ObjectResult result = _responder.GetExceptionResponse("JSON deserialization failed: property 'kanaal' is missing");

            Assert.That(result.StatusCode, Is.EqualTo(422));
        }

        [Test]
        public void GetExceptionResponse_String_DeserializationInvalidValue_Returns422()
        {
            ObjectResult result = _responder.GetExceptionResponse("The JSON value 'foo' could not be converted to Boolean");

            Assert.That(result.StatusCode, Is.EqualTo(422));
        }

        [Test]
        public void GetExceptionResponse_String_InvalidJsonStructure_Returns422()
        {
            ObjectResult result = _responder.GetExceptionResponse("Unexpected end of JSON input");

            Assert.That(result.StatusCode, Is.EqualTo(422));
        }
        #endregion

        #region ContainsErrorMessage
        [Test]
        public void ContainsErrorMessage_DollarKey_ReturnsTrue()
        {
            var errors = new Dictionary<string, string[]> { { "$", ["Error from root"] } };

            bool result = _responder.ContainsErrorMessage(errors, out string message);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(message, Is.EqualTo("Error from root"));
            });
        }

        [Test]
        public void ContainsErrorMessage_KanaalKey_ReturnsTrue()
        {
            var errors = new Dictionary<string, string[]> { { "$.kanaal", ["Invalid kanaal value"] } };

            bool result = _responder.ContainsErrorMessage(errors, out string message);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(message, Is.EqualTo("Invalid kanaal value"));
            });
        }

        [Test]
        public void ContainsErrorMessage_KenmerkenKey_ReturnsTrue()
        {
            var errors = new Dictionary<string, string[]> { { "$.kenmerken", ["Invalid kenmerken value"] } };

            bool result = _responder.ContainsErrorMessage(errors, out string message);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(message, Is.EqualTo("Invalid kenmerken value"));
            });
        }

        [Test]
        public void ContainsErrorMessage_DynamicDollarPrefixKey_ReturnsTrue()
        {
            var errors = new Dictionary<string, string[]> { { "$.someOtherField", ["Field error"] } };

            bool result = _responder.ContainsErrorMessage(errors, out string message);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(message, Is.EqualTo("Field error"));
            });
        }

        [Test]
        public void ContainsErrorMessage_EmptyDict_ReturnsFalse()
        {
            bool result = _responder.ContainsErrorMessage(new Dictionary<string, string[]>(), out string message);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(message, Is.Not.Empty);
            });
        }

        [Test]
        public void ContainsErrorMessage_UnknownKey_ReturnsFalse()
        {
            var errors = new Dictionary<string, string[]> { { "someKey", ["Some error"] } };

            bool result = _responder.ContainsErrorMessage(errors, out string message);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(message, Is.Not.Empty);
            });
        }
        #endregion
    }
}
