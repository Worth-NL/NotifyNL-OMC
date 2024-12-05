// © 2024, Worth Systems.

using Asp.Versioning;
using EventsHandler.Services.Versioning;

namespace EventsHandler.Attributes.Versioning
{
    /// <summary>
    /// The <see cref="ApiVersion"/> attribute used for OMC purposes.
    /// </summary>
    internal sealed class OmcVersionAttribute : ApiVersionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OmcVersionAttribute"/> class.
        /// </summary>
        internal OmcVersionAttribute() : base(OmcVersion.GetNetVersion())
        {
            // Satisfies API controllers with .NET version of the software (major.minor)
        }
    }
}