// © 2026, Worth Systems.

using ZgwModels.Enums;

namespace ZgwModels.Extensions
{
    /// <summary>
    /// The methods extending <see cref="NotifyMethods"/> enum.
    /// </summary>
    public static class NotifyMethodsExtensions
    {
        /// <summary>
        /// Converts a <see cref="NotifyMethods"/> into the value recorded as "kanaal" on a klantcontact.
        /// </summary>
        /// <remarks>
        ///   The enum's own name is used for every channel except letters, which are recorded as "brief":
        ///   that is what OpenKlant is expected to hold for post. The others are left exactly as they were -
        ///   "Mobb" in particular is already the established abbreviation for MijnOverheid Berichtenbox, and
        ///   changing what live flows write into OpenKlant is not something to do in passing.
        ///   <para>
        ///     Note this deliberately does not go through the enum's <c>JsonPropertyName</c>: those values
        ///     describe the wire format of "Notify NL" payloads, not what OMC registers.
        ///   </para>
        /// </remarks>
        public static string ToKanaal(this NotifyMethods notifyMethod)
        {
            return notifyMethod == NotifyMethods.Letter
                ? "brief"
                : notifyMethod.ToString();
        }
    }
}
