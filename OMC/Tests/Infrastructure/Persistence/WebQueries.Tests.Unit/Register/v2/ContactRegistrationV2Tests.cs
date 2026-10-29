// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.MOBB.Models;
using WebQueries.Print.Models;
using WebQueries.Producten.Models;
using WebQueries.Register.Interfaces;
using ZgwModels.Enums;
using ZgwModels.Mapping.Models.POCOs.Objecten.Print;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.v2;

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

        #region GetPrintContactMomentJsonBody
        [Test]
        public void GetPrintContactMomentJsonBody_PayloadSuppliedCodes_WinOverConfiguration()
        {
            // Arrange
            var reference = new PrintNotifyReference
            {
                ObjectId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Subject = "Wij hebben aanvullende informatie nodig",
                SubjectObject = new SubjectObjectIdentifier
                {
                    ObjectId = "44444444-4444-4444-4444-444444444444",
                    CodeObjectType = "zaak",
                    CodeRegister = "openzaak",
                    CodeSoortObjectId = "uuid"
                }
            };

            // Act
            string actualResult = CreateService().GetPrintContactMomentJsonBody(reference, NotifyMethods.Letter, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain("\"objectId\":\"44444444-4444-4444-4444-444444444444\""));
                Assert.That(actualResult, Does.Contain("\"codeObjecttype\":\"zaak\""));
                Assert.That(actualResult, Does.Contain("\"codeRegister\":\"openzaak\""));
                Assert.That(actualResult, Does.Contain("\"codeSoortObjectId\":\"uuid\""));

                // The subject falls back to the reference when no messages are supplied.
                Assert.That(actualResult, Does.Contain("Wij hebben aanvullende informatie nodig"));
                Assert.That(actualResult, Does.Contain("\"uuid\":\"22222222-2222-2222-2222-222222222222\""));
            });
        }

        [Test]
        public void GetPrintContactMomentJsonBody_BlankPayloadCodes_FallBackToConfiguration()
        {
            // Arrange: only the objectId is supplied, which is the shape to expect when the writing party
            // does not care to restate what OMC already has configured.
            var reference = new PrintNotifyReference
            {
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                SubjectObject = new SubjectObjectIdentifier
                {
                    ObjectId = "44444444-4444-4444-4444-444444444444"
                }
            };

            string expectedCodeObjectType = _configuration.AppSettings.Variables.OpenKlant.CodeObjectType();
            string expectedCodeRegister = _configuration.AppSettings.Variables.OpenKlant.CodeRegister();

            // Act
            string actualResult = CreateService().GetPrintContactMomentJsonBody(reference, NotifyMethods.Letter, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain($"\"codeObjecttype\":\"{expectedCodeObjectType}\""));
                Assert.That(actualResult, Does.Contain($"\"codeRegister\":\"{expectedCodeRegister}\""));
            });
        }

        [Test]
        public void GetPrintContactMomentJsonBody_NeverMentionsBijlagen()
        {
            // Arrange: "maak-klantcontact" has no field for attachments (MBO-1025 - requested by Den Haag
            // but deferred), so the PDF is linked afterwards by a separate call to "/bijlagen". Sending it
            // inline here would be rejected or silently ignored.
            var reference = new PrintNotifyReference
            {
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                AttachmentId = Guid.Parse("33333333-3333-3333-3333-333333333333")
            };

            // Act
            string actualResult = CreateService().GetPrintContactMomentJsonBody(reference, NotifyMethods.Letter, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Not.Contain("bijlage"));
            });
        }

        [Test]
        public void GetBijlageJsonBody_LinksTheDocumentToTheKlantcontact()
        {
            // Arrange
            Guid klantcontactUuid = Guid.Parse("44444444-4444-4444-4444-444444444444");
            Guid documentUuid = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // Act
            string actualResult = CreateService().GetBijlageJsonBody(klantcontactUuid, documentUuid);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain($"\"wasBijlageVanKlantcontact\":{{\"uuid\":\"{klantcontactUuid}\"}}"));
                Assert.That(actualResult, Does.Contain($"\"objectId\":\"{documentUuid}\""));
                Assert.That(actualResult, Does.Contain("\"codeObjecttype\":"));
                Assert.That(actualResult, Does.Contain("\"codeRegister\":"));
                Assert.That(actualResult, Does.Contain("\"codeSoortObjectId\":"));
            });
        }

        [Test]
        public void GetPrintContactMomentJsonBody_SubjectWithQuotes_StaysValidJson()
        {
            // Arrange: the subject is written by an external party, so it has to survive being embedded.
            var reference = new PrintNotifyReference
            {
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Subject = """Betreft: "aanvullende informatie" \ dossier"""
            };

            // Act
            string actualResult = CreateService().GetPrintContactMomentJsonBody(reference, NotifyMethods.Letter, []);

            // Assert
            Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
        }

        [Test]
        public void GetPrintContactMomentJsonBody_IncludesOriginalResourceUrlInMetadata()
        {
            // Arrange: GZAC relates its own print-request process to the contactmoment via this URL.
            var reference = new PrintNotifyReference
            {
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                OriginalResourceUrl = new Uri("https://objecten.test/api/v2/objects/55555555-5555-5555-5555-555555555555")
            };

            // Act
            string actualResult = CreateService().GetPrintContactMomentJsonBody(reference, NotifyMethods.Letter, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain(
                    "\"metadata\":{\"originalResourceUrl\":\"https://objecten.test/api/v2/objects/55555555-5555-5555-5555-555555555555\"}"));
            });
        }

        [Test]
        public void GetPrintContactMomentJsonBody_SubjectObjectTypeUriSupplied_IncludesHoofdOnderwerpType()
        {
            // Arrange: "contact_hoofdOnderwerpType" identifies the zaaktype of the onderwerpobject and has
            // no configured fallback - it is registered verbatim as the klantcontact's "hoofdOnderwerpType".
            var reference = new PrintNotifyReference
            {
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                SubjectObjectTypeUri = new Uri("https://openzaak.test/catalogi/api/v1/zaaktypen/66666666-6666-6666-6666-666666666666")
            };

            // Act
            string actualResult = CreateService().GetPrintContactMomentJsonBody(reference, NotifyMethods.Letter, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain(
                    "\"hoofdOnderwerpType\":\"https://openzaak.test/catalogi/api/v1/zaaktypen/66666666-6666-6666-6666-666666666666\""));
            });
        }

        [Test]
        public void GetPrintContactMomentJsonBody_SubjectObjectTypeUriAbsent_HoofdOnderwerpTypeIsEmptyString()
        {
            // Arrange: unlike "onderwerpobjectidentificator", there is no sensible tenant-wide default for
            // a case-specific zaaktype URI - an omitted value is registered as an empty string, not defaulted.
            var reference = new PrintNotifyReference
            {
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222")
            };

            // Act
            string actualResult = CreateService().GetPrintContactMomentJsonBody(reference, NotifyMethods.Letter, []);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain("\"hoofdOnderwerpType\":\"\""));
            });
        }
        #endregion

        #region GetNewCreateContactMomentJsonBody
        [Test]
        public void GetNewCreateContactMomentJsonBody_IncludesOriginalResourceUrlInMetadata()
        {
            // Arrange: e.g. for a case-status-update notification, this is the specific status URI -
            // distinct from the case URI already linked via onderwerpobject/reference.CaseId.
            var reference = new NotifyReference
            {
                CaseId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                PartyId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                Notification = new NotificationEvent
                {
                    ResourceUri = new Uri("https://openzaak.test/zaken/api/v1/statussen/66666666-6666-6666-6666-666666666666")
                }
            };

            // Act: real callers (NotifyCallbackResponder) always pass 4 messages, the 3rd being the
            // "true"/"false" success flag - matching that shape here rather than an unrealistic empty list.
            string actualResult = CreateService().GetNewCreateContactMomentJsonBody(
                reference, NotifyMethods.Email, ["Subject", "Body", "true"]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain(
                    "\"metadata\":{\"originalResourceUrl\":\"https://openzaak.test/zaken/api/v1/statussen/66666666-6666-6666-6666-666666666666\"}"));
            });
        }

        [Test]
        public void GetNewCreateContactMomentJsonBody_SubjectTypeUriSupplied_IncludesHoofdOnderwerpType()
        {
            // Arrange
            NotifyReference reference = s_reference;
            reference.Notification = new NotificationEvent { ResourceUri = new Uri("https://openzaak.test/zaken/api/v1/statussen/1") };
            reference.SubjectTypeUri = new Uri("https://openzaak.test/catalogi/api/v1/zaaktypen/66666666-6666-6666-6666-666666666666");

            // Act
            string actualResult = CreateService().GetNewCreateContactMomentJsonBody(reference, NotifyMethods.Email, ["Subject", "Body", "true"]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain(
                    "\"hoofdOnderwerpType\":\"https://openzaak.test/catalogi/api/v1/zaaktypen/66666666-6666-6666-6666-666666666666\""));
            });
        }

        [Test]
        public void GetNewCreateContactMomentJsonBody_SubjectTypeUriAbsent_HoofdOnderwerpTypeIsEmptyString()
        {
            // Arrange
            NotifyReference reference = s_reference;
            reference.Notification = new NotificationEvent { ResourceUri = new Uri("https://openzaak.test/zaken/api/v1/statussen/1") };

            // Act
            string actualResult = CreateService().GetNewCreateContactMomentJsonBody(reference, NotifyMethods.Email, ["Subject", "Body", "true"]);

            // Assert
            Assert.That(actualResult, Does.Contain("\"hoofdOnderwerpType\":\"\""));
        }
        #endregion

        #region GetProductContactMomentJsonBody (hoofdOnderwerpType)
        [Test]
        public void GetProductContactMomentJsonBody_ProductTypeUriSupplied_IncludesHoofdOnderwerpType()
        {
            // Arrange
            ProductNotifyReference reference = s_productReference;
            reference.ProductTypeUri = new Uri("https://test.domain/producttypen/api/v1/producttypen/44444444-4444-4444-4444-444444444444");

            // Act
            string actualResult = CreateService().GetProductContactMomentJsonBody(reference, NotifyMethods.Email, ["Subject", "Body", "true"]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain(
                    "\"hoofdOnderwerpType\":\"https://test.domain/producttypen/api/v1/producttypen/44444444-4444-4444-4444-444444444444\""));
            });
        }

        [Test]
        public void GetProductContactMomentJsonBody_ProductTypeUriAbsent_HoofdOnderwerpTypeIsEmptyString()
        {
            // Act
            string actualResult = CreateService().GetProductContactMomentJsonBody(s_productReference, NotifyMethods.Email, ["Subject", "Body", "true"]);

            // Assert
            Assert.That(actualResult, Does.Contain("\"hoofdOnderwerpType\":\"\""));
        }
        #endregion

        #region GetMessageBoxContactMomentJsonBody
        [Test]
        public void GetMessageBoxContactMomentJsonBody_MessageTypeUriSupplied_IncludesHoofdOnderwerpType()
        {
            // Arrange
            var reference = new MessageBoxNotifyReference
            {
                MessageTypeUri = new Uri("https://openvtb.test/berichten/api/v1/berichttypen/33333333-3333-3333-3333-333333333333")
            };

            // Act
            string actualResult = CreateService().GetMessageBoxContactMomentJsonBody(reference, NotifyMethods.Mobb, ["Subject", "Body", "true"]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain(
                    "\"hoofdOnderwerpType\":\"https://openvtb.test/berichten/api/v1/berichttypen/33333333-3333-3333-3333-333333333333\""));
            });
        }

        [Test]
        public void GetMessageBoxContactMomentJsonBody_MessageTypeUriAbsent_HoofdOnderwerpTypeIsEmptyString()
        {
            // Act: a reference minted before the BerichtType was carried along
            string actualResult = CreateService().GetMessageBoxContactMomentJsonBody(
                new MessageBoxNotifyReference(), NotifyMethods.Mobb, ["Subject", "Body", "true"]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(actualResult));
                Assert.That(actualResult, Does.Contain("\"hoofdOnderwerpType\":\"\""));
            });
        }
        #endregion

        #region verdereActieOndernomen
        private static bool FurtherActionTaken(string json)
            => System.Text.Json.JsonDocument.Parse(json).RootElement
                .GetProperty("klantcontact").GetProperty("verdereActieOndernomen").GetBoolean();

        [Test]
        public void ContactMomentJsonBody_NonMobbFlows_VerdereActieOndernomen_IsAlwaysFalse()
        {
            // Arrange
            NotifyReference zaakReference = s_reference;
            zaakReference.Notification = new NotificationEvent { ResourceUri = new Uri("https://openzaak.test/zaken/api/v1/statussen/1") };
            ITelemetryService service = CreateService();

            // Act & Assert: the zaak, taak, print and product flows never try another channel themselves
            Assert.Multiple(() =>
            {
                Assert.That(FurtherActionTaken(service.GetNewCreateContactMomentJsonBody(zaakReference, NotifyMethods.Email, ["Subject", "Body", "false"])), Is.False);
                Assert.That(FurtherActionTaken(service.GetPrintContactMomentJsonBody(new PrintNotifyReference(), NotifyMethods.Letter, ["Subject", "Body", "false"])), Is.False);
                Assert.That(FurtherActionTaken(service.GetProductContactMomentJsonBody(s_productReference, NotifyMethods.Email, ["Subject", "Body", "false"])), Is.False);
            });
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void GetMessageBoxContactMomentJsonBody_VerdereActieOndernomen_FollowsTheCaller(string furtherActionTaken, bool expected)
        {
            string actualResult = CreateService().GetMessageBoxContactMomentJsonBody(
                new MessageBoxNotifyReference(), NotifyMethods.Mobb, ["Subject", "Body", "false", string.Empty, furtherActionTaken]);

            Assert.That(FurtherActionTaken(actualResult), Is.EqualTo(expected));
        }

        [Test]
        public void GetMessageBoxContactMomentJsonBody_VerdereActieOndernomenNotSupplied_IsFalse()
        {
            string actualResult = CreateService().GetMessageBoxContactMomentJsonBody(
                new MessageBoxNotifyReference(), NotifyMethods.Mobb, ["Subject", "Body", "true"]);

            Assert.That(FurtherActionTaken(actualResult), Is.False);
        }
        #endregion

        #region Inhoud length
        private static readonly string s_longBody = new('x', 1500);

        private static IEnumerable<TestCaseData> EveryContactMomentBuilder()
        {
            var zaakReference = new NotifyReference
            {
                CaseId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                PartyId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                Notification = new NotificationEvent { ResourceUri = new Uri("https://openzaak.test/zaken/api/v1/statussen/1") }
            };

            yield return new TestCaseData(new Func<ITelemetryService, string>(service =>
                service.GetNewCreateContactMomentJsonBody(zaakReference, NotifyMethods.Email, ["Subject", s_longBody, "true"])))
                .SetName("Zaak");
            yield return new TestCaseData(new Func<ITelemetryService, string>(service =>
                service.GetPrintContactMomentJsonBody(new PrintNotifyReference(), NotifyMethods.Letter, ["Subject", s_longBody, "true"])))
                .SetName("Print");
            yield return new TestCaseData(new Func<ITelemetryService, string>(service =>
                service.GetProductContactMomentJsonBody(s_productReference, NotifyMethods.Email, ["Subject", s_longBody, "true"])))
                .SetName("Product");
            yield return new TestCaseData(new Func<ITelemetryService, string>(service =>
                service.GetMessageBoxContactMomentJsonBody(new MessageBoxNotifyReference(), NotifyMethods.Email, ["Subject", s_longBody, "true"])))
                .SetName("MOBB");
        }

        [TestCaseSource(nameof(EveryContactMomentBuilder))]
        public void ContactMomentJsonBody_BodyLongerThan1000_IsCutTo1000EndingInThreeDots(Func<ITelemetryService, string> build)
        {
            // Act
            string actualResult = build(CreateService());

            // Assert
            string inhoud = System.Text.Json.JsonDocument.Parse(actualResult)
                .RootElement.GetProperty("klantcontact").GetProperty("inhoud").GetString()!;

            Assert.Multiple(() =>
            {
                Assert.That(inhoud, Has.Length.EqualTo(1000));
                Assert.That(inhoud, Does.EndWith("..."));
                Assert.That(inhoud, Does.Not.Contain("…"));
            });
        }
        #endregion

        #region GetProductContactMomentJsonBody
        private static readonly ProductNotifyReference s_productReference = new()
        {
            ProductId = Guid.Parse("da0df49a-cd71-4e24-9bae-5be8b01f2c36"),
            PartyId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            ProductName = "Parkeervergunning",
            ProductTypeCode = "PARKEERVERGUNNING-A",
            OriginalResourceUrl = "https://test.domain/producten/api/v1/producten/da0df49a-cd71-4e24-9bae-5be8b01f2c36"
        };

        [Test]
        public void GetProductContactMomentJsonBody_WritesTheKlantcontact_TheOwnersPartij_AndTheProductAsOnderwerpobject()
        {
            // Act
            string json = CreateService().GetProductContactMomentJsonBody(
                s_productReference, NotifyMethods.Email,
                ["Subject", "Body", "false", "2026-10-05T12:00:00.0000000+02:00"]);

            // Assert
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);
            System.Text.Json.JsonElement root = document.RootElement;
            System.Text.Json.JsonElement klantcontact = root.GetProperty("klantcontact");
            System.Text.Json.JsonElement identificator = root.GetProperty("onderwerpobject").GetProperty("onderwerpobjectidentificator");

            Assert.Multiple(() =>
            {
                Assert.That(klantcontact.GetProperty("onderwerp").GetString(), Is.EqualTo("Subject"));
                Assert.That(klantcontact.GetProperty("inhoud").GetString(), Is.EqualTo("Body"));
                Assert.That(klantcontact.GetProperty("indicatieContactGelukt").GetBoolean(), Is.False);
                Assert.That(klantcontact.GetProperty("metadata").GetProperty("originalResourceUrl").GetString(),
                    Is.EqualTo(s_productReference.OriginalResourceUrl));

                Assert.That(root.GetProperty("betrokkene").GetProperty("wasPartij").GetProperty("uuid").GetString(),
                    Is.EqualTo(s_productReference.PartyId.ToString()));

                Assert.That(identificator.GetProperty("objectId").GetString(), Is.EqualTo(s_productReference.ProductId.ToString()));
                Assert.That(identificator.GetProperty("codeObjecttype").GetString(),
                    Is.EqualTo(_configuration.AppSettings.Variables.OpenKlant.CodeObjectType_Product()));
                Assert.That(identificator.GetProperty("codeRegister").GetString(),
                    Is.EqualTo(_configuration.AppSettings.Variables.OpenKlant.CodeRegister_Product()));
            });
        }

        [Test]
        public void GetProductContactMomentJsonBody_SubjectAndBodyWithQuotes_StayValidJson()
        {
            // NOTE: The body can carry the text of an exception or a Notify NL error (the failure reason).

            // Act
            string json = CreateService().GetProductContactMomentJsonBody(
                s_productReference, NotifyMethods.Email,
                ["He said \"hi\"", "Reden: path C:\\temp \"quoted\"\nnext line", "false"]);

            // Assert
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty("klantcontact").GetProperty("inhoud").GetString(),
                Is.EqualTo("Reden: path C:\\temp \"quoted\"\nnext line"));
        }

        [Test]
        public void GetProductContactMomentJsonBody_NoMessages_FallsBackToTheProductName_AndASuccessfulContact()
        {
            // Act
            string json = CreateService().GetProductContactMomentJsonBody(s_productReference, NotifyMethods.Email, []);

            // Assert
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);
            System.Text.Json.JsonElement klantcontact = document.RootElement.GetProperty("klantcontact");

            Assert.Multiple(() =>
            {
                Assert.That(klantcontact.GetProperty("onderwerp").GetString(), Is.EqualTo(s_productReference.ProductName));
                Assert.That(klantcontact.GetProperty("indicatieContactGelukt").GetBoolean(), Is.True);
            });
        }
        #endregion

        #region ReportProductCompletionAsync
        private static (ITelemetryService Service, Mock<IQueryContext> QueryContext) CreateServiceWithQueryContext(
            OmcConfiguration configuration)
        {
            Mock<IQueryContext> queryContext = new();
            return (new WebQueries.Register.v2.ContactRegistration(configuration, queryContext.Object, configuration), queryContext);
        }

        private static MaakKlantContact GetCreatedKlantcontact()
            => new()
            {
                ContactMoment = new ContactMoment
                {
                    ReferenceUri = new Uri($"https://openklant.test/klantinteracties/api/v1/klantcontacten/{Guid.NewGuid()}")
                }
            };

        #region Writing a klantcontact once per delivery receipt
        private static readonly Guid s_notifyNotificationId = Guid.Parse("99999999-8888-7777-6666-555555555555");
        private static readonly Guid s_existingKlantcontactId = Guid.Parse("12121212-3434-5656-7878-909090909090");

        private static ProductNotifyReference ProductReferenceFromReceipt()
        {
            ProductNotifyReference reference = s_productReference;
            reference.NotifyNotificationId = s_notifyNotificationId;
            return reference;
        }

        private static KlantcontactResults Found(string notificationId, bool hasActor, Guid actorId, bool hasAttachment = false)
            => new()
            {
                Results =
                [
                    new ExistingKlantcontact
                    {
                        Id = s_existingKlantcontactId,
                        Metadata = new Dictionary<string, string> { ["notifyNotificationId"] = notificationId },
                        Actors = hasActor ? [new LinkedResource { Id = actorId }] : [],
                        Attachments = hasAttachment ? [new LinkedResource { Id = Guid.NewGuid() }] : []
                    }
                ]
            };

        [Test]
        public async Task ReportProductCompletionAsync_FirstReceipt_LooksUp_CreatesWithTheNotifyIdInItsMetadata_AndLinksTheActor()
        {
            // Arrange
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);
            queryContext.Setup(mock => mock.FindKlantcontactenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new KlantcontactResults());
            queryContext.Setup(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>())).ReturnsAsync(GetCreatedKlantcontact());
            queryContext.Setup(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>())).ReturnsAsync(HttpRequestResponse.Success("{}"));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                ProductReferenceFromReceipt(), NotifyMethods.Email, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                queryContext.Verify(mock => mock.FindKlantcontactenAsync(
                    s_productReference.PartyId, s_productReference.ProductId.ToString(), It.IsAny<string>()), Times.Once);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(
                    It.Is<string>(json => json.Contains($"\"notifyNotificationId\":\"{s_notifyNotificationId}\""))), Times.Once);
                queryContext.Verify(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>()), Times.Once);
            });
        }

        [Test]
        public async Task ReportProductCompletionAsync_RetriedReceipt_ActorMissing_OnlyLinksTheActor()
        {
            // Arrange: the earlier attempt created the klantcontact and then failed to link the actor
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);
            queryContext.Setup(mock => mock.FindKlantcontactenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Found(s_notifyNotificationId.ToString(), hasActor: false, _configuration.OMC.Actor.Id()));
            queryContext.Setup(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>())).ReturnsAsync(HttpRequestResponse.Success("{}"));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                ProductReferenceFromReceipt(), NotifyMethods.Email, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>()), Times.Never);
                queryContext.Verify(mock => mock.LinkActorToContactMomentAsync(
                    It.Is<string>(json => json.Contains(s_existingKlantcontactId.ToString()))), Times.Once);
            });
        }

        [Test]
        public async Task ReportProductCompletionAsync_RetriedReceipt_AlreadyComplete_WritesNothing()
        {
            // Arrange: a duplicate receipt for a klantcontact that is already complete
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);
            queryContext.Setup(mock => mock.FindKlantcontactenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Found(s_notifyNotificationId.ToString(), hasActor: true, _configuration.OMC.Actor.Id()));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                ProductReferenceFromReceipt(), NotifyMethods.Email, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>()), Times.Never);
                queryContext.Verify(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task ReportProductCompletionAsync_OnlyAKlantcontactOfAnotherReceipt_CreatesANewOne()
        {
            // Arrange
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);
            queryContext.Setup(mock => mock.FindKlantcontactenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Found(Guid.NewGuid().ToString(), hasActor: true, _configuration.OMC.Actor.Id()));
            queryContext.Setup(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>())).ReturnsAsync(GetCreatedKlantcontact());
            queryContext.Setup(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>())).ReturnsAsync(HttpRequestResponse.Success("{}"));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                ProductReferenceFromReceipt(), NotifyMethods.Email, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>()), Times.Once);
            });
        }

        [Test]
        public async Task ReportProductCompletionAsync_LookupFails_ReportsFailure_WithoutCreating()
        {
            // Arrange: not knowing is no reason to write a possibly second klantcontact; the receipt is retried
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);
            queryContext.Setup(mock => mock.FindKlantcontactenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("OpenKlant unavailable"));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                ProductReferenceFromReceipt(), NotifyMethods.Email, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsFailure, Is.True);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task ReportProductCompletionAsync_NoNotifyId_WritesWithoutLookingUp()
        {
            // Arrange: a klantcontact for a send that never reached Notify NL has no receipt to repeat
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);
            queryContext.Setup(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>())).ReturnsAsync(GetCreatedKlantcontact());
            queryContext.Setup(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>())).ReturnsAsync(HttpRequestResponse.Success("{}"));

            // Act
            await service.ReportProductCompletionAsync(s_productReference, NotifyMethods.Email, "Subject", "Body", "false");

            // Assert
            Assert.Multiple(() =>
            {
                queryContext.Verify(mock => mock.FindKlantcontactenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(
                    It.Is<string>(json => !json.Contains("notifyNotificationId"))), Times.Once);
            });
        }

        [Test]
        public async Task ReportPrintCompletionAsync_RetriedReceipt_BijlageMissing_OnlyAttachesIt()
        {
            // Arrange: the earlier attempt created and linked the klantcontact, then failed to attach the PDF
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);
            queryContext.Setup(mock => mock.FindKlantcontactenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Found(s_notifyNotificationId.ToString(), hasActor: true, _configuration.OMC.Actor.Id(), hasAttachment: false));
            queryContext.Setup(mock => mock.CreateBijlageAsync(It.IsAny<string>())).ReturnsAsync(HttpRequestResponse.Success("{}"));

            var reference = new PrintNotifyReference
            {
                ObjectId = Guid.NewGuid(),
                PartyId = Guid.NewGuid(),
                AttachmentId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                NotifyNotificationId = s_notifyNotificationId
            };

            // Act
            HttpRequestResponse response = await service.ReportPrintCompletionAsync(reference, NotifyMethods.Letter, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>()), Times.Never);
                queryContext.Verify(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>()), Times.Never);
                queryContext.Verify(mock => mock.CreateBijlageAsync(
                    It.Is<string>(json => json.Contains(s_existingKlantcontactId.ToString()))), Times.Once);
            });
        }

        [Test]
        public void GetMessageBoxContactMomentJsonBody_FromAReceipt_CarriesTheNotifyIdInItsMetadata()
        {
            var reference = new MessageBoxNotifyReference { NotifyNotificationId = s_notifyNotificationId };

            string actualResult = CreateService().GetMessageBoxContactMomentJsonBody(reference, NotifyMethods.Mobb, ["Subject", "Body", "true"]);

            Assert.That(actualResult, Does.Contain($"\"metadata\":{{\"notifyNotificationId\":\"{s_notifyNotificationId}\"}}"));
        }
        #endregion

        [Test]
        public async Task ReportProductCompletionAsync_CreatesTheKlantcontact_LinksTheActor_AndSucceeds()
        {
            // Arrange
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);

            queryContext.Setup(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>())).ReturnsAsync(GetCreatedKlantcontact());
            queryContext.Setup(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>())).ReturnsAsync(HttpRequestResponse.Success("{}"));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                s_productReference, NotifyMethods.Email, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                queryContext.Verify(mock => mock.CreateNewContactMomentAsync(
                    It.Is<string>(json => json.Contains(s_productReference.ProductId.ToString()))), Times.Once);
                queryContext.Verify(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>()), Times.Once);
            });
        }

        [Test]
        public async Task ReportProductCompletionAsync_LinkingTheActorFails_ReportsFailure()
        {
            // Arrange
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);

            queryContext.Setup(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>())).ReturnsAsync(GetCreatedKlantcontact());
            queryContext.Setup(mock => mock.LinkActorToContactMomentAsync(It.IsAny<string>())).ReturnsAsync(HttpRequestResponse.Failure("link refused"));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                s_productReference, NotifyMethods.Email, "Subject", "Body", "true");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsFailure, Is.True);
                Assert.That(response.JsonResponse, Is.EqualTo("link refused"));
            });
        }

        [TestCase("duplicate key value violates unique constraint", "Duplicate key conflict in OpenKlant API")]
        [TestCase("OpenKlant is down", "OpenKlant is down")]
        public async Task ReportProductCompletionAsync_CreatingTheKlantcontactThrows_ReportsFailureInsteadOfThrowing(
            string exceptionMessage, string expectedMessage)
        {
            // NOTE: The product flow registers failures mid-delivery; a throw here must not escape and stop the
            //       remaining owners, so it has to come back as a failure response.

            // Arrange
            (ITelemetryService service, Mock<IQueryContext> queryContext) = CreateServiceWithQueryContext(_configuration);

            queryContext.Setup(mock => mock.CreateNewContactMomentAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException(exceptionMessage));

            // Act
            HttpRequestResponse response = await service.ReportProductCompletionAsync(
                s_productReference, NotifyMethods.Email, "Subject", "Body", "false");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsFailure, Is.True);
                Assert.That(response.JsonResponse, Is.EqualTo(expectedMessage));
            });
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
