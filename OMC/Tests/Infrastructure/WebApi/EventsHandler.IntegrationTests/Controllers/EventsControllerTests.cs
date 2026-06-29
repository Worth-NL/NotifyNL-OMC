// © 2024, Worth Systems.

using EventsHandler.Tests.Integration._TestHelpers;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EventsHandler.Tests.Integration.Controllers
{
    [TestFixture]
    public sealed class EventsControllerTests
    {
        private static Dictionary<string, string> s_envVars = [];
        private static OmcWebApplicationFactory s_factory = null!;
        private static HttpClient s_client = null!;
        private static string s_validToken = string.Empty;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            s_envVars = LaunchSettingsReader.GetEnvironmentVariables();

            if (!LaunchSettingsReader.HasRealValues(s_envVars))
            {
                Assert.Ignore("Integration tests require real values in launchSettings.json — skipping in CI.");
                return;
            }

            s_factory    = new OmcWebApplicationFactory(s_envVars);
            s_client     = s_factory.CreateClient();
            s_validToken = JwtTokenHelper.GenerateToken(s_envVars);
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            s_client?.Dispose();
            s_factory?.Dispose();
        }

        #region GET Events/Version
        [Test]
        public async Task Version_WithoutToken_Returns401()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Events/Version");

            HttpResponseMessage response = await s_client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Version_WithValidToken_Returns200()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Events/Version");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", s_validToken);

            HttpResponseMessage response = await s_client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region POST Events/Listen
        [Test]
        public async Task Listen_WithoutToken_Returns401()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Events/Listen")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await s_client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Listen_WithValidToken_NoBody_Returns400()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Events/Listen");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", s_validToken);
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await s_client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Listen_WithValidToken_TestPingNotification_Returns206()
        {
            // A "test ping" notification: Channel="-", Resource="-", both URIs = the hardcoded test URL.
            // The processor recognises this pattern as a connectivity test and returns Skipped (→ 206).
            const string testPingJson =
                """
                {
                  "actie": "create",
                  "kanaal": "-",
                  "resource": "-",
                  "kenmerken": {
                    "zaaktype": "http://some.hoofdobject.nl/",
                    "bronorganisatie": "000000000",
                    "vertrouwelijkheidaanduiding": "openbaar",
                    "objectType": "http://some.hoofdobject.nl/",
                    "besluittype": "http://some.hoofdobject.nl/",
                    "verantwoordelijkeOrganisatie": "000000000"
                  },
                  "hoofdObject": "http://some.hoofdobject.nl/",
                  "resourceUrl": "http://some.hoofdobject.nl/",
                  "aanmaakdatum": "2024-01-01T00:00:00Z"
                }
                """;

            var request = new HttpRequestMessage(HttpMethod.Post, "/Events/Listen")
            {
                Content = new StringContent(testPingJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", s_validToken);

            HttpResponseMessage response = await s_client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.PartialContent));
        }

        [Test]
        public async Task Listen_WithExpiredToken_Returns401()
        {
            string expiredToken = JwtTokenHelper.GenerateExpiredToken(s_envVars);

            var request = new HttpRequestMessage(HttpMethod.Post, "/Events/Listen")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

            HttpResponseMessage response = await s_client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        #endregion
    }
}
