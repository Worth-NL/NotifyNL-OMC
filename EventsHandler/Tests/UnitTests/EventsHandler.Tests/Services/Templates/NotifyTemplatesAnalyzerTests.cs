// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.Templates;
using EventsHandler.Services.Templates.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Notify.Models.Responses;
using System.Text.Json;

namespace EventsHandler.UnitTests.Services.Templates
{
    [TestFixture]
    public sealed class NotifyTemplatesAnalyzerTests
    {
        private ITemplatesService<TemplateResponse, NotificationEvent>? _templatesService;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._templatesService = new NotifyTemplatesAnalyzer();
        }

        #region GetPlaceholders()
        private const string SimpleSubject = nameof(SimpleSubject);
        private const string SimpleBody = nameof(SimpleBody);

        private const string MissingEnd1 = "Missing ((end";
        private const string MissingEnd2 = "Missing ((end)";
        private const string MissingStart1 = "Missing start))";
        private const string MissingStart2 = "Missing (start))";
        private const string SingleBrackets = "Almost (template)";
        private const string EmptySingleBrackets = "()";
        private const string EmptyDoubleBrackets = "(())";

        private const string TestTemplateSubject = "\"Case ((identificatie)) for ((naam))\"";
        private const string TestTemplateBody = "\"Geachte ((aanhef)) ((achternaam)),\\r\\n\\r\\nEr is een nieuwe zaak aangemaakt bij de gemeente Den Haag " +
            "met als details:\\r\\n\\r\\nIdentificatienummer: ((identificatie))\\r\\nBronorganisatienummer: ((bronorganisatie))\\r\\nRegistratiedatum: " +
            "((registratiedatum))\\r\\nStartdatum: ((startdatum))\\r\\n\\r\\nVoor verdere vragen kunt u contact opnemen met ((contactpersoon)).\\r\\n\\r\\n" +
            "Met vriendelijke groet,\\r\\nDe gemeente Den Haag\"";

        // Subject is invalid
        [TestCase("", SimpleBody)]
        [TestCase(" ", SimpleBody)]
        // Body is invalid
        [TestCase(SimpleSubject, "")]
        [TestCase(SimpleSubject, " ")]
        public void GetPlaceholders_FromTemplate_SubjectOrBody_Invalid_Returns_0_Placeholders(string testSubject, string testBody)
        {
            // Arrange
            TemplateResponse testTemplate = new()
            {
                subject = testSubject,
                body = testBody
            };

            // Act
            string[] actualPlaceholders = this._templatesService!.GetPlaceholders(testTemplate);

            // Assert
            Assert.That(actualPlaceholders, Is.Empty);
        }

        [TestCase(SimpleSubject, SimpleBody)]
        // Subject
        [TestCase(MissingEnd1, SimpleBody)]
        [TestCase(MissingEnd2, SimpleBody)]
        [TestCase(MissingStart1, SimpleBody)]
        [TestCase(MissingStart2, SimpleBody)]
        [TestCase(SingleBrackets, SimpleBody)]
        [TestCase(EmptySingleBrackets, SimpleBody)]
        [TestCase(EmptyDoubleBrackets, SimpleBody)]
        // Body
        [TestCase(SimpleSubject, MissingEnd1)]
        [TestCase(SimpleSubject, MissingEnd2)]
        [TestCase(SimpleSubject, MissingStart1)]
        [TestCase(SimpleSubject, MissingStart2)]
        [TestCase(SimpleSubject, SingleBrackets)]
        [TestCase(SimpleSubject, EmptySingleBrackets)]
        [TestCase(SimpleSubject, EmptyDoubleBrackets)]
        public void GetPlaceholders_FromTemplate_SubjectAndBody_Valid_ButNoPlaceholders_Returns_0_Placeholders(string simpleSubject, string simpleBody)
        {
            // Arrange
            TemplateResponse testTemplate = new()
            {
                subject = simpleSubject,
                body = simpleBody
            };

            // Act
            string[] actualPlaceholders = this._templatesService!.GetPlaceholders(testTemplate);

            // Assert
            Assert.That(actualPlaceholders, Is.Empty);
        }

