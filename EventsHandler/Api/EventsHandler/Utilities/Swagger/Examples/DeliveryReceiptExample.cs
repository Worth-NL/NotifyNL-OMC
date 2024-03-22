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
        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public DeliveryReceipt GetExamples()
        {
            DateTime currentTime = DateTime.UtcNow;

            return new DeliveryReceipt
            {
                Id = Guid.NewGuid(),
                Reference = "12345678",
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