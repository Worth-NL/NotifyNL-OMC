// © 2026, Worth Systems.

using WebQueries.BRP.Models;

namespace WebQueries.BRP
{
    /// <summary>
    /// The fixed address the Berichtenbox letter goes to instead of the BRP address when
    /// <c>BRP_GEBRUIK_TESTADRES</c> is switched on: Worth Systems in Rijswijk.
    /// </summary>
    /// <remarks>
    ///   Lets a test environment without a BRP connection send a real letter to a (test) printstraat. It holds no
    ///   personal data, so unlike a BRP address it may be traced and logged.
    /// </remarks>
    public static class BrpTestAddress
    {
        /// <summary>
        /// The test address, shaped like the "Adressering" BRP returns.
        /// </summary>
        public static Adressering Value { get; } = new()
        {
            Aanschrijfwijze = new Adressering.AanschrijfwijzeDetails { Naam = "Worth Systems t.a.v. OMC-test" },
            Adresregel1 = "Laan van Vredenoord 11",
            Adresregel2 = "2289 DA Rijswijk"
        };
    }
}
