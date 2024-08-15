// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Services.DataSending.Responses;

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
        /// <returns>
        ///   Standardized response from the Web API service.
        /// </returns>
        internal Task<NotifySendResponse> SendSmsAsync(TModel model, TPackage package);

        /// <summary>
        /// Sends the package of data over e-mail method.
        /// </summary>
        /// <param name="model">The business model to be processed.</param>
        /// <param name="package">The package of data to be sent.</param>
        /// <returns>
        ///   Standardized response from the Web API service.
        /// </returns>
        internal Task<NotifySendResponse> SendEmailAsync(TModel model, TPackage package);
    }
}