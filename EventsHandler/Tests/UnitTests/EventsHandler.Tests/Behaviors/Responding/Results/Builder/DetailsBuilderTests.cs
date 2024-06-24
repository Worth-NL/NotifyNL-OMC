// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Results.Builder;
using EventsHandler.Behaviors.Responding.Results.Builder.Interface;
using EventsHandler.Behaviors.Responding.Results.Enums;
using System.Text.Json;

namespace EventsHandler.UnitTests.Behaviors.Responding.Results.Builder
{
    [TestFixture]
    public sealed class DetailsBuilderTests
    {
        private const string TestCase = "TEST: Example text %$#!";
        
        private IDetailsBuilder? _detailsBuilder;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._detailsBuilder = new DetailsBuilder();
        }

        [Test]
        public void DetailsBuilder_ForInvalidJson_ReturnsErrorDetails_WithExpectedStrings()
        {
            // Act
            ErrorDetails actualDetails = this._detailsBuilder!.Get<ErrorDetails>(Reasons.InvalidJson, TestCase);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails);
            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"The JSON payload is invalid.\"," +
                  $"\"Cases\":\"{TestCase}\"," +
                  $"\"Reasons\":" +
                    $"[" +
                      $"\"SENDER: The input cannot be recognized as JSON format.\"" +
                    $"]" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }

        [Test]
        public void DetailsBuilder_ForMissingProperties_Notification_ReturnsErrorDetails_WithExpectedStrings()
        {
            // Act
            ErrorDetails actualDetails = this._detailsBuilder!.Get<ErrorDetails>(Reasons.MissingProperties_Notification, TestCase);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails);
            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"Required properties are missing in the given JSON payload.\"," +
                  $"\"Cases\":\"{TestCase}\"," +
                  $"\"Reasons\":" +
                    $"[" +
                      $"\"SENDER: In the received notification some [Required] properties are either null, empty, or with default values.\"," +
                      $"\"SENDER: The standard JSON schema was recently changed and some mandatory properties were removed from it.\"," +
                      $"\"RECEIVER: In the POCO model new [Required] properties were added, causing a mismatch with the standard JSON schema.\"" +
                    $"]" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }

        [Test]
        public void DetailsBuilder_ForInvalidProperties_Notification_ReturnsErrorDetails_WithExpectedStrings()
        {
            // Act
            ErrorDetails actualDetails = this._detailsBuilder!.Get<ErrorDetails>(Reasons.InvalidProperties_Notification, TestCase);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails);
            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"Received value could not be recognized (might be unexpected).\"," +
                  $"\"Cases\":\"{TestCase}\"," +
                  $"\"Reasons\":" +
                  $"[" +
                    $"\"SENDER: In the JSON schema some data (value) of property (key) has a different type, format, or is out of range than supported in the POCO model.\"," +
                    $"\"RECEIVER: In the POCO model the type, format or range of expected data was recently changed, causing a mismatch with the JSON schema.\"" +
                  $"]" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }

        [Test]
        public void DetailsBuilder_ForMissingProperties_Attributes_ReturnsInfoDetails_WithExpectedStrings()
        {
            // Act
            InfoDetails actualDetails = this._detailsBuilder!.Get<InfoDetails>(Reasons.MissingProperties_Attributes, TestCase);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails).Replace("\\u0027", @"'");
            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"Some values of properties in the nested 'attributes' ('kenmerken') in the POCO model are missing.\"," +
                  $"\"Cases\":\"{TestCase}\"," +
                  $"\"Reasons\":" +
                  $"[" +
                    $"\"SENDER: In the JSON schema some properties from the nested 'attributes' ('kenmerken') are missing, although they were previously defined in the POCO model.\"," +
                    $"\"SENDER: In the JSON schema some keys of properties in the nested 'attributes' ('kenmerken') were renamed, causing mismatch with the POCO model.\"," +
                    $"\"RECEIVER: In the POCO model the custom names of some attributes are different from the keys of properties in the nested 'attributes' ('kenmerken') in the received JSON payload.\"" +
                  $"]" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }

        [Test]
        public void DetailsBuilder_ForUnexpectedProperties_Notification_ReturnsInfoDetails_WithExpectedStrings()
        {
            // Act
            InfoDetails actualDetails = this._detailsBuilder!.Get<InfoDetails>(Reasons.UnexpectedProperties_Notification, TestCase);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails).Replace("\\u0027", @"'");
            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"The JSON payload contains more root 'notification' properties than expected by the POCO model.\"," +
                  $"\"Cases\":\"{TestCase}\"," +
                  $"\"Reasons\":" +
                  $"[" +
                    $"\"SENDER: The JSON schema was recently changed and some root 'notification' properties were added to it.\"," +
                    $"\"RECEIVER: In the POCO model some existing properties were removed from the root 'notification', causing a mismatch with the JSON schema.\"" +
                  $"]" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }

        [Test]
        public void DetailsBuilder_ForUnexpectedProperties_Attributes_ReturnsInfoDetails_WithExpectedStrings()
        {
            // Act
            InfoDetails actualDetails = this._detailsBuilder!.Get<InfoDetails>(Reasons.UnexpectedProperties_Attributes, TestCase);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails).Replace("\\u0027", @"'");
            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"The JSON payload contains more properties in the nested 'attributes' ('kenmerken') than expected by the POCO model.\"," +
                  $"\"Cases\":\"{TestCase}\"," +
                  $"\"Reasons\":" +
                  $"[" +
                    $"\"SENDER: The JSON schema was recently changed and some properties in the nested 'attributes' ('kenmerken') were added to it.\"," +
                    $"\"RECEIVER: In the POCO model some existing properties were removed from the nested 'attributes', causing a mismatch with the JSON schema.\"" +
                  $"]" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }

        [Test]
        public void DetailsBuilder_ForHttpRequestError_ReturnsErrorDetails_WithExpectedStrings()
        {
            // Act
            ErrorDetails actualDetails = this._detailsBuilder!.Get<ErrorDetails>(Reasons.HttpRequestError, TestCase);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails);
            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"The HTTP Request sent to external Web API service failed.\"," +
                  $"\"Cases\":\"{TestCase}\"," +
                  $"\"Reasons\":" +
                  $"[" +
                    $"\"RECEIVER: The requested resource is unavailable or not existing.\"," +
                    $"\"SENDER: The network configuration is improper.\"" +
                  $"]" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }

        [Test]
        public void DetailsBuilder_ForOtherValidationIssues_ReturnsUnknownDetails_WithExpectedStrings()
        {
            // Act
            UnknownDetails actualDetails = this._detailsBuilder!.Get<UnknownDetails>(Reasons.ValidationIssue);

            // Assert
            string serializedActualDetails = JsonSerializer.Serialize(actualDetails);

            TestContext.WriteLine(serializedActualDetails);

            const string serializedExpectedDetails =
                $"{{" +
                  $"\"Message\":\"An unknown validation issue occurred.\"" +
                $"}}";

            Assert.That(serializedActualDetails, Is.EqualTo(serializedExpectedDetails));
        }
    }
}