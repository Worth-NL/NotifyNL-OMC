// © 2023, Worth Systems.

using EventsHandler.Services.DataSending.Responses;

namespace EventsHandler.Services.DataSending.Interfaces
{
    /// <summary>
    /// The service responsible for transmission of data to the other external service.
    /// </summary>
    public interface INotifyService<in TPackage> : IDisposable
        where TPackage : struct
    {
        /// <summary>
        /// Sends the package of data over e-mail method.
        /// </summary>
        /// <param name="package">The package of data to be sent.</param>
        /// <returns>
        ///   Standardized response from the Web API service.
        /// </returns>
        internal Task<NotifySendResponse> SendEmailAsync(TPackage package);

        /// <summary>
        /// Sends the package of data over SMS method.
        /// </summary>
        /// <param name="package">The package of data to be sent.</param>
        /// <returns>
        ///   Standardized response from the Web API service.
        /// </returns>
        internal Task<NotifySendResponse> SendSmsAsync(TPackage package);

        /// <summary>
        /// Generates template preview from a given package.
        /// </summary>
        /// <param name="package">The package of data to be used.</param>
        /// <returns>
        ///   Standardized response from the Web API service.
        /// </returns>
        internal Task<NotifyTemplateResponse> GenerateTemplatePreviewAsync(TPackage package);
    }
}