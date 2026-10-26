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
            id = "66666666-6666-6666-6666-666666666666"
        });

        private static MessageBoxNotifyReference BuildReference(JsonElement cloudEvent) => new()
        {
            CloudEvent = cloudEvent,
            MessageId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Mobb = false,
            WasGefaaldeNotificatie = true,
            MessageTypeUri = new Uri("https://test.domain/api/v1/berichttypen/33333333-3333-3333-3333-333333333333")
        };

        // The same steps MessageBoxScenarioImplementation.BuildReferenceAsync takes
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
    }
}
