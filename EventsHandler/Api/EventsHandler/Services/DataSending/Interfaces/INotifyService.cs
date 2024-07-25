// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Exceptions;

namespace EventsHandler.Services.DataSending.Interfaces
{
    /// <summary>
    /// The service responsible for transmission of data to the other external service.
    /// </summary>
    public interface INotifyService<in TModel, in TPackage> : IDisposable
        where TModel : struct, IJsonSerializable
        where TPackage : struct
    {
        /// <summary>
        /// Sends the package of data over SMS method.
        /// </summary>
        /// <param name="model">The business model to be processed.</param>
        /// <param name="package">The package of data to be sent.</param>
        /// <exception cref="TelemetryException">The completion status could not be sent.</exception>
        internal Task SendSmsAsync(TModel model, TPackage package);

        /// <summary>
        /// Sends the package of data over e-mail method.
        /// </summary>
        /// <param name="model">The business model to be processed.</param>
        /// <param name="package">The package of data to be sent.</param>
        /// <exception cref="TelemetryException">The completion status could not be sent.</exception>
        internal Task SendEmailAsync(TModel model, TPackage package);
    }
}