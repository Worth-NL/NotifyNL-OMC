// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Settings.Extensions;
using Common.Versioning.Interfaces;
using Common.Versioning.Models;
using EventsHandler.Properties;

namespace EventsHandler.Versioning
{
    /// <inheritdoc cref="IVersionRegister"/>
    public sealed class OmcVersionRegister : IVersionRegister
    {
        private readonly WebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="OmcVersionRegister"/> class.
        /// </summary>
        public OmcVersionRegister(WebApiConfiguration configuration)  // Dependency Injection (DI)
        {
            this._configuration = configuration;
        }

        /// <inheritdoc cref="IVersionRegister.GetVersion(string)"/>
        /// <remarks>
        /// NOTE: This implementation is handling version of the main OMC application.
        /// </remarks>
        string IVersionRegister.GetVersion(string componentsVersions)
        {
            return string.Format(ApiResources.Endpoint_Events_Version_INFO_OmcVersionsSummary,
                // Output Management Component (OMC)
                /* {0} */ ApiResources.Application_Name,
                /* {1} */ OmcVersion.GetExpandedVersion(),
                /* {2} */ Environment.GetEnvironmentVariable(ConfigExtensions.AspNetCoreEnvironment),
                /* {3} */ this._configuration.OMC.Feature.Workflow_Version(),
 
                // ZHV (Open Services)
                /* {4} */ componentsVersions);
        }
    }
}