        [Test]
        public void GetPlaceholders_FromTemplate_Subject_Valid_WithPlaceholders_0_Duplicates_Returns_2_ExpectedPlaceholders()
        {
            // Arrange
            TemplateResponse testTemplate = new()
            {
                subject = TestTemplateSubject,
                body = SimpleBody
            };

            // Act
            string[] actualPlaceholders = this._templatesService!.GetPlaceholders(testTemplate);

            // Assert
            string actualCombinedPlaceholders = string.Join("|", actualPlaceholders);
            const string expectedCombinedPlaceholders = "identificatie|naam";

            Assert.Multiple(() =>
            {
                Assert.That(actualPlaceholders, Has.Length.EqualTo(2));
                Assert.That(actualCombinedPlaceholders, Is.EqualTo(expectedCombinedPlaceholders));
            });
        }

        [Test]
        public void GetPlaceholders_FromTemplate_Body_Valid_WithPlaceholders_0_Duplicates_Returns_7_ExpectedPlaceholders()
        {
            // Arrange
            TemplateResponse testTemplate = new()
            {
                subject = SimpleSubject,
                body = TestTemplateBody
            };

            // Act
            string[] actualPlaceholders = this._templatesService!.GetPlaceholders(testTemplate);

            // Assert
            string actualCombinedPlaceholders = string.Join("|", actualPlaceholders);
            const string expectedCombinedPlaceholders = "aanhef|achternaam|identificatie|bronorganisatie|registratiedatum|startdatum|contactpersoon";

            Assert.Multiple(() =>
            {
                Assert.That(actualPlaceholders, Has.Length.EqualTo(7));
                Assert.That(actualCombinedPlaceholders, Is.EqualTo(expectedCombinedPlaceholders));
            });
        }

        [Test]
        public void GetPlaceholders_FromTemplate_SubjectAndBody_Valid_WithPlaceholders_1_Duplicate_Returns_8_Not_9_ExpectedPlaceholders()
        {
            // Arrange
            TemplateResponse testTemplate = new()
            {
                subject = TestTemplateSubject,
                body = TestTemplateBody
            };

            // Act
            string[] actualPlaceholders = this._templatesService!.GetPlaceholders(testTemplate);

            // Assert
            string actualCombinedPlaceholders = string.Join("|", actualPlaceholders);
            const string expectedCombinedPlaceholders = "identificatie|naam|aanhef|achternaam|bronorganisatie|registratiedatum|startdatum|contactpersoon";

            Assert.Multiple(() =>
            {
                Assert.That(actualPlaceholders, Has.Length.EqualTo(8));
                Assert.That(actualCombinedPlaceholders, Is.EqualTo(expectedCombinedPlaceholders));
            });
        }
        #endregion

        #region MapPersonalization()
        [TestCaseSource(nameof(GetNotifications_WithOrphans))]
        public void MapPersonalization_ForGivenPlaceholders_AndNotification_ReturnsExpectedPersonalization(TestCase testCase)
        {
            // Act
            Dictionary<string, dynamic> actualPersonalization =
                this._templatesService!.MapPersonalization(testCase.Placeholders, testCase.Notification);

            // Assert
            string serializedActualPersonalization = JsonSerializer.Serialize(actualPersonalization);

            Assert.Multiple(() =>
            {
                TestContext.Out.Write(testCase.Description);

                Assert.That(actualPersonalization, Has.Count.EqualTo(testCase.Placeholders.Length));
                Assert.That(serializedActualPersonalization, Is.EqualTo(testCase.SerializedExpectedPersonalization));
            });
        }

