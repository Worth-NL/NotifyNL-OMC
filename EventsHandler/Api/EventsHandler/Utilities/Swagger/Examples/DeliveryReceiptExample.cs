// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.NotifyNL;
using EventsHandler.Mapping.Models.POCOs.NotifyNL;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of delivery status received from "Notify NL" Web API service.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage(Justification = "This is example model used by Swagger UI; testing how third-party dependency is dealing with it is unnecessary.")]
    internal sealed class DeliveryReceiptExample : IExamplesProvider<DeliveryReceipt>
    {
        private const string SerializedNotification =
            $"{{" +
              $"{{" +
              $"  \"actie\":\"create\"," +
              $"  \"kanaal\":\"zaken\"," +
              $"  \"resource\":\"status\"," +
              $"  \"kenmerken\":{{" +
              $"    \"objectType\":\"http://0.0.0.0:0/\"," +
              $"    \"zaaktype\":\"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\"," +
              $"    \"bronorganisatie\":\"286130270\"," +
              $"    \"vertrouwelijkheidaanduiding\":\"openbaar\"" +
              $"  }}," +
              $"  \"hoofdObject\":\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\"," +
              $"  \"resourceUrl\":\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\"," +
              $"  \"aanmaakdatum\":\"2023-09-22T11:41:46.052Z\"" +
              $"}}," +
              $"\"caseUri\":\"http://0.0.0.0:0/\"," +
              $"\"partyUri\": \"http://0.0.0.0:0/\"" +
            $"}}";

        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public DeliveryReceipt GetExamples()
        {
            DateTime currentTime = DateTime.UtcNow;

            return new DeliveryReceipt
            {
                Id = Guid.NewGuid(),
                Reference = SerializedNotification.Base64Encode(),
                Recipient = "hello@gov.nl",
                Status = DeliveryStatuses.Delivered,
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