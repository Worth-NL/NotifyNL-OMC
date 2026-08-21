// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataSending.Models.DTOs;
using ZgwModels.Enums;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;

namespace WebQueries.Tests.Unit.DataSending.Models
{
    [TestFixture]
    public sealed class NotifyDataTests
    {
        private static readonly Guid s_templateId = Guid.NewGuid();
        private static readonly Dictionary<string, object> s_personalization = new() { ["key"] = "value" };
        private static readonly NotifyReference s_reference = new() { CaseId = Guid.NewGuid(), PartyId = Guid.NewGuid() };

        #region Constructor (NotifyMethods only)
        [TestCase(NotifyMethods.Email)]
        [TestCase(NotifyMethods.Sms)]
        [TestCase(NotifyMethods.None)]
        public void Constructor_WithMethodOnly_SetsNotificationMethod(NotifyMethods method)
        {
            NotifyData data = new(method);
            Assert.That(data.NotificationMethod, Is.EqualTo(method));
        }

        [Test]
        public void Constructor_WithMethodOnly_ContactDetailsIsEmpty()
        {
            NotifyData data = new(NotifyMethods.Email);
            Assert.That(data.ContactDetails, Is.Empty);
        }

        [Test]
        public void Constructor_WithMethodOnly_TemplateIdIsEmpty()
        {
            NotifyData data = new(NotifyMethods.Email);
            Assert.That(data.TemplateId, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void Constructor_WithMethodOnly_PersonalizationIsEmpty()
        {
            NotifyData data = new(NotifyMethods.Email);
            Assert.That(data.Personalization, Is.Empty);
        }
        #endregion

        #region Full constructor
        [Test]
        public void Constructor_Full_SetsAllProperties()
        {
            const string contactDetails = "test@example.com";

            NotifyData data = new(NotifyMethods.Email, contactDetails, s_templateId, s_personalization, s_reference);

            Assert.Multiple(() =>
            {
                Assert.That(data.NotificationMethod, Is.EqualTo(NotifyMethods.Email));
                Assert.That(data.ContactDetails, Is.EqualTo(contactDetails));
                Assert.That(data.TemplateId, Is.EqualTo(s_templateId));
                Assert.That(data.Personalization, Is.EqualTo(s_personalization));
                Assert.That(data.Reference.CaseId, Is.EqualTo(s_reference.CaseId));
                Assert.That(data.Reference.PartyId, Is.EqualTo(s_reference.PartyId));
            });
        }

        [TestCase(NotifyMethods.Email)]
        [TestCase(NotifyMethods.Sms)]
        public void Constructor_Full_SetsNotificationMethod(NotifyMethods method)
        {
            NotifyData data = new(method, "contact", s_templateId, s_personalization, s_reference);
            Assert.That(data.NotificationMethod, Is.EqualTo(method));
        }
        #endregion
    }
}
