// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using System.Text.Json;

namespace EventsHandler.UnitTests.Services.Serialization
{
    [TestFixture]
    public sealed class SpecificSerializerTests
    {
        private ISerializationService? _serializer;

        #region Test data
        private const int TestNumber = 50;
        private readonly string _testJson = $"{{\"Number\":{TestNumber}}}";

        private readonly struct TestFullModel : IJsonSerializable
        {
            public int Number { get; init; }
        }

        private readonly struct TestEmptyModel : IJsonSerializable { }
        #endregion

        [SetUp]
        public void InitializeTests()
        {
            this._serializer = new SpecificSerializer();
        }

        #region Deserialize
        [Test]
        public void Deserialize_TakesValidJson_AndReturnsDeserializedModel()
        {
            // Act
            TestFullModel actualResult = this._serializer!.Deserialize<TestFullModel>(_testJson);

            // Assert
            Assert.That(actualResult.Number, Is.EqualTo(TestNumber));
        }

        [Test]
        public void Deserialize_TakesEmptyJson_ThrowsJsonException()
        {
            // Act & Assert
            Assert.Throws<JsonException>(() => this._serializer!.Deserialize<TestFullModel>(new object()));
        }
        #endregion

        #region Serialize
        [Test]
        public void Serialize_TakesValidModel_AndReturnsSerializedJson()
        {
            // Arrange
            var testModel = new TestFullModel { Number = TestNumber };

            // Act
            string actualResult = this._serializer!.Serialize(testModel);

            // Assert
            Assert.That(actualResult, Is.EqualTo(_testJson));
        }
        
        [Test]
        public void Serialize_TakesUnknownModel_DoesNotThrowException_AndReturnsDefaultJson()
        {
            // Act
            string actualResult = this._serializer!.Serialize(default(TestEmptyModel));

            // Assert
            Assert.That(actualResult, Is.EqualTo(DefaultValues.Models.EmptyJson));
        }
        #endregion
    }
}