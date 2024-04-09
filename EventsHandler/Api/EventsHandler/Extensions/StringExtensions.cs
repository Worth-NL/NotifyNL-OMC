// © 2024, Worth Systems.

using Microsoft.IdentityModel.Tokens;
using System.Buffers.Text;

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
            if (string.IsNullOrWhiteSpace(originalTextValue))
            {
                return string.Empty;
            }
            
            return Base64UrlEncoder.Encode(originalTextValue);
        }
        
        /// <summary>
        /// Encodes the <see cref="Base64"/> value back to a raw plain <see langword="string"/>.
        /// </summary>
        /// <returns>
        ///   Decoded original string.
        /// </returns>
        internal static string Base64Decode(this string encodedTextValue)
        {
            if (string.IsNullOrWhiteSpace(encodedTextValue))
            {
                return string.Empty;
            }

            return Base64UrlEncoder.Decode(encodedTextValue);
        }
    }
}