// © 2024, Worth Systems.

using Asp.Versioning;
using Common.Versioning.Models;

namespace EventsHandler.Attributes.Versioning
{
    /// <summary>
    /// The <see cref="ApiVersion"/> attribute used for OMC purposes.
    /// </summary>
    internal sealed class OmcApiVersionAttribute : ApiVersionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OmcApiVersionAttribute"/> class.
        /// </summary>
        internal OmcApiVersionAttribute() : base(OmcVersion.GetNetVersion())
        {
            // Satisfies API controllers with .NET version of the software (major.minor)
        }
    }
}