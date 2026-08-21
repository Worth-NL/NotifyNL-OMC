// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Register.Interfaces;
using ZgwModels.Enums;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.Tests.Unit.Register.v2
{
    [TestFixture]
    public sealed class ContactRegistrationV2Tests
    {
        private OmcConfiguration _configuration = null!;

        [OneTimeSetUp]
        public void SetUp() =>
            _configuration = ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.BothValid_v2);

        [OneTimeTearDown]
        public void TearDown() => _configuration.Dispose();

        private static readonly NotifyReference s_reference = new()
        {

            CaseId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            PartyId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            Notification = new NotificationEvent()
        };

        private ITelemetryService CreateService()
        {
            IQueryContext queryContext = new Mock<IQueryContext>().Object;
            return new WebQueries.Register.v2.ContactRegistration(_configuration, queryContext, _configuration);
        }

        #region GetActorCustomerContactMomentJsonBody
        [Test]
        public void GetActorCustomerContactMomentJsonBody_ReturnsJsonWithBothGuids()
        {
            Guid actor = Guid.NewGuid();
            Guid customerContactMoment = Guid.NewGuid();

            string result = CreateService().GetActorCustomerContactMomentJsonBody(actor, customerContactMoment);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain(actor.ToString()));
                Assert.That(result, Does.Contain(customerContactMoment.ToString()));
            });
        }

        [Test]
        public void GetActorCustomerContactMomentJsonBody_IsValidJson()
        {
            string result = CreateService().GetActorCustomerContactMomentJsonBody(Guid.NewGuid(), Guid.NewGuid());

            Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(result));
        }
        #endregion

        #region IVersionDetails
        [Test]
        public void Name_ReturnsKlantcontacten()
        {
            string name = ((WebQueries.Versioning.Interfaces.IVersionDetails)CreateService()).Name;
            Assert.That(name, Is.EqualTo("Klantcontacten"));
        }

        [Test]
        public void Version_Returns200()
        {
            string version = ((WebQueries.Versioning.Interfaces.IVersionDetails)CreateService()).Version;
            Assert.That(version, Is.EqualTo("2.0.0"));
        }
        #endregion
    }
}
