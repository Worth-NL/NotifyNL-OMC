// © 2026, Worth Systems.

using Common.Extensions;
using NUnit.Framework;
using System.Text.Json;
using WebQueries.MOBB.Models;
using ZgwModels.Serialization;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.MOBB
{
    /// <summary>
    /// The check against sending the same Bericht twice (#247) looks the reference up in "Notify NL", so the same
    /// step of the same Bericht has to build exactly the same reference every time - also when the CloudEvent in it
    /// was read back from an earlier reference, as it is on the delivery-receipt path.
    /// </summary>
    [TestFixture]
    public sealed class MessageBoxNotifyReferenceTests
    {
        private static readonly JsonElement s_cloudEvent = JsonSerializer.SerializeToElement(new
        {
            specversion = "1.0",
            type = "nl.overheid.berichten.bericht-gepubliceerd",
            source = "urn:nld:oin:123456789000:openvtb",
            subject = "55555555-5555-5555-5555-555555555555",
            id = "66666666-6666-6666-6666-666666666666",
            time = "2026-01-01T00:00:00Z",
            dataref = (string?)null,
            datacontenttype = "application/json",
            data = new
            {
                onderwerp = "Uw aanvraag",
                publicatiedatum = "2026-01-01T00:00:00Z",
                ontvanger = "urn:nld:bsn:nummer:123456789"
            }
        });

        // The same steps MessageBoxScenarioImplementation.BuildReferenceAsync takes
        private static MessageBoxNotifyReference BuildReference(JsonElement cloudEvent) => new()
        {
            CloudEvent = MessageBoxNotifyReference.EventMetadataOf(cloudEvent),
            MessageId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Mobb = false,
            WasGefaaldeNotificatie = true,
            MessageTypeUri = new Uri("https://test.domain/api/v1/berichttypen/33333333-3333-3333-3333-333333333333")
        };

        private static Task<string> EncodeAsync(ISerializationService serializer, MessageBoxNotifyReference reference)
            => serializer.Serialize(reference).CompressGZipAsync(CancellationToken.None);

        [Test]
        public async Task SameStep_OfTheSameBericht_BuildsTheSameReferenceEveryTime()
        {
            ISerializationService serializer = new SpecificSerializer();

            string first = await EncodeAsync(serializer, BuildReference(s_cloudEvent));
            string second = await EncodeAsync(serializer, BuildReference(s_cloudEvent));

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public async Task CloudEventReadBackFromAnEarlierReference_BuildsTheSameReference()
        {
            ISerializationService serializer = new SpecificSerializer();

            string original = await EncodeAsync(serializer, BuildReference(s_cloudEvent));

            // The delivery-receipt path: decode the reference "Notify NL" echoed back, and build the next one from it
            MessageBoxNotifyReference readBack = serializer.Deserialize<MessageBoxNotifyReference>(
                await original.DecompressGZipAsync(CancellationToken.None));

            string rebuilt = await EncodeAsync(serializer, BuildReference(readBack.CloudEvent));

            Assert.That(rebuilt, Is.EqualTo(original));
        }

        #region Event metadata only (#265)
        [Test]
        public async Task Reference_HoldsNoEventData_SoNoBsn()
        {
            ISerializationService serializer = new SpecificSerializer();

            string reference = await (await EncodeAsync(serializer, BuildReference(s_cloudEvent))).DecompressGZipAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(reference, Does.Not.Contain("urn:nld:bsn"));
                Assert.That(reference, Does.Not.Contain("ontvanger"));
                Assert.That(reference, Does.Not.Contain("onderwerp"));
                Assert.That(reference, Does.Not.Contain("\"data\""));
            });
        }

        [Test]
        public void EventMetadataOf_KeepsOnlyTheEventMetadata()
        {
            JsonElement cloudEvent = JsonSerializer.SerializeToElement(new
            {
                specversion = "1.0",
                id = "66666666-6666-6666-6666-666666666666",
                source = "urn:nld:oin:123456789000:openvtb",
                type = "nl.overheid.berichten.bericht-gepubliceerd",
                subject = "55555555-5555-5555-5555-555555555555",
                time = "2026-01-01T00:00:00Z",
                datacontenttype = "application/json",
                dataref = "https://openvtb.test/data",
                someextension = "urn:nld:bsn:nummer:123456789",
                data = new { ontvanger = "urn:nld:bsn:nummer:123456789" }
            });

            JsonElement metadata = MessageBoxNotifyReference.EventMetadataOf(cloudEvent);

            Assert.That(metadata.EnumerateObject().Select(property => property.Name),
                Is.EqualTo(new[] { "specversion", "id", "source", "type", "subject", "time" }));
        }

        [Test]
        public async Task AnotherEvent_AboutTheSameBericht_BuildsAnotherReference()
        {
            // A redelivery carries the same event id and is not sent again; a new event about the same Bericht is
            JsonElement anotherEvent = JsonSerializer.SerializeToElement(new
            {
                specversion = "1.0",
                type = "nl.overheid.berichten.bericht-gepubliceerd",
                source = "urn:nld:oin:123456789000:openvtb",
                subject = "55555555-5555-5555-5555-555555555555",
                id = "77777777-7777-7777-7777-777777777777",
                time = "2026-01-01T00:00:00Z"
            });
            ISerializationService serializer = new SpecificSerializer();

            string first = await EncodeAsync(serializer, BuildReference(s_cloudEvent));
            string another = await EncodeAsync(serializer, BuildReference(anotherEvent));

            Assert.That(another, Is.Not.EqualTo(first));
        }

        [Test]
        public async Task ReferenceFromBefore265_WithTheFullCloudEvent_StaysReadable_AndBuildsTheNextStepWithoutData()
        {
            ISerializationService serializer = new SpecificSerializer();
            MessageBoxNotifyReference fromBefore = BuildReference(s_cloudEvent);
            fromBefore.CloudEvent = s_cloudEvent;  // As built before #265
            string original = await EncodeAsync(serializer, fromBefore);

            MessageBoxNotifyReference readBack = serializer.Deserialize<MessageBoxNotifyReference>(
                await original.DecompressGZipAsync(CancellationToken.None));
            string next = await EncodeAsync(serializer, BuildReference(readBack.CloudEvent));

            Assert.Multiple(() =>
            {
                Assert.That(readBack.MessageId, Is.EqualTo(fromBefore.MessageId));
                Assert.That(next, Is.EqualTo(EncodeAsync(serializer, BuildReference(s_cloudEvent)).Result));
            });
        }
        #endregion
    }
}
