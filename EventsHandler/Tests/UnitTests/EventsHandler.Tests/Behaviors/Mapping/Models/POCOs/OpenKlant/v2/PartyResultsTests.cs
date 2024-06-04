// © 2024, Worth Systems.

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

        #region Helper methods
        private static PartyResults GetTestPartyResults(params PartyResult[] roles)
        {
            var caseRoles = new PartyResults
            {
                Results = new List<PartyResult>
                {
                }
            };

            caseRoles.Results.AddRange(roles);

            return caseRoles;
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