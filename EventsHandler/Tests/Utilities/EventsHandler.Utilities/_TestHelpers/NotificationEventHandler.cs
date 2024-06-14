// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Constants;
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
        internal const string TestOrganization = "286130270";

        // Regulars
        internal const string Regular_FirstProperty  = "kanaal";
        internal const string Regular_SecondProperty = "bronorganisatie";

        private const Channels Regular_FirstValue       = Channels.Objects;
        internal const string  Regular_FirstCustomValue = "objecten";
        internal const string  Regular_SecondValue      = "123456789";

        // Orphans
        internal const string Orphan_FirstProperty  = "orphan_1";
        internal const string Orphan_SecondProperty = "orphan_2";
        internal const string Orphan_ThirdProperty  = "orphan_3";

        internal const int    Orphan_FirstValue  = 1;
        private const bool    Orphan_SecondValue = false;
        internal const string Orphan_ThirdValue  = "maybe";
        #endregion

        #region Notifications
        internal static NotificationEvent GetNotification_Test_WithOrphans_ManuallyCreated()
        {
            // CASE #1: Manual creation of NotificationEvent
            NotificationEvent testNotification = new()
            {
                // Root level of NotificationEvent class
                Orphans = new Dictionary<string, object>
                {
                    { Orphan_FirstProperty, Orphan_FirstValue }
                },
                Attributes = new EventAttributes
                {
                    // Nested level of EventAttributes subclass
                    Orphans = new Dictionary<string, object>
                    {
                        { Orphan_SecondProperty, Orphan_SecondValue },
                        { Orphan_ThirdProperty,  Orphan_ThirdValue }
                    }
                }
            };

            return testNotification;
        }

        internal static NotificationEvent GetNotification_Test_WithOrphans_DynamicallyDeserialized()
        {
            string jsonPayload =
                $"{{\r\n" +
                  $"\"actie\": \"create\", \r\n" +
                  $"\"kanaal\": \"zaken\", \r\n" +
                  $"\"resource\": \"zaak\", \r\n" +
                  $"\"kenmerken\": {{\r\n" +
                    // Cases
                    $"\"zaaktype\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                    $"\"bronorganisatie\": \"000000000\", \r\n" +
                    $"\"vertrouwelijkheidaanduiding\": \"vertrouwelijk\", \r\n" +
                    // Objects
                    $"\"objectType\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                    // Decisions
                    $"\"besluittype\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                    $"\"verantwoordelijkeOrganisatie\": \"000000000\", \r\n" +
                    // Orphans (attributes)
                    $"\"{Orphan_SecondProperty}\": {GetOrphanSecondValue()}, \r\n" +  // Unexpected JSON property => will be moved to nested Orphans
                    $"\"{Orphan_ThirdProperty}\": \"{Orphan_ThirdValue}\"\r\n" +     // Unexpected JSON property => will be moved to nested Orphans
                  $"}}, \r\n" +
                  $"\"hoofdObject\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                  $"\"resourceUrl\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                  $"\"aanmaakdatum\": \"2023-09-12T13:58:29.9237316Z\", \r\n" +
                  // Orphans (event)
                  $"\"{Orphan_FirstProperty}\": {Orphan_FirstValue}\r\n" +  // Unexpected JSON property => will be moved to root Orphans
                $"}}";

            return JsonSerializer.Deserialize<NotificationEvent>(jsonPayload);
        }

        internal static NotificationEvent GetNotification_Test_WithRegulars_ChannelAndSourceOrganization()
        {
            return new NotificationEvent
            {
                Channel = Regular_FirstValue,
                Attributes = new EventAttributes
                {
                    SourceOrganization = Regular_SecondValue
                }
            };
        }

        internal static NotificationEvent GetNotification_Test_WithMixed_RegularAndOrphans_Properties()
        {
            NotificationEvent mixedNotification = GetNotification_Test_WithOrphans_ManuallyCreated();

            // Update attributes
            EventAttributes updatedAttributes = mixedNotification.Attributes;
            updatedAttributes.SourceOrganization = Regular_SecondValue;

            // Update notification
            mixedNotification.Channel = Regular_FirstValue;
            mixedNotification.Attributes = updatedAttributes;

            return mixedNotification;
        }

        internal static NotificationEvent GetNotification_Real_TheHague()
        {
            string jsonPayload =
                $"{{\r\n" +
                  $"\"actie\": \"create\", \r\n" +
                  $"\"kanaal\": \"zaken\", \r\n" +
                  $"\"resource\": \"status\", \r\n" +
                  $"\"kenmerken\": {{\r\n" +
                    // Cases
                    $"\"zaaktype\": \"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\", \r\n" +
                    $"\"bronorganisatie\": \"{TestOrganization}\", \r\n" +
                    $"\"vertrouwelijkheidaanduiding\": \"openbaar\", \r\n" +
                    // Objects
                    $"\"objectType\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                    // Decisions
                    $"\"besluittype\": \"{DefaultValues.Models.EmptyUri}\", \r\n" +
                    $"\"verantwoordelijkeOrganisatie\": \"{TestOrganization}\"\r\n" +
                  $"}}, \r\n" +
                  $"\"hoofdObject\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\", \r\n" +
                  $"\"resourceUrl\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\", \r\n" +
                  $"\"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"\r\n" +
                $"}}";

            return JsonSerializer.Deserialize<NotificationEvent>(jsonPayload);
        }
        #endregion

        internal static string GetOrphanSecondValue()
        {
            return Orphan_SecondValue.ToString().ToLower();
        }
    }
}