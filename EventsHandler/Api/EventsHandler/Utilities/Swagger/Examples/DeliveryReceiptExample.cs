// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotifyNL;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of delivery status received from "NotifyNL" API web service.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage]
    internal sealed class DeliveryReceiptExample : IExamplesProvider<DeliveryReceipt>
    {
        private const string SerializedNotification =
            $"{{\r\n" +
            $"  \"actie\": \"create\",\r\n" +
            $"  \"kanaal\": \"zaken\",\r\n" +
            $"  \"resource\": \"status\",\r\n" +
            $"  \"kenmerken\": {{\r\n" +
            $"    \"objectType\": \"http://0.0.0.0:0/\",\r\n" +
            $"    \"zaaktype\": \"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\",\r\n" +
            $"    \"bronorganisatie\": \"286130270\",\r\n" +
            $"    \"vertrouwelijkheidaanduiding\": \"openbaar\"\r\n" +
            $"  }},\r\n" +
            $"  \"hoofdObject\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\",\r\n" +
            $"  \"resourceUrl\": \"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\",\r\n" +
            $"  \"aanmaakdatum\": \"2023-09-22T11:41:46.052Z\"\r\n" +
            $"}}";

        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public DeliveryReceipt GetExamples()
        {
            DateTime currentTime = DateTime.UtcNow;

            return new DeliveryReceipt
            {
                Id = Guid.NewGuid(),
                Reference = SerializedNotification,
                Recipient = "hello@gov.nl",
                Status = DeliveryStatus.Delivered,
                CreatedAt = currentTime,
                CompletedAt = currentTime.Add(new TimeSpan(seconds: 1, hours: 0, minutes: 0)),
                SentAt = currentTime.Add(new TimeSpan(seconds: 2, hours: 0, minutes: 0)),
                Type = NotificationTypes.Email,
                TemplateId = Guid.NewGuid(),
                TemplateVersion = 1
            };
        }
    }
}