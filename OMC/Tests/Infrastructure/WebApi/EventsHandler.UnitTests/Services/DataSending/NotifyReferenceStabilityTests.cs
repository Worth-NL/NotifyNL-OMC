// © 2026, Worth Systems.

using Common.Extensions;
using EventsHandler.Services.Responding.v2;
using NUnit.Framework;
using System.Text.Json;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.MOBB.Models;
using WebQueries.Print.Models;
using WebQueries.Producten.Models;
using WebQueries.Tracing;
using ZgwModels.Mapping.Models.Interfaces;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotifyNL;
using ZgwModels.Serialization;
using ZgwModels.Serialization.Interfaces;

namespace EventsHandler.Tests.Unit.Services.DataSending
{
    /// <summary>
    /// The check against sending the same notification twice (#247) looks the reference up in "Notify NL", so the
    /// reference has to be the same on every delivery of a notification. It also has to stay well within the 1000
    /// characters "Notify NL" accepts, because it carries the notification itself.
    /// </summary>
    [TestFixture]
    public sealed class NotifyReferenceStabilityTests
    {
        /// <summary>
        /// Head room below the 1000 characters "Notify NL" accepts: a reference that grows past this has started to
        /// eat the margin a longer host name, path or a few extra "kenmerken" would need.
        /// </summary>
        private const int MaxReferenceLength = 800;

        private const string NotificationJson = """
        {
          "actie": "create",
          "kanaal": "zaken",
          "resource": "status",
          "kenmerken": {
            "zaaktype": "https://openzaak.gemeente-voorbeeldstad.nl/catalogi/api/v1/zaaktypen/b51b2a95-36ab-4628-8dbe-c2ecabc23afa",
            "bronorganisatie": "123456789",
            "vertrouwelijkheidaanduiding": "zaakvertrouwelijk"
          },
          "hoofdObject": "https://openzaak.gemeente-voorbeeldstad.nl/zaken/api/v1/zaken/7c0bb6a6-4b44-4d40-9f6c-3b0e8b4b1b0f",
          "resourceUrl": "https://openzaak.gemeente-voorbeeldstad.nl/zaken/api/v1/statussen/0c4e5a6d-1f2b-4c3d-8e9f-a1b2c3d4e5f6",
          "aanmaakdatum": "2026-10-26T10:15:30.123456Z"
        }
        """;

        private static readonly ISerializationService s_serializer = new SpecificSerializer();

        [TearDown]
        public void TearDown() => TraceContext.Clear();

        private static Task<string> EncodeAsync<TReference>(TReference reference) where TReference : struct, IJsonSerializable
            => s_serializer.Serialize(reference).CompressGZipAsync(CancellationToken.None);

        /// <summary>
        /// One delivery of the notification, the way het OMC handles it: parse it, start the trace seeded with it
        /// (NotifyProcessor), and build the reference (BaseScenario).
        /// </summary>
        private static Task<string> BuildZaakReferenceForOneDeliveryAsync()
        {
            NotificationEvent notification = s_serializer.Deserialize<NotificationEvent>(NotificationJson);
            TraceContext.Start(new TraceEmitter(), seed: s_serializer.Serialize(notification));

            return EncodeAsync(new NotifyReference
            {
                Notification = notification,
                CaseId = Guid.Parse("7c0bb6a6-4b44-4d40-9f6c-3b0e8b4b1b0f"),
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                TraceId = TraceContext.CurrentTraceId,
                SubjectTypeUri = new Uri("https://openzaak.gemeente-voorbeeldstad.nl/catalogi/api/v1/zaaktypen/b51b2a95-36ab-4628-8dbe-c2ecabc23afa")
            });
        }

        private static Task<string> BuildProductReferenceForOneDeliveryAsync()
        {
            NotificationEvent notification = s_serializer.Deserialize<NotificationEvent>(NotificationJson);
            TraceContext.Start(new TraceEmitter(), seed: s_serializer.Serialize(notification));

            return EncodeAsync(new ProductNotifyReference
            {
                ProductId = Guid.Parse("da0df49a-cd71-4e24-9bae-5be8b01f2c36"),
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductName = "Parkeervergunning bewoners zone centrum: Voorbeeldstraat 123",
                ProductTypeCode = "PARKEERVERGUNNING-BEWONERS",
                OriginalResourceUrl = "https://openproduct.gemeente-voorbeeldstad.nl/producten/api/v1/producten/da0df49a-cd71-4e24-9bae-5be8b01f2c36",
                TraceId = TraceContext.CurrentTraceId,
                ProductTypeUri = new Uri("https://openproduct.gemeente-voorbeeldstad.nl/producttypen/api/v1/producttypen/44444444-4444-4444-4444-444444444444")
            });
        }

        private static Task<string> BuildMessageBoxReferenceAsync() => EncodeAsync(new MessageBoxNotifyReference
        {
            CloudEvent = JsonSerializer.SerializeToElement(new
            {
                specversion = "1.0",
                type = "nl.overheid.berichten.bericht-gepubliceerd",
                source = "urn:nld:oin:00000001234567890000:systeem:openvtb",
                subject = "55555555-5555-5555-5555-555555555555",
                id = "66666666-6666-6666-6666-666666666666",
                time = "2026-10-26T10:15:30.123456Z",
                datacontenttype = "application/json",
                data = new { url = "https://openvtb.gemeente-voorbeeldstad.nl/berichten/api/v1/berichten/55555555-5555-5555-5555-555555555555" }
            }),
            MessageId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Mobb = false,
            WasGefaaldeNotificatie = true,
            MessageTypeUri = new Uri("https://openvtb.gemeente-voorbeeldstad.nl/berichten/api/v1/berichttypen/33333333-3333-3333-3333-333333333333")
        });

        private static Task<string> BuildPrintReferenceAsync() => EncodeAsync(new PrintNotifyReference
        {
            ObjectId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Subject = "Wij hebben aanvullende informatie van u nodig voor uw aanvraag",
            AttachmentId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            OriginalResourceUrl = new Uri("https://objecten.gemeente-voorbeeldstad.nl/api/v2/objects/11111111-1111-1111-1111-111111111111"),
            SubjectObjectTypeUri = new Uri("https://openzaak.gemeente-voorbeeldstad.nl/catalogi/api/v1/zaaktypen/b51b2a95-36ab-4628-8dbe-c2ecabc23afa")
        });

        [Test]
        public async Task ZaakReference_IsTheSameOnEveryDeliveryOfTheNotification()
        {
            string first = await BuildZaakReferenceForOneDeliveryAsync();
            string redelivery = await BuildZaakReferenceForOneDeliveryAsync();

            Assert.That(redelivery, Is.EqualTo(first));
        }

        [Test]
        public async Task ProductReference_IsTheSameOnEveryDeliveryOfTheNotification()
        {
            string first = await BuildProductReferenceForOneDeliveryAsync();
            string redelivery = await BuildProductReferenceForOneDeliveryAsync();

            Assert.That(redelivery, Is.EqualTo(first));
        }

        [Test]
        public async Task EveryReference_StaysWellWithinWhatNotifyAccepts()
        {
            string zaak = await BuildZaakReferenceForOneDeliveryAsync();
            string product = await BuildProductReferenceForOneDeliveryAsync();
            string messageBox = await BuildMessageBoxReferenceAsync();
            string print = await BuildPrintReferenceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(zaak, Has.Length.LessThanOrEqualTo(MaxReferenceLength), "zaak");
                Assert.That(product, Has.Length.LessThanOrEqualTo(MaxReferenceLength), "product");
                Assert.That(messageBox, Has.Length.LessThanOrEqualTo(MaxReferenceLength), "MOBB");
                Assert.That(print, Has.Length.LessThanOrEqualTo(MaxReferenceLength), "print");
            });
        }

        #region HandedOffAtUnixMs
        [Test]
        public void HandedOffAtUnixMs_ReferenceWithoutTheMoment_UsesWhenNotifyAcceptedIt()
        {
            var callback = new DeliveryReceipt { CreatedAt = new DateTime(2026, 10, 26, 10, 15, 30, DateTimeKind.Utc) };

            Assert.That(NotifyCallbackResponder.HandedOffAtUnixMs(null, callback),
                Is.EqualTo(new DateTimeOffset(2026, 10, 26, 10, 15, 30, TimeSpan.Zero).ToUnixTimeMilliseconds()));
        }

        [Test]
        public void HandedOffAtUnixMs_ReferenceFromBefore247_KeepsItsOwnMoment()
        {
            var callback = new DeliveryReceipt { CreatedAt = new DateTime(2026, 10, 26, 10, 15, 30, DateTimeKind.Utc) };

            Assert.That(NotifyCallbackResponder.HandedOffAtUnixMs(1_761_000_000_000, callback), Is.EqualTo(1_761_000_000_000));
        }

        [Test]
        public void HandedOffAtUnixMs_NoMomentAnywhere_IsNull()
        {
            Assert.That(NotifyCallbackResponder.HandedOffAtUnixMs(null, new DeliveryReceipt()), Is.Null);
        }
        #endregion
    }
}
