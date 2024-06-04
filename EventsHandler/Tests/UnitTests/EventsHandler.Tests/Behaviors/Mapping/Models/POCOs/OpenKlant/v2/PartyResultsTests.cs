// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2;
using EventsHandler.Configuration;
using EventsHandler.Properties;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.UnitTests.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    [TestFixture]
    public class PartyResultsTests
    {
        #region Party (method)
        [Test]
        public void Party_Method_ForMissingResults_ThrowsHttpRequestException()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetWebApiConfiguration();

            var partyResults = new PartyResults();  // Empty "Results" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(testConfiguration, partyResults, Resources.HttpRequest_ERROR_EmptyPartiesResults);
        }

        [Test]
        public void Party_Method_ForExistingResults_ButMissingAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetWebApiConfiguration();

            PartyResults partyResults = GetTestPartyResults();  // Empty "DigitalAddresses" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(testConfiguration, partyResults, Resources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_Method_ForExistingResults_WithMatchingPreferredAddress_ReturnsExpectedResult()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();
            
            var testGuid = Guid.NewGuid();

            const string testName = "John";
            const string testPrefix = "";
            const string testSurname = "Doe";
            const string testAddressValue = "john.does@gmail.com";

            string testAddressType = testConfiguration.AppSettings.Variables.EmailGenericDescription();
            
            var testParty = new PartyResult
            {
                PreferredDigitalAddress = new DigitalAddressShort
                {
                    Id = testGuid
                },
                Identification = new PartyIdentification
                {
                    Details = new PartyDetails
                    {
                        Name = testName,
                        SurnamePrefix = testPrefix,
                        Surname = testSurname
                    }
                },
                Expansion = new Expansion
                {
                    DigitalAddresses = new List<DigitalAddressLong>
                    {
                        new(), // Just empty address
                        new()
                        {
                            Id = testGuid,
                            Value = testAddressValue,
                            Type = testAddressType
                        }
                    }
                }
            };

            PartyResults partyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel,
             string actualEmailAddress, string actualPhoneNumber) =
                partyResults.Party(testConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo(testAddressValue));
                Assert.That(actualPhoneNumber, Is.Empty);
            });
        }

        #region Helper methods
        private static PartyResults GetTestPartyResults(params PartyResult[] roles)
        {
            var partyResults = new PartyResults
            {
                Results = new List<PartyResult>
                {
                    new()  // Just empty result to always have at least a few of them
                }
            };
            
            partyResults.Results.AddRange(roles);

            return partyResults;
        }

        private static void AssertThrows<TException>(WebApiConfiguration configuration, PartyResults partyResults, string exceptionMessage)
            where TException : Exception
        {
            Assert.Multiple(() =>
            {
                TException? exception = Assert.Throws<TException>(() =>
                    partyResults.Party(configuration));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
            });
        }
        #endregion
        #endregion
    }
}