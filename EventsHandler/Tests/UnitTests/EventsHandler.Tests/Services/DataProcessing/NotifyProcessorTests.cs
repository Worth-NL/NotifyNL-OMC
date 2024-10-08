﻿// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Utilities._TestHelpers;
using Moq;
using ResourcesText = EventsHandler.Properties.Resources;

namespace EventsHandler.UnitTests.Services.DataProcessing
{
    [TestFixture]
    public sealed class NotifyProcessorTests
    {
        private Mock<IScenariosResolver<INotifyScenario, NotificationEvent>> _mockedScenariosResolver = null!;
        private IProcessingService<NotificationEvent> _processor = null!;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._mockedScenariosResolver = new Mock<IScenariosResolver<INotifyScenario, NotificationEvent>>(MockBehavior.Strict);
            this._processor = new NotifyProcessor(this._mockedScenariosResolver.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            this._mockedScenariosResolver.Reset();
        }

        #region Test data
        private static readonly NotificationEvent s_validNotification =
            NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague().Deserialized();
        #endregion

        #region ProcessAsync()
        [Test]
        public async Task ProcessAsync_TestNotification_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            var testUri = new Uri("http://some.hoofdobject.nl/");

            var testNotification = new NotificationEvent
            {
                Channel = Channels.Unknown,
                Resource = Resources.Unknown,
                MainObjectUri = testUri,
                ResourceUri = testUri
            };

            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(testNotification);

            // Assert
            VerifyMethodsCalls(0);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Skipped));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Notification_Test));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_UnknownScenario_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            this._mockedScenariosResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<NotImplementedException>();

            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Skipped));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_NotImplemented));
            });
        }

        [Test]
        public async Task ProcessAsync_InternalErrors_WhenResolvingScenario_ReturnsProcessingResult_Failure()
        {
            // Arrange
            this._mockedScenariosResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<HttpRequestException>();

            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Does.StartWith($"{nameof(HttpRequestException)} | Exception of type '{typeof(HttpRequestException).FullName}' was"));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidScenario_FailedGetDataResponse_ReturnsProcessingResult_Failure()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Failure());
            
            this._mockedScenariosResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));

                string expectedMessage =
                    ResourcesText.Processing_ERROR_Scenario_NotificationNotSent.Replace("{0}",
                        ResourcesText.Processing_ERROR_Scenario_NotificationMethod);

                Assert.That(message, Is.EqualTo(expectedMessage));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidScenario_SuccessGetDataResponse_FailedProcessDataResponse_ReturnsProcessingResult_Failure()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Success(new[]
                {
                    new NotifyData(NotifyMethods.Email)
                }));

            const string processingErrorText = "HTTP Bad Request";
            mockedNotifyScenario
                .Setup(mock => mock.ProcessDataAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<IReadOnlyCollection<NotifyData>>()))
                .ReturnsAsync(ProcessingDataResponse.Failure(processingErrorText));
            
            this._mockedScenariosResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));

                string expectedMessage =
                    ResourcesText.Processing_ERROR_Scenario_NotificationNotSent.Replace("{0}", processingErrorText);

                Assert.That(message, Is.EqualTo(expectedMessage));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidScenario_SuccessGetDataResponse_SuccessProcessDataResponse_ReturnsProcessingResult_Success()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Success(new[]
                {
                    new NotifyData(NotifyMethods.Email)
                }));

            mockedNotifyScenario
                .Setup(mock => mock.ProcessDataAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<IReadOnlyCollection<NotifyData>>()))
                .ReturnsAsync(ProcessingDataResponse.Success);
            
            this._mockedScenariosResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Success));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_SUCCESS_Scenario_NotificationSent));
            });
        }
        #endregion

        #region Verify
        private void VerifyMethodsCalls(int determineScenarioInvokeCount)
        {
            this._mockedScenariosResolver
                .Verify(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()),
                Times.Exactly(determineScenarioInvokeCount));
        }
        #endregion
    }
}
