// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using System.Text.Json;

// ReSharper disable InconsistentNaming => allow constants with underscores (for readability)

namespace EventsHandler.Utilities._TestHelpers
{
    /// <summary>
    /// Collection of utility methods related to retrieving or manipulating of <see cref="NotificationEvent"/> class.
    /// </summary>
    internal static class NotificationEventHandler
    {
        #region Constants
        internal const string SourceOrganization_Real_TheHague = "286130270";
        internal const string ResponsibleOrganization_Real_TheHague = "999990639";

        // Regulars
        internal const string Regular_Real_Property_Channel = "kanaal";
        internal const string Regular_Real_Property_SourceOrganization = "bronorganisatie";

        private const Channels Regular_Real_Value_Channel_Enum = Channels.Objects;
        internal const string Regular_Real_Value_Channel_String = "objecten";
        internal const string Regular_Test_Value_SourceOrganization = "123456789";

        // Orphans
        internal const string Orphan_Test_Property_1 = "orphan_1";
        internal const string Orphan_Test_Property_2 = "orphan_2";
        internal const string Orphan_Test_Property_3 = "orphan_3";

        internal const int Orphan_Test_Value_1 = 1;
        internal const bool Orphan_Test_Value_2 = false;
        internal const string Orphan_Test_Value_3 = "maybe";
        #endregion

        #region Notifications (Test)
        internal static NotificationEvent GetNotification_Test_EmptyAttributes_WithOrphans_ManuallyCreated()
        {
            // CASE #1: Manual creation of NotificationEvent
            NotificationEvent testNotification = new()
            {
                // Root level of NotificationEvent class
                Orphans = new Dictionary<string, object>
                {
                    { Orphan_Test_Property_1, Orphan_Test_Value_1 }
                },
                Attributes = new EventAttributes
                {
                    // Nested level of EventAttributes subclass
                    Orphans = new Dictionary<string, object>
                    {
                        { Orphan_Test_Property_2, Orphan_Test_Value_2 },
                        { Orphan_Test_Property_3, Orphan_Test_Value_3 }
                    }
                }
            };

            return testNotification;
        }

        internal static string GetNotification_Test_AllAttributes_WithOrphans()
        {
            string jsonPayload =
                $"{{" +
                  $"\"actie\": \"create\", " +
                  $"\"kanaal\": \"zaken\", " +
                  $"\"resource\": \"zaak\", " +
                  $"\"kenmerken\": {{" +
                    // Cases
                    $"\"zaaktype\": \"{DefaultValues.Models.EmptyUri}\", " +
                    $"\"bronorganisatie\": \"{SourceOrganization_Real_TheHague}\", " +
                    "\"vertrouwelijkheidaanduiding\": 2, " +
                    // Objects
                    $"\"objectType\": \"{DefaultValues.Models.EmptyUri}\", " +
                    // Decisions
                    $"\"besluittype\": \"{DefaultValues.Models.EmptyUri}\", " +
                    $"\"verantwoordelijkeOrganisatie\": \"{ResponsibleOrganization_Real_TheHague}\", " +
                    // Orphans (attributes)
                    $"\"{Orphan_Test_Property_2}\": {GetOrphanSecondValue()}, " +  // Unexpected JSON property => will be moved to nested Orphans
                    $"\"{Orphan_Test_Property_3}\": \"{Orphan_Test_Value_3}\"" +      // Unexpected JSON property => will be moved to nested Orphans
                  $"}}, " +
                  $"\"hoofdObject\": \"{DefaultValues.Models.EmptyUri}\", " +
                  $"\"resourceUrl\": \"{DefaultValues.Models.EmptyUri}\", " +
                  $"\"aanmaakdatum\": \"2023-09-12T13:58:29.9237316Z\", " +
                  // Orphans (event)
                  $"\"{Orphan_Test_Property_1}\": {Orphan_Test_Value_1}" +  // Unexpected JSON property => will be moved to root Orphans
                $"}}";

            return jsonPayload;
        }

        internal static string GetNotification_Test_AllAttributes_Null_WithoutOrphans()
        {
            const string jsonPayload =
                $"{{" +
                  $"\"actie\": \"create\", " +
                  $"\"kanaal\": \"zaken\", " +
                  $"\"resource\": \"status\", " +
                  $"\"kenmerken\": {{" +
                    // Cases
                    $"\"zaaktype\": null, " +
                    $"\"bronorganisatie\": null, " +
                    $"\"vertrouwelijkheidaanduiding\": null, " +
                    // Objects
                    $"\"objectType\": null, " +
                    // Decisions
                    $"\"besluittype\": null, " +
                    $"\"verantwoordelijkeOrganisatie\": null" +
                  $"}}, " +
                  $"\"hoofdObject\": \"https://www.test.something.org/\", " +
                  $"\"resourceUrl\": \"https://www.test.something.org/\", " +
                  $"\"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"" +
                $"}}";

            return jsonPayload;
        }

        internal static NotificationEvent GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization_ManuallyCreated()
        {
            return new NotificationEvent
            {
                Channel = Regular_Real_Value_Channel_Enum,
                Attributes = new EventAttributes
                {
                    SourceOrganization = Regular_Test_Value_SourceOrganization
                }
            };
        }

        internal static NotificationEvent GetNotification_Test_WithMixed_RegularAndOrphans_Properties_ManuallyCreated()
        {
            NotificationEvent mixedNotification = GetNotification_Test_EmptyAttributes_WithOrphans_ManuallyCreated();

            // Update attributes
            EventAttributes updatedAttributes = mixedNotification.Attributes;
            updatedAttributes.SourceOrganization = Regular_Test_Value_SourceOrganization;

            // Update notification
            mixedNotification.Channel = Regular_Real_Value_Channel_Enum;
            mixedNotification.Attributes = updatedAttributes;

            return mixedNotification;
        }
        #endregion

