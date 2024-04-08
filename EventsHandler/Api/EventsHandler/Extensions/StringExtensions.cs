// © 2024, Worth Systems.

using System.Buffers.Text;
using System.Text;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Encodes a raw plain <see langword="string"/> into <see cref="Base64"/> value.
        /// </summary>
        /// <returns>
        ///   Encoded original string.
        /// </returns>
        internal static string Base64Encode(this string originalTextValue)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(originalTextValue);
            
            return Convert.ToBase64String(plainTextBytes);
        }
        
        /// <summary>
        /// Encodes the <see cref="Base64"/> value back to a raw plain <see langword="string"/>.
        /// </summary>
        /// <returns>
        ///   Decoded original string.
        /// </returns>
        internal static string Base64Decode(this string encodedTextValue)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(encodedTextValue);
            
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}