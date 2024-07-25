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

        private static readonly NotificationEvent s_testNotification =
            NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague()
                .Deserialized();

        [SetUp]
        public void InitializeTests()
        {
            var mockedBuilder = new Mock<IDetailsBuilder>(MockBehavior.Strict);
            mockedBuilder.Setup(mock => mock.Get<InfoDetails>(
                    It.IsAny<Reasons>(), It.IsAny<string>()))
                .Returns(new InfoDetails());

            mockedBuilder.Setup(mock => mock.Get<ErrorDetails>(
                    It.IsAny<Reasons>(), It.IsAny<string>()))
                .Returns(new ErrorDetails());

            this._validator = new NotificationValidator(mockedBuilder.Object);
        }

        [Test]
        public void Validate_ForNotification_Invalid_NullProperties_ReturnsErrorStatus()
        {
            // Arrange
            var testModel = new NotificationEvent();

            // Act
            HealthCheck actualResult = this._validator!.Validate(ref testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(HealthCheck.ERROR_Invalid));
        }

        [Test]
        public void Validate_ForNotification_Invalid_EmptyProperties_ReturnsErrorStatus()
        {
            // Arrange
            NotificationEvent testModel = NotificationEventHandler.GetNotification_Test_AllAttributes_Null_WithoutOrphans()
                .Deserialized();

            // Act
            HealthCheck actualResult = this._validator!.Validate(ref testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(HealthCheck.ERROR_Invalid));
        }

        [Test]
        public void Validate_ForNotification_Invalid_AdditionalProperties_ReturnsInvalidStatus()
        {
            // Arrange
            NotificationEvent testModel = s_testNotification;

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
        public void Validate_ForAttributes_Invalid_AdditionalProperties_ReturnsInconsistentStatus()
        {
            // Arrange
            NotificationEvent testModel = s_testNotification;

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
        public void Validate_ForNotification_Valid_ReturnsOkStatus()
        {
            // Arrange
            NotificationEvent testModel = s_testNotification;

            // Act
            HealthCheck actualResult = this._validator!.Validate(ref testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(HealthCheck.OK_Valid));
        }
    }
}
