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
        internal static string GetNotification_Test_CasesScenario()
        {
            string jsonPayload =
                $"{{\r\n" +
                  $"\"actie\": \"create\", \r\n" +
                  $"\"kanaal\": \"zaken\", \r\n" +
                  $"\"resource\": \"status\", \r\n" +
                  $"\"kenmerken\": {{\r\n" +
                    // Cases
                    $"\"zaaktype\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                    $"\"bronorganisatie\": \"{DefaultValues.Models.DefaultOrganization}\", \r\n" +
                    $"\"vertrouwelijkheidaanduiding\": \"openbaar\"\r\n" +
                  $"}}, \r\n" +
                  $"\"hoofdObject\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                  $"\"resourceUrl\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                  $"\"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"\r\n" +
                $"}}";

            return jsonPayload;
        }

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
                $"{{\r\n" +
                  $"\"actie\": \"create\", \r\n" +
                  $"\"kanaal\": \"zaken\", \r\n" +
                  $"\"resource\": \"zaak\", \r\n" +
                  $"\"kenmerken\": {{\r\n" +
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
                    $"\"{Orphan_Test_Property_2}\": {GetOrphanSecondValue()}, \r\n" +  // Unexpected JSON property => will be moved to nested Orphans
                    $"\"{Orphan_Test_Property_3}\": \"{Orphan_Test_Value_3}\"\r\n" +      // Unexpected JSON property => will be moved to nested Orphans
                  $"}}, \r\n" +
                  $"\"hoofdObject\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                  $"\"resourceUrl\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                  $"\"aanmaakdatum\": \"2023-09-12T13:58:29.9237316Z\", \r\n" +
                  // Orphans (event)
                  $"\"{Orphan_Test_Property_1}\": {Orphan_Test_Value_1}\r\n" +  // Unexpected JSON property => will be moved to root Orphans
                $"}}";

            return jsonPayload;
        }

        internal static string GetNotification_Test_AllAttributes_Null_WithoutOrphans()
        {
            const string jsonPayload =
                $"{{\r\n" +
                  $"\"actie\": \"create\", \r\n" +
                  $"\"kanaal\": \"zaken\", \r\n" +
                  $"\"resource\": \"status\", \r\n" +
                  $"\"kenmerken\": {{\r\n" +
                    // Cases
                    $"\"zaaktype\": null, \r\n" +
                    $"\"bronorganisatie\": null, \r\n" +
                    $"\"vertrouwelijkheidaanduiding\": null, \r\n" +
                    // Objects
                    $"\"objectType\": null, \r\n" +
                    // Decisions
                    $"\"besluittype\": null, \r\n" +
                    $"\"verantwoordelijkeOrganisatie\": null\r\n" +
                  $"}}, \r\n" +
                  $"\"hoofdObject\": \"https://www.test.something.org/\", \r\n" +
                  $"\"resourceUrl\": \"https://www.test.something.org/\", \r\n" +
                  $"\"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"\r\n" +
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
        internal static string GetNotification_Real_CasesScenario_TheHague()
        {
            const string jsonPayload =
                $"{{\r\n" +
                  $"\"actie\": \"create\", \r\n" +
                  $"\"kanaal\": \"zaken\", \r\n" +
                  $"\"resource\": \"status\", \r\n" +
                  $"\"kenmerken\": {{\r\n" +
                    // Cases
                    $"\"zaaktype\": \"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\", \r\n" +
                    $"\"bronorganisatie\": \"{SourceOrganization_Real_TheHague}\", \r\n" +
                    $"\"vertrouwelijkheidaanduiding\": \"openbaar\"\r\n" +
                  $"}}, \r\n" +
                  $"\"hoofdObject\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\", \r\n" +
                  $"\"resourceUrl\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\", \r\n" +
                  $"\"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"\r\n" +
                $"}}";

            return jsonPayload;
        }

        internal static string GetNotification_Real_DecisionsScenario_TheHague()
        {
            const string jsonPayload =
                $"{{\r\n" +
                  $"\"actie\": \"create\", \r\n" +
                  $"\"kanaal\": \"besluiten\", \r\n" +
                  $"\"resource\": \"besluitinformatieobject\", \r\n" +
                  $"\"kenmerken\": {{\r\n" +
                    // Decisions
                    $"\"besluittype\": \"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/besluittypen/7002077e-0358-4301-8aac-5b440093f214\", \r\n" +
                    $"\"verantwoordelijkeOrganisatie\": \"{ResponsibleOrganization_Real_TheHague}\"\r\n" +
                  $"}}, \r\n" +
                  $"\"hoofdObject\": \"https://openzaak.test.denhaag.opengem.nl/besluiten/api/v1/besluiten/a5300781-943f-49e4-a6c2-c0ca4516936c\", \r\n" +
                  $"\"resourceUrl\": \"https://openzaak.test.denhaag.opengem.nl/besluiten/api/v1/besluiten/a5300781-943f-49e4-a6c2-c0ca4516936c\", \r\n" +
                  $"\"aanmaakdatum\": \"2023-10-05T08:52:02.273Z\"\r\n" +
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