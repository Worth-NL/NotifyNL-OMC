// © 2024, Worth Systems.

using Common.Models.Messages.Details;
using EventsHandler.Services.Responding.Enums;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using EventsHandler.Services.Validation;
using EventsHandler.Services.Validation.Interfaces;
using Moq;
using ZgwModels.Enums;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Tests.Unit._TestHelpers;

namespace EventsHandler.Tests.Unit.Services.Validation
{
    [TestFixture]
    public sealed class NotificationValidatorTests
    {
        private IValidationService<NotificationEvent>? _validator;

        private static readonly NotificationEvent s_testNotification =
            NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague()
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
        public void Validate_ForProductNotification_ReturnsInconsistentStatus_WithoutThrowing()
        {
            // NOTE: EventAttributes.Properties indexes a dictionary that has an entry per channel, and
            //       indexing it with a channel that was never added throws KeyNotFoundException here,
            //       before the resolver ever sees the notification. This is the regression guard for that.
            //
            //       Inconsistent rather than valid is the expected outcome: "Open Product" publishes its
            //       product type as kenmerken that EventAttributes deliberately does not map, so they land
            //       in its extension data. NotifyProcessor only rejects ERROR_Invalid, so the notification
            //       still goes on to be processed.

            // Arrange
            NotificationEvent testModel = NotificationEventHandler.GetNotification_Real_ProductCreatedScenario()
                .Deserialized();

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