        /// <summary>
        /// Emits test <see cref="NotificationEvent"/>s.
        /// </summary>
        private static IEnumerable<TestCase> GetNotifications_WithOrphans()
        {
            // Test 1: Empty
            yield return new TestCase
            {
                Description = "0 Placeholders with default Notification returns 0 Personalizations",
                Placeholders = Array.Empty<string>(),
                Notification = default,
                SerializedExpectedPersonalization = "{}"
            };

            const string notExistingProperty1 = "missing1";
            const string notExistingProperty2 = "missing2";

            // Test 2a: Orphan properties
            yield return new TestCase
            {
                Description = "3 mappable Placeholders used with manually created Notification having 3 matching properties (Orphans) returns 3 expected Personalizations",
                Placeholders = new[]
                {
                    NotificationEventHandler.Orphan_Test_Property_1,  // Matching placeholder (the POCO model has this property) => root
                    NotificationEventHandler.Orphan_Test_Property_2,  // Matching placeholder (the POCO model has this property) => nested
                    NotificationEventHandler.Orphan_Test_Property_3   // Matching placeholder (the POCO model has this property) => nested
                },
                Notification = NotificationEventHandler.GetNotification_Test_EmptyAttributes_WithOrphans(),
                SerializedExpectedPersonalization =
                $"{{" +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_1}\":{NotificationEventHandler.Orphan_Test_Value_1}," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_2}\":{NotificationEventHandler.GetOrphanSecondValue()}," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_3}\":\"{NotificationEventHandler.Orphan_Test_Value_3}\"" +
                $"}}"
            };
            // Test 2b: Orphan properties
            yield return new TestCase
            {
                Description = "3 mappable Placeholders used with dynamically deserialized Notification having 3 matching properties (Orphans) returns 3 expected Personalizations",
                Placeholders = new[]
                {
                    NotificationEventHandler.Orphan_Test_Property_1,  // Matching placeholder (the POCO model has this property) => root
                    NotificationEventHandler.Orphan_Test_Property_2,  // Matching placeholder (the POCO model has this property) => nested
                    NotificationEventHandler.Orphan_Test_Property_3   // Matching placeholder (the POCO model has this property) => nested
                },
                Notification = NotificationEventHandler.GetNotification_Test_AllAttributes_WithOrphans().Deserialized(),
                SerializedExpectedPersonalization =
                $"{{" +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_1}\":{NotificationEventHandler.Orphan_Test_Value_1}," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_2}\":{NotificationEventHandler.GetOrphanSecondValue()}," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_3}\":\"{NotificationEventHandler.Orphan_Test_Value_3}\"" +
                $"}}"
            };
            // Test 3: Orphan properties
            yield return new TestCase
            {
                Description = "2 unmappable Placeholders used with Notification having 3 unmatching properties (Orphans) returns 2 default Personalizations",
                Placeholders = new[]
                {
                    notExistingProperty1,  // Unmatching placeholder (the POCO model doesn't have such property)
                    notExistingProperty2   // Unmatching placeholder (the POCO model doesn't have such property)
                },
                Notification = NotificationEventHandler.GetNotification_Test_EmptyAttributes_WithOrphans(),
                SerializedExpectedPersonalization =
                $"{{" +
                  $"\"{notExistingProperty1}\":\"{NotifyTemplatesAnalyzer.ValueNotAvailable}\"," +
                  $"\"{notExistingProperty2}\":\"{NotifyTemplatesAnalyzer.ValueNotAvailable}\"" +
                $"}}"
            };
            // Test 4: Orphan properties
            yield return new TestCase
            {
                Description = "3 Placeholders (1 unmappable, 2 mappable) used with Notification having 3 properties " +
                              "(1 unmatching and 2 matching Orphans) returns 3 Personalizations (1 default, 2 expected)",
                Placeholders = new[]
                {
                    notExistingProperty1,                             // Unmatching placeholder (the POCO model doesn't have such property)
                    NotificationEventHandler.Orphan_Test_Property_1,  // Matching placeholder (the POCO model has this property) => root
                    NotificationEventHandler.Orphan_Test_Property_2   // Matching placeholder (the POCO model has this property) => nested
                },
                Notification = NotificationEventHandler.GetNotification_Test_EmptyAttributes_WithOrphans(),
                SerializedExpectedPersonalization =
                $"{{" +
                  $"\"{notExistingProperty1}\":\"{NotifyTemplatesAnalyzer.ValueNotAvailable}\"," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_1}\":{NotificationEventHandler.Orphan_Test_Value_1}," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_2}\":{NotificationEventHandler.GetOrphanSecondValue()}" +
                $"}}"
            };

            // Test 5: Regular properties
            yield return new TestCase
            {
                Description = "2 Placeholders (2 mappable) used with Notification having 2 matching properties (regular) returns 2 expected Personalizations",
                Placeholders = new[]
                {
                    NotificationEventHandler.Regular_Real_Property_Channel,            // Matching placeholder (the POCO model has this property) => root
                    NotificationEventHandler.Regular_Real_Property_SourceOrganization  // Matching placeholder (the POCO model has this property) => nested
                },
                Notification = NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization(),
                SerializedExpectedPersonalization =
                $"{{" +
                  $"\"{NotificationEventHandler.Regular_Real_Property_Channel}\":\"{NotificationEventHandler.Regular_Real_Value_Channel_String}\"," +
                  $"\"{NotificationEventHandler.Regular_Real_Property_SourceOrganization}\":\"{NotificationEventHandler.Regular_Test_Value_SourceOrganization}\"" +
                $"}}"
            };
            // Test 6: Regular properties
            yield return new TestCase
            {
                Description = "3 Placeholders (1 unmappable, 2 mappable) used with Notification having 2 matching properties (regular) returns 2 Personalizations (1 default, 2 expected)",
                Placeholders = new[]
                {
                    notExistingProperty1,                                              // Unmatching placeholder (the POCO model doesn't have such property)
                    NotificationEventHandler.Regular_Real_Property_Channel,            // Matching placeholder (the POCO model has this property) => root
                    NotificationEventHandler.Regular_Real_Property_SourceOrganization  // Matching placeholder (the POCO model has this property) => nested
                },
                Notification = NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization(),
                SerializedExpectedPersonalization =
                $"{{" +
                  $"\"{notExistingProperty1}\":\"{NotifyTemplatesAnalyzer.ValueNotAvailable}\"," +
                  $"\"{NotificationEventHandler.Regular_Real_Property_Channel}\":\"{NotificationEventHandler.Regular_Real_Value_Channel_String}\"," +
                  $"\"{NotificationEventHandler.Regular_Real_Property_SourceOrganization}\":\"{NotificationEventHandler.Regular_Test_Value_SourceOrganization}\"" +
                $"}}"
            };

            // Test 7: Mixed properties (orphan + regular)
            yield return new TestCase
            {
                Description = "7 Placeholders (2 unmappable, 5 mappable) used with Notification having 5 matching properties (2 regular, 3 Orphans) returns 7 Personalizations (2 default, 5 expected)",
                Placeholders = new[]
                {
                    notExistingProperty1,                             // Unmatching placeholder (the POCO model doesn't have such property)
                    notExistingProperty2,                             // Unmatching placeholder (the POCO model doesn't have such property)
                    // Regular                                      
                    NotificationEventHandler.Regular_Real_Property_Channel,             // Matching placeholder (the POCO model has this property) => root
                    NotificationEventHandler.Regular_Real_Property_SourceOrganization,  // Matching placeholder (the POCO model has this property) => nested
                    // Orphans                                      
                    NotificationEventHandler.Orphan_Test_Property_1,  // Matching placeholder (the POCO model has this property) => root
                    NotificationEventHandler.Orphan_Test_Property_2,  // Matching placeholder (the POCO model has this property) => nested
                    NotificationEventHandler.Orphan_Test_Property_3   // Matching placeholder (the POCO model has this property) => nested
                },
                Notification = NotificationEventHandler.GetNotification_Test_WithMixed_RegularAndOrphans_Properties(),
                SerializedExpectedPersonalization =
                $"{{" +
                  $"\"{notExistingProperty1}\":\"{NotifyTemplatesAnalyzer.ValueNotAvailable}\"," +
                  $"\"{notExistingProperty2}\":\"{NotifyTemplatesAnalyzer.ValueNotAvailable}\"," +
                  $"\"{NotificationEventHandler.Regular_Real_Property_Channel}\":\"{NotificationEventHandler.Regular_Real_Value_Channel_String}\"," +
                  $"\"{NotificationEventHandler.Regular_Real_Property_SourceOrganization}\":\"{NotificationEventHandler.Regular_Test_Value_SourceOrganization}\"," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_1}\":{NotificationEventHandler.Orphan_Test_Value_1}," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_2}\":{NotificationEventHandler.GetOrphanSecondValue()}," +
                  $"\"{NotificationEventHandler.Orphan_Test_Property_3}\":\"{NotificationEventHandler.Orphan_Test_Value_3}\"" +
                $"}}"
            };
        }

        public sealed class TestCase
        {
            internal string Description { get; init; } = string.Empty;

            internal string[] Placeholders { get; init; } = Array.Empty<string>();

            internal NotificationEvent Notification { get; init; }

            internal string SerializedExpectedPersonalization { get; init; } = string.Empty;
        }
        #endregion
    }
}