        #region Notifications (Real)
        internal static string GetNotification_Real_CaseUpdateScenario_TheHague()
        {
            const string jsonPayload =
                $"{{" +
                  $"\"actie\": \"create\", " +
                  $"\"kanaal\": \"zaken\", " +
                  $"\"resource\": \"status\", " +
                  $"\"kenmerken\": {{" +
                    // Cases
                    $"\"zaaktype\": \"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\", " +
                    $"\"bronorganisatie\": \"{SourceOrganization_Real_TheHague}\", " +
                    $"\"vertrouwelijkheidaanduiding\": \"openbaar\"" +
                  $"}}, " +
                  $"\"hoofdObject\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\", " +
                  $"\"resourceUrl\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\", " +
                  $"\"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"" +
                $"}}";

            return jsonPayload;
        }
        internal static string GetNotification_Real_CaseCreateScenario_TheHague()
        {
            const string jsonPayload =
                $"{{" +
                  $"\"actie\": \"create\", " +
                  $"\"kanaal\": \"zaken\", " +
                  $"\"resource\": \"zaak\", " +
                  $"\"kenmerken\": {{" +
                    // Cases
                    $"\"zaaktype\": \"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\", " +
                    $"\"bronorganisatie\": \"{SourceOrganization_Real_TheHague}\", " +
                    $"\"vertrouwelijkheidaanduiding\": \"openbaar\"" +
                  $"}}, " +
                  $"\"hoofdObject\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\", " +
                  $"\"resourceUrl\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\", " +
                  $"\"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"" +
                $"}}";

            return jsonPayload;
        }

        internal static string GetNotification_Real_TaskAssignedScenario_TheHague()
        {
            const string jsonPayload =
                $"{{" +
                  $"\"actie\": \"create\", " +
                  $"\"kanaal\": \"objecten\", " +
                  $"\"resource\": \"object\", " +
                  $"\"kenmerken\": {{" +
                    // Objects
                    $"\"objectType\": \"https://objecttypen.test.denhaag.opengem.nl/api/v2/objecttypes/{ConfigurationHandler.TestTaskObjectTypeUuid}\"" +
                  $"}}, " +
                  $"\"hoofdObject\": \"https://objecten.test.denhaag.opengem.nl/api/v2/objects/fa3d63f4-4caf-4a30-97ab-b19cd753ab59\", " +
                  $"\"resourceUrl\": \"https://objecten.test.denhaag.opengem.nl/api/v2/objects/fa3d63f4-4caf-4a30-97ab-b19cd753ab59\", " +
                  $"\"aanmaakdatum\": \"2023-10-04T12:15:04.005Z\"" +
                $"}}";

            return jsonPayload;
        }

        internal static string GetNotification_Real_DecisionMadeScenario_TheHague()
        {
            const string jsonPayload =
                $"{{" +
                  $"\"actie\": \"create\", " +
                  $"\"kanaal\": \"besluiten\", " +
                  $"\"resource\": \"besluitinformatieobject\", " +
                  $"\"kenmerken\": {{" +
                    // Decisions
                    $"\"besluittype\": \"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/besluittypen/7002077e-0358-4301-8aac-5b440093f214\", " +
                    $"\"verantwoordelijkeOrganisatie\": \"{ResponsibleOrganization_Real_TheHague}\"" +
                  $"}}, " +
                  $"\"hoofdObject\": \"https://openzaak.test.denhaag.opengem.nl/besluiten/api/v1/besluiten/a5300781-943f-49e4-a6c2-c0ca4516936c\", " +
                  $"\"resourceUrl\": \"https://openzaak.test.denhaag.opengem.nl/besluiten/api/v1/besluiten/a5300781-943f-49e4-a6c2-c0ca4516936c\", " +
                  $"\"aanmaakdatum\": \"2023-10-05T08:52:02.273Z\"" +
                $"}}";

            return jsonPayload;
        }

        internal static string GetNotification_Real_MessageReceivedScenario_TheHague()
        {
            const string jsonPayload =
                $"{{" +
                $"\"actie\": \"create\", " +
                $"\"kanaal\": \"objecten\", " +
                $"\"resource\": \"object\", " +
                $"\"kenmerken\": {{" +
                // Objects
                $"\"objectType\": \"https://objecttypen.test.denhaag.opengem.nl/api/v2/objecttypes/{ConfigurationHandler.TestMessageObjectTypeUuid1}\"" +
                $"}}, " +
                $"\"hoofdObject\": \"https://objecten.test.denhaag.opengem.nl/api/v2/objects/bb2b870c-41f5-4c98-88f5-772aca9ed326\", " +
                $"\"resourceUrl\": \"https://objecten.test.denhaag.opengem.nl/api/v2/objects/bb2b870c-41f5-4c98-88f5-772aca9ed326\", " +
                $"\"aanmaakdatum\": \"2023-10-04T12:15:04.005Z\"" +
                $"}}";

            return jsonPayload;
        }
        #endregion

        internal static string GetOrphanSecondValue()
        {
            return Orphan_Test_Value_2.ToString().ToLower();
        }

        internal static NotificationEvent Deserialized(this string jsonPayload)
        {
            return JsonSerializer.Deserialize<NotificationEvent>(jsonPayload);
        }
    }
}