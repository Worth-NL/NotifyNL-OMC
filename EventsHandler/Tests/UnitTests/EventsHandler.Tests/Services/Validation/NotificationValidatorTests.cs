// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Results.Builder.Interface;
using EventsHandler.Behaviors.Responding.Results.Enums;
using EventsHandler.Services.Validation;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Moq;

namespace EventsHandler.UnitTests.Services.Validation
{
    [TestFixture]
    public sealed class NotificationValidatorTests
    {
        private IValidationService<NotificationEvent>? _validator;

        [SetUp]
        public void InitializeTests()
        {
            var mockedBuilder = new Mock<IDetailsBuilder>(MockBehavior.Strict);
            mockedBuilder.Setup(mock => mock.Get<InfoDetails>(
                    It.IsAny<Reasons>(), It.IsAny<string>()))
                .Returns(new InfoDetails());
            
            this._validator = new NotificationValidator(mockedBuilder.Object);
        }

        [Test]
        public void Validate_ForNotificationEvent_Invalid_WithoutSomeProperties_ReturnsErrorStatus()
        {
            // Arrange
            var testModel = new NotificationEvent();

            // Act
            HealthCheck actualResult = this._validator!.Validate(ref testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(HealthCheck.ERROR_Invalid));
        }

        [Test]
        public void Validate_ForNotificationEvent_Invalid_WithAdditionalProperties_InNotificationEvent_ReturnsInvalidStatus()
        {
            // Arrange
            NotificationEvent testModel = NotificationEventHandler.GetNotification_Real_TheHague();

            testModel.Orphans = new Dictionary<string, object>
            {
                { "Orphan1", 10 },
                { "Orphan2", false }
            };

            // Act
            HealthCheck actualResult = this._validator!.Validate(ref testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(HealthCheck.ERROR_Invalid));
        }

        [Test]
        public void Validate_ForNotificationEvent_Invalid_WithAdditionalProperties_InEventAttributes_ReturnsInconsistentStatus()
        {
            // Arrange
            NotificationEvent testModel = NotificationEventHandler.GetNotification_Real_TheHague();

            EventAttributes testAttributes = testModel.Attributes;
            testAttributes.Orphans = new Dictionary<string, object>
            {
                { "Orphan1", 10 },
                { "Orphan2", false }
            };
            testModel.Attributes = testAttributes;

            // Act
            HealthCheck actualResult = this._validator!.Validate(ref testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(HealthCheck.OK_Inconsistent));
        }

        [Test]
        public void Validate_ForNotificationEvent_Valid_ReturnsOkStatus()
        {
            // Arrange
            NotificationEvent testModel = NotificationEventHandler.GetNotification_Real_TheHague();

            // Act
            HealthCheck actualResult = this._validator!.Validate(ref testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(HealthCheck.OK_Valid));
        }
    }
}
