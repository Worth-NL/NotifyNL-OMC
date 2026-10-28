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

        [Test]
        public void Deserialize_ReferenceMintedBeforeSubjectTypeUri_StillDeserializes()
        {
            // A reference echoed back by "Notify NL" for a notification sent before this field existed
            System.Text.Json.Nodes.JsonObject json = System.Text.Json.JsonSerializer.SerializeToNode(
                new NotifyReference { PartyId = Guid.Parse("11111111-2222-3333-4444-555555555555") })!.AsObject();
            json.Remove(nameof(NotifyReference.SubjectTypeUri));

            NotifyReference reference = System.Text.Json.JsonSerializer.Deserialize<NotifyReference>(json);

            Assert.Multiple(() =>
            {
                Assert.That(reference.PartyId, Is.EqualTo(Guid.Parse("11111111-2222-3333-4444-555555555555")));
                Assert.That(reference.SubjectTypeUri, Is.Null);
            });
        }
    }
}
