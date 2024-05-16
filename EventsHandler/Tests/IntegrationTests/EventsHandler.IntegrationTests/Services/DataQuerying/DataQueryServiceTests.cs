// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataLoading.Strategy.Interfaces;
using EventsHandler.Services.DataQuerying;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataReceiving;
using EventsHandler.Services.DataReceiving.Factories;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataReceiving.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.Extensions.Configuration;
using SecretsManager.Services.Authentication.Encryptions.Strategy;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using System.Text.Json;

namespace EventsHandler.IntegrationTests.Services.DataQuerying
{
    [TestFixture]
    public sealed class DataQueryServiceTests
    {
        private IDataQueryService<NotificationEvent>? _dataQuery;
        private NotificationEvent _notification;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            // Mocked IQueryContext
            IQueryContext queryContext = new Mock<IQueryContext>().Object;  // TODO: MockBehavior.Strict

            // Mocked IHttpSupplier
            IConfiguration appSettings = ConfigurationHandler.GetConfiguration();
            ILoadersContext loadersContext = new Mock<ILoadersContext>().Object;  // TODO: MockBehavior.Strict
            WebApiConfiguration configuration = new(appSettings, loadersContext);
            EncryptionContext encryptionContext = new(new SymmetricEncryptionStrategy());
            IHttpClientFactory<HttpClient, (string, string)[]> httpClientFactory = new HeadersHttpClientFactory();
            IHttpSupplierService supplier = new JwtHttpSupplier(configuration, encryptionContext, httpClientFactory);

            // Larger services
            this._dataQuery = new DataQueryService(queryContext, supplier);

            // Notification
            this._notification = NotificationEventHandler.GetNotification_Test_WithOrphans_ManuallyCreated();
        }

        [Test, Ignore("Used for debug purposes only")]  // TODO: To be replaced by mocks later
        public async Task GetCaseAsync_ReturnsExpectedResult()
        {
            // Act
            string result = (await this._dataQuery!.From(this._notification).GetCaseAsync()).Name;

            // Assert
            Assert.That(result, Is.EqualTo("Klacht afhandeling"));
        }

        [Test, Ignore("Used for debug purposes only")]  // TODO: To be replaced by mocks later
        public async Task GetCitizenDetailsAsync_ReturnsExpectedResult()
        {
            // Act
            CitizenDetails result = await this._dataQuery!.From(this._notification).GetCitizenDetailsAsync();

            // Assert
            string serializedResult = JsonSerializer.Serialize(result).Replace("\\", "").Replace("u002B", "+");  // Replace "\+" with "+"

            Assert.That(serializedResult, Is.EqualTo("""{"count":1,"results":[{"voornaam":"Berthold","voorvoegselAchternaam":"van der","achternaam":"Brecht","geslachtsaanduiding":"","aanmaakkanaal":"-","telefoonnummer":"+31615551461","emailadres":"fmolenaar@worth.systems"}]}"""));
        }

        [Test, Ignore("Used for debug purposes only")]  // TODO: To be replaced by mocks later
        public async Task GetCaseStatusesAsync_ReturnsExpectedResult()
        {
            // Act
            CaseStatuses result = await this._dataQuery!.From(this._notification).GetCaseStatusesAsync();

            // Assert
            string serializedResult = JsonSerializer.Serialize(result);

            Assert.That(serializedResult, Is.EqualTo("""{"count":6,"results":[{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/529f5ad3-c33e-4200-84e6-c6d6f0c41e88","datumStatusGezet":"2023-09-26T08:15:22Z"},{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/4c3fcd18-651b-4ada-86af-eb47c72c0654","datumStatusGezet":"2023-09-19T17:02:38Z"},{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/529f5ad3-c33e-4200-84e6-c6d6f0c41e88","datumStatusGezet":"2023-09-19T17:02:18Z"},{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/529f5ad3-c33e-4200-84e6-c6d6f0c41e88","datumStatusGezet":"2020-08-24T14:15:22Z"},{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/529f5ad3-c33e-4200-84e6-c6d6f0c41e88","datumStatusGezet":"2019-08-24T14:15:22Z"},{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/529f5ad3-c33e-4200-84e6-c6d6f0c41e88","datumStatusGezet":"2018-08-24T14:15:22Z"}]}"""));
        }

        [Test, Ignore("Used for debug purposes only")]  // TODO: To be replaced by mocks later
        public async Task GetLastCaseStatusTypeAsync_ReturnsExpectedResult()
        {
            // Arrange
            CaseStatuses caseStatuses = JsonSerializer.Deserialize<CaseStatuses>("""{"count":2,"results":[{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/529f5ad3-c33e-4200-84e6-c6d6f0c41e88","datumStatusGezet":"2023-09-26T08:15:22Z"},{"statustype":"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/statustypen/4c3fcd18-651b-4ada-86af-eb47c72c0654","datumStatusGezet":"2023-09-19T17:02:38Z"}]}""");

            // Act
            CaseStatusType result = await this._dataQuery!.From(this._notification).GetLastCaseStatusTypeAsync(caseStatuses);

            // Assert
            string serializedResult = JsonSerializer.Serialize(result);

            Assert.That(serializedResult, Is.EqualTo("""{"omschrijvingGeneriek":"Geregistreerd","isEindstatus":false}"""));
        }
    }
}