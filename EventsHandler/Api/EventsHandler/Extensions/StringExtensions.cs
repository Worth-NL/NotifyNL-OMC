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
        /// Determines whether the given <see langword="string"/> <c>is</c> empty.
        /// </summary>
        /// <param name="text">The text value to be validated.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified text <c>is</c> empty; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsEmpty(this string text)
        {
            return text == string.Empty;
        }

        /// <summary>
        /// Determines whether the given <see langword="string"/> is <c>not</c> empty.
        /// </summary>
        /// <param name="text">The text value to be validated.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified text is <c>not</c> empty; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotEmpty(this string text)
        {
            return !text.IsEmpty();
        }

        /// <summary>
        /// Encodes a raw plain <see langword="string"/> into <see cref="Base64"/> value.
        /// </summary>
        /// <returns>
        ///   Encoded original string.
        /// </returns>
        internal static string Base64Encode(this string originalTextValue)
        {
            return string.IsNullOrWhiteSpace(originalTextValue)
                ? string.Empty
                : Base64UrlEncoder.Encode(originalTextValue);
        }
        
        /// <summary>
        /// Encodes the <see cref="Base64"/> value back to a raw plain <see langword="string"/>.
        /// </summary>
        /// <returns>
        ///   Decoded original string.
        /// </returns>
        internal static string Base64Decode(this string encodedTextValue)
        {
            return string.IsNullOrWhiteSpace(encodedTextValue)
                ? string.Empty
                : Base64UrlEncoder.Decode(encodedTextValue);
        }
    }
}