// © 2026, Worth Systems.

using Common.Settings.Configuration;
using System.Text.Json;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenVtb;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenVtb.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenVtb" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    public interface IQueryVtb : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="OmcConfiguration"/>
        protected internal OmcConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenVtb";

        #region Abstract (Messages data)
        /// <summary>
        /// Gets the details of a specific Message (Vtb 'Berichten') from "OpenVtb" Web API service.
        /// </summary>
        /// <remarks>
        ///   The method used to obtain Message data.
        /// </remarks>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="vtbMessageUri">he <see cref="Uri"/> to get the specific VtbMessageData.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<VtbMessage> TryGetVtbMessageAsync(IQueryBase queryBase, Uri vtbMessageUri);
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.ZGW.Endpoint.OpenVtb();
        #endregion
    }
}
