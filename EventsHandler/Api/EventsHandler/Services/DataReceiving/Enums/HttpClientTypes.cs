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
        /// The <see cref="HttpClient"/> used to obtain data from "OpenZaak" v1+ Web API service.
        /// </summary>
        /// <remarks>
        /// Authorization: JSON Web Token.
        /// </remarks>
        OpenZaak_v1 = 1,
        #endregion

        #region OpenKlant
        /// <summary>
        /// The <see cref="HttpClient"/> used to obtain data from "OpenKlant" v1+ Web API service.
        /// </summary>
        /// <remarks>
        /// Authorization: JSON Web Token.
        /// </remarks>
        OpenKlant_v1 = 11,

        /// <summary>
        /// The <see cref="HttpClient"/> used to obtain data from "OpenKlant" v2+ Web API service.
        /// </summary>
        /// <remarks>
        /// Authorization: Static API key.
        /// </remarks>
        OpenKlant_v2 = 12,
        #endregion

        #region Objecten
        /// <summary>
        /// The <see cref="HttpClient"/> used to obtain data from "Objecten" v2+ Web API service.
        /// </summary>
        /// <remarks>
        /// Authorization: Static API key.
        /// </remarks>
        Objecten = 22,
        #endregion

        #region Telemetry
        // NOTE: "OpenZaak" v1+ is part of the "OMC workflow" v1, but those version numbers
        //       are not synonymous. For example, "OMC workflow" v2 includes "OpenKlant" v2+
        //       but also "OpenZaak" v1+. For more details check "OMC - Documentation.md".

        /// <summary>
        /// The <see cref="HttpClient"/> used for feedback and telemetry purposes in "OMC workflow" v1.
        /// </summary>
        /// <remarks>
        /// Authorization: JSON Web Token.
        /// </remarks>
        Telemetry_Contactmomenten = 41,
        
        /// <summary>
        /// The <see cref="HttpClient"/> used for feedback and telemetry purposes in "OMC workflow" v2.
        /// </summary>
        /// <remarks>
        /// Authorization: Static API key.
        /// </remarks>
        Telemetry_Klantinteracties = 42
        #endregion
    }
}