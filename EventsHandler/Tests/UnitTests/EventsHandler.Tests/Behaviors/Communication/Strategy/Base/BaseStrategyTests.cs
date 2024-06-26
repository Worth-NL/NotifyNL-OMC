﻿// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Implementations;
using EventsHandler.Behaviors.Communication.Strategy.Implementations.Cases;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Moq;
using System.Reflection;

namespace EventsHandler.UnitTests.Behaviors.Communication.Strategy.Base
{
    [TestFixture]
    public sealed class BaseStrategyTests
    {
        private const string TestEmailAddress = "test@gmail.com";
        private const string TestPhoneNumber = "+310123456789";

        private WebApiConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._testConfiguration = ConfigurationHandler.GetValidEnvironmentConfiguration();
        }

        #region GetAllNotifyDataAsync()
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Email, 1, NotifyMethods.Email, "test@gmail.com", 2, 1, 1)]
        [TestCase(typeof(CaseCreatedScenario), DistributionChannels.Sms, 1, NotifyMethods.Sms, "+310123456789", 2, 1, 1)]
        [TestCase(typeof(CaseCaseStatusUpdatedScenario), DistributionChannels.Email, 1, NotifyMethods.Email, "test@gmail.com", 2, 1, 1)]
        [TestCase(typeof(CaseCaseStatusUpdatedScenario), DistributionChannels.Sms, 1, NotifyMethods.Sms, "+310123456789", 2, 1, 1)]
        [TestCase(typeof(CaseCaseFinishedScenario), DistributionChannels.Email, 1, NotifyMethods.Email, "test@gmail.com", 2, 1, 1)]
        [TestCase(typeof(CaseCaseFinishedScenario), DistributionChannels.Sms, 1, NotifyMethods.Sms, "+310123456789", 2, 1, 1)]
        [TestCase(typeof(DecisionMadeScenario), DistributionChannels.Email, 1, NotifyMethods.Email, "test@gmail.com", 2, 1, 1)]
        [TestCase(typeof(DecisionMadeScenario), DistributionChannels.Sms, 1, NotifyMethods.Sms, "+310123456789", 2, 1, 1)]
        public async Task GetAllNotifyDataAsync_ForValidNotification_WithSingleNotifyMethod_ReturnsExpectedData(
            Type scenarioType, DistributionChannels testDistributionChannel, int expectedResultsCount,
            NotifyMethods expectedNotificationMethod, string expectedContactDetails,
            int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(testDistributionChannel);
            Mock<IDataQueryService<NotificationEvent>> mockedDataQuery = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedDataQuery.Object)!;

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(expectedResultsCount));

                NotifyData firstResult = actualResult[0];
                Assert.That(firstResult.NotificationMethod, Is.EqualTo(expectedNotificationMethod));
                Assert.That(firstResult.ContactDetails, Is.EqualTo(expectedContactDetails));
                Assert.That(firstResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, firstResult.NotificationMethod, this._testConfiguration)));

                VerifyMethodCalls(
                    scenarioType, mockedQueryContext, mockedDataQuery,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }
        
        [TestCase(typeof(CaseCreatedScenario), 2, 1, 1)]
        [TestCase(typeof(CaseCaseStatusUpdatedScenario), 3, 1, 1)]
        [TestCase(typeof(CaseCaseFinishedScenario), 3, 1, 1)]
        [TestCase(typeof(DecisionMadeScenario), 2, 1, 1)]
        public async Task GetAllNotifyDataAsync_ForValidNotification_WithBothNotifyMethods_ReturnsBothExpectedData(
            Type scenarioType, int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(DistributionChannels.Both);
            Mock<IDataQueryService<NotificationEvent>> mockedDataQuery = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedDataQuery.Object)!;

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(2));

                NotifyData firstResult = actualResult[0];
                Assert.That(firstResult.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                Assert.That(firstResult.ContactDetails, Is.EqualTo(TestEmailAddress));
                Assert.That(firstResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, firstResult.NotificationMethod, this._testConfiguration)));

                NotifyData secondResult = actualResult[1];
                Assert.That(secondResult.NotificationMethod, Is.EqualTo(NotifyMethods.Sms));
                Assert.That(secondResult.ContactDetails, Is.EqualTo(TestPhoneNumber));
                Assert.That(secondResult.TemplateId, Is.EqualTo(
                    DetermineTemplateId(scenarioType, secondResult.NotificationMethod, this._testConfiguration)));

                VerifyMethodCalls(
                    scenarioType, mockedQueryContext, mockedDataQuery,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario), 1, 1, 0)]
        [TestCase(typeof(CaseCaseStatusUpdatedScenario), 1, 1, 0)]
        [TestCase(typeof(CaseCaseFinishedScenario), 1, 1, 0)]
        [TestCase(typeof(DecisionMadeScenario), 1, 1, 0)]
        public async Task GetAllNotifyDataAsync_ForNotification_WithoutNotifyMethod_ReturnsEmptyData_DoesNotThrowException(
            Type scenarioType, int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(DistributionChannels.None);
            Mock<IDataQueryService<NotificationEvent>> mockedDataQuery = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedDataQuery.Object)!;

            // Act
            NotifyData[] actualResult = await scenario.GetAllNotifyDataAsync(default);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Has.Length.EqualTo(0));

                VerifyMethodCalls(
                    scenarioType, mockedQueryContext, mockedDataQuery,
                    fromInvokeCount, partyInvokeCount, caseInvokeCount);
            });
        }

        [TestCase(typeof(CaseCreatedScenario))]
        [TestCase(typeof(CaseCaseStatusUpdatedScenario))]
        [TestCase(typeof(CaseCaseFinishedScenario))]
        [TestCase(typeof(DecisionMadeScenario))]
        public void GetAllNotifyDataAsync_ForNotification_WithUnknownNotifyMethod_ReturnsEmptyData_DoesNotThrowException(Type scenarioType)
        {
            // Arrange
            Mock<IQueryContext> mockedQueryContext = MockQueryContextMethods(DistributionChannels.Unknown);
            Mock<IDataQueryService<NotificationEvent>> mockedDataQuery = GetMockedQueryService(mockedQueryContext);

            INotifyScenario scenario = (BaseScenario)Activator.CreateInstance(scenarioType, this._testConfiguration, mockedDataQuery.Object)!;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GetAllNotifyDataAsync(default));
        }

        [Test]
        public void GetAllNotifyDataAsync_ForNotImplementedScenario_ThrowsNotImplementedException()
        {
            // Arrange
            var mockedQueryService = new Mock<IDataQueryService<NotificationEvent>>();

            INotifyScenario scenario = new NotImplementedScenario(this._testConfiguration, mockedQueryService.Object);

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(() => scenario.GetAllNotifyDataAsync(default));
        }
        #endregion

        #region Helper methods
        private static Mock<IQueryContext> MockQueryContextMethods(DistributionChannels testDistributionChannel)
        {
            var mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Loose);

            // GetCaseAsync()
            var testCase = new Case
            {
                Name = "Test case",
                Identification = "Test case Id"
            };

            mockedQueryContext
                .Setup(mock => mock.GetCaseAsync())
                .ReturnsAsync(testCase);

            mockedQueryContext
                .Setup(mock => mock.GetCaseAsync(It.IsAny<Uri?>()))
                .ReturnsAsync(testCase);
            
            // GetPartyDataAsync(string)
            var testParty = new CommonPartyData
            {
                Name = "Alice",
                SurnamePrefix = "van",
                Surname = "Wonderland",
                DistributionChannel = testDistributionChannel,
                EmailAddress = TestEmailAddress,
                TelephoneNumber = TestPhoneNumber
            };

            mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(It.IsAny<string>()))
                .ReturnsAsync(testParty);

            return mockedQueryContext;
        }

        private static Mock<IDataQueryService<NotificationEvent>> GetMockedQueryService(IMock<IQueryContext> mockedQueryContext)
        {
            var mockedQueryService = new Mock<IDataQueryService<NotificationEvent>>();
            
            mockedQueryService
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(mockedQueryContext.Object);

            return mockedQueryService;
        }

        private static string DetermineTemplateId<TStrategy>(TStrategy strategy, NotifyMethods notifyMethod, WebApiConfiguration configuration)
            where TStrategy : Type
        {
            return strategy.Name switch
            {
                nameof(CaseCreatedScenario) =>
                    notifyMethod switch
                    {
                        NotifyMethods.Email => configuration.User.TemplateIds.Email.ZaakCreate(),
                        NotifyMethods.Sms => configuration.User.TemplateIds.Sms.ZaakCreate(),
                        _ => string.Empty
                    },
                
                nameof(CaseCaseStatusUpdatedScenario) =>
                    notifyMethod switch
                    {
                        NotifyMethods.Email => configuration.User.TemplateIds.Email.ZaakUpdate(),
                        NotifyMethods.Sms => configuration.User.TemplateIds.Sms.ZaakUpdate(),
                        _ => string.Empty
                    },
                
                nameof(CaseCaseFinishedScenario) =>
                    notifyMethod switch
                    {
                        NotifyMethods.Email => configuration.User.TemplateIds.Email.ZaakClose(),
                        NotifyMethods.Sms => configuration.User.TemplateIds.Sms.ZaakClose(),
                        _ => string.Empty
                    },
                
                nameof(DecisionMadeScenario) =>
                    notifyMethod switch
                    {
                        NotifyMethods.Email => configuration.User.TemplateIds.Email.DecisionMade(),
                        NotifyMethods.Sms => configuration.User.TemplateIds.Sms.DecisionMade(),
                        _ => string.Empty
                    },

                _ => string.Empty
            };
        }

        private static void VerifyMethodCalls(MemberInfo scenarioType,
            Mock<IQueryContext> mockedQueryContext, Mock<IDataQueryService<NotificationEvent>> mockedDataQuery,
            int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            if (scenarioType.Name == nameof(DecisionMadeScenario))
            {
                VerifyMethodCalls_Decision(mockedQueryContext, mockedDataQuery, fromInvokeCount, partyInvokeCount, 1, caseInvokeCount);
            }
            else
            {
                VerifyMethodCalls_Case(mockedQueryContext, mockedDataQuery, fromInvokeCount, partyInvokeCount, caseInvokeCount);
            }
        }

        private static void VerifyMethodCalls_Case(
            Mock<IQueryContext> mockedQueryContext, Mock<IDataQueryService<NotificationEvent>> mockedDataQuery,
            int fromInvokeCount, int partyInvokeCount, int caseInvokeCount)
        {
            mockedDataQuery
                .Verify(mock => mock.From(
                        It.IsAny<NotificationEvent>()),
                    Times.Exactly(fromInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(
                        It.IsAny<string>()),
                    Times.Exactly(partyInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(),
                    Times.Exactly(caseInvokeCount));
        }

        private static void VerifyMethodCalls_Decision(
            Mock<IQueryContext> mockedQueryContext, Mock<IDataQueryService<NotificationEvent>> mockedDataQuery,
            int fromInvokeCount, int partyInvokeCount, int decisionInvokeCount, int caseInvokeCount)
        {
            mockedDataQuery
                .Verify(mock => mock.From(
                        It.IsAny<NotificationEvent>()),
                    Times.Exactly(fromInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetPartyDataAsync(
                        It.IsAny<string>()),
                    Times.Exactly(partyInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetDecisionAsync(),
                    Times.Exactly(decisionInvokeCount));

            mockedQueryContext
                .Verify(mock => mock.GetCaseAsync(It.IsAny<Uri?>()),
                    Times.Exactly(caseInvokeCount));
        }
        #endregion
    }
}