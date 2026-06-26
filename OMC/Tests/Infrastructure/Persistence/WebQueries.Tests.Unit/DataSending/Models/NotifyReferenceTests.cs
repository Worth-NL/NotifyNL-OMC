// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataSending.Models.DTOs;

namespace WebQueries.Tests.Unit.DataSending.Models
{
    [TestFixture]
    public sealed class NotifyReferenceTests
    {
        [Test]
        public void DefaultConstructor_CaseId_IsGuidEmpty()
        {
            NotifyReference reference = new();
            Assert.That(reference.CaseId, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void DefaultConstructor_PartyId_IsGuidEmpty()
        {
            NotifyReference reference = new();
            Assert.That(reference.PartyId, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void ObjectInitializer_SetsCaseIdAndPartyId()
        {
            Guid caseId = Guid.NewGuid();
            Guid partyId = Guid.NewGuid();

            NotifyReference reference = new() { CaseId = caseId, PartyId = partyId };

            Assert.Multiple(() =>
            {
                Assert.That(reference.CaseId, Is.EqualTo(caseId));
                Assert.That(reference.PartyId, Is.EqualTo(partyId));
            });
        }

        [Test]
        public void CaseId_CanBeNull()
        {
            NotifyReference reference = new() { CaseId = null };
            Assert.That(reference.CaseId, Is.Null);
        }
    }
}
