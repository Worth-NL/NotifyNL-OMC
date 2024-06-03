// © 2023, Worth Systems.

namespace EventsHandler.Services.DataReceiving.Enums
{
    /// <summary>
    /// A specific types of <see cref="HttpClient"/>s used by internal business logic.
    /// </summary>
    internal enum HttpClientTypes
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The default value.
        /// </summary>
        Unknown = 0,

        #region OpenZaak
        /// <summary>
        /// The <see cref="HttpClient"/> used to obtain data from "OpenZaak" v1.0.0.
        /// </summary>
        /// <remarks>
        /// Authorization: JSON Web Token.
        /// </remarks>
        OpenZaak_v1 = 1,
        #endregion

        #region OpenKlant
        /// <summary>
        /// The <see cref="HttpClient"/> used to obtain data from "OpenKlant" v1.0.0.
        /// </summary>
        /// <remarks>
        /// Authorization: JSON Web Token.
        /// </remarks>
        OpenKlant_v1 = 11,

        /// <summary>
        /// The <see cref="HttpClient"/> used to obtain data from "OpenKlant" v2.0.0.
        /// </summary>
        /// <remarks>
        /// Authorization: Static token.
        /// </remarks>
        OpenKlant_v2 = 12,
        #endregion

        #region Telemetry
        // NOTE: "OpenZaak" v1.0.0 is part of the "OpenServices" v1 workflow, but those version numbers
        //       are not synonymous. For example, "OpenServices" v2 workflow includes "OpenKlant" v2.0.0
        //       but also "OpenZaak" v1.0.0. For more details check notes in QueryServiceResolver.

        /// <summary>
        /// The <see cref="HttpClient"/> used for feedback and telemetry purposes in "OpenServices" v1 workflow.
        /// </summary>
        /// <remarks>
        /// Authorization: JSON Web Token.
        /// </remarks>
        Telemetry_ContactMomenten = 21
        #endregion
    }
}