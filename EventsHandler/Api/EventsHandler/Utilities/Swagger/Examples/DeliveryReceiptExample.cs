﻿// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.NotifyNL;
using EventsHandler.Mapping.Models.POCOs.NotifyNL;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EventsHandler.Utilities.Swagger.Examples
{
    /// <summary>
    /// An example of delivery status received from "Notify NL" Web API service.
    /// </summary>
    /// <seealso cref="IExamplesProvider{T}"/>
    [ExcludeFromCodeCoverage(Justification = "This is example model used by Swagger UI; testing how third-party dependency is dealing with it is unnecessary.")]
    internal sealed class DeliveryReceiptExample : IExamplesProvider<DeliveryReceipt>
    {
        private static readonly string s_serializedNotification = JsonSerializer.Serialize(new NotifyReferenceExample().GetExamples());

        /// <inheritdoc cref="IExamplesProvider{TModel}.GetExamples"/>
        public DeliveryReceipt GetExamples()
        {
            DateTime currentTime = DateTime.UtcNow;

            return new DeliveryReceipt
            {
                Id = Guid.NewGuid(),
                Reference = Task.Run(async () => await s_serializedNotification.CompressGZipAsync(CancellationToken.None)).Result,
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