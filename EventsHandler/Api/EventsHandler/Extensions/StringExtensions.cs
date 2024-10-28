// © 2024, Worth Systems.

using EventsHandler.Constants;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for <see langword="string"/>s.
    /// </summary>
    internal static class StringExtensions
    {
        #region Validation
        /// <summary>
        /// Determines whether the given <see langword="string"/> <c>is</c> empty.
        /// </summary>
        /// <param name="text">The text value to be validated.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified text <c>is</c> empty; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNullOrEmpty([NotNullWhen(false)] this string? text)
        {
            return string.IsNullOrEmpty(text);
        }

        /// <summary>
        /// Determines whether the given <see langword="string"/> is <c>not</c> empty.
        /// </summary>
        /// <param name="text">The text value to be validated.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified text is <c>not</c> empty; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? text)
        {
            return !string.IsNullOrEmpty(text);
        }
        #endregion

        #region Encoding / Decoding (Base64)
        /// <summary>
        /// Encodes a raw plain <see langword="string"/> into <see cref="Base64"/> value.
        /// </summary>
        /// <returns>
        ///   Encoded original string.
        /// </returns>
        internal static string Base64Encode(this byte[] originalStream)
        {
            return originalStream.Length == 0
                ? string.Empty
                : Convert.ToBase64String(originalStream);
        }

        /// <summary>
        /// Encodes the <see cref="Base64"/> value back to a raw plain <see langword="string"/>.
        /// </summary>
        /// <returns>
        ///   Decoded original string.
        /// </returns>
        internal static byte[] Base64Decode(this string encodedTextValue)
        {
            return string.IsNullOrWhiteSpace(encodedTextValue)
                ? Array.Empty<byte>()
                : Convert.FromBase64String(encodedTextValue);
        }
        #endregion

        // NOTE: GZipStream algorithm is slightly faster and for general purpose or large files
        //       ZLibStream algorithm is slightly slower but offers better compression level
        #region Compressing / Decompressing
        /// <summary>
        /// Performs <see langword="string"/> compression using GZip algorithm.
        /// </summary>
        /// <returns>
        ///   The compressed and encoded <see langword="string"/>.
        /// </returns>
        internal static async Task<string> CompressGZipAsync(this string originalTextValue, CancellationToken cancellationToken)
        {
            if (originalTextValue.IsNullOrEmpty())
            {
                return string.Empty;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(originalTextValue);

            using var memoryStream = new MemoryStream();
            await using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))  // NOTE: Leaves MemoryStream open, so it's data can be read in the final statement
            {
                await gzipStream.WriteAsync(buffer, cancellationToken);
            }

            return memoryStream.ToArray().Base64Encode();
        }

        /// <summary>
        /// Performs <see langword="string"/> decompression using GZip algorithm.
        /// </summary>
        /// <returns>
        ///   The decoded and decompressed <see langword="string"/>.
        /// </returns>
        internal static async Task<string> DecompressGZipAsync(this string compressedTextValue, CancellationToken cancellationToken)
        {
            if (compressedTextValue.IsNullOrEmpty())
            {
                return string.Empty;
            }

            byte[] buffer = compressedTextValue.Base64Decode();

            using var memoryStream = new MemoryStream(buffer);
            await using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();

            await gzipStream.CopyToAsync(resultStream, cancellationToken);

            return Encoding.UTF8.GetString(resultStream.ToArray());
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Converts a given <see langword="string"/> into a target generic <typeparamref name="TData"/> type.
        /// </summary>
        /// <typeparam name="TData">The target type onto which the conversion should be handled.</typeparam>
        /// <param name="originalTextValue">The original text value.</param>
        /// <returns>
        ///   The converted generic value.
        /// </returns>
        internal static TData ChangeType<TData>(this string originalTextValue)
        {
            if (originalTextValue.IsNullOrEmpty() &&
                typeof(TData) != typeof(string) &&
                typeof(TData) != typeof(Uri))
            {
                return default!;
            }

            // Retrieve as TData => Guid
            if (typeof(TData) == typeof(Guid))
            {
                _ = Guid.TryParse(originalTextValue, out Guid validGuid);

                return (TData)Convert.ChangeType(validGuid, typeof(TData));
            }
            
            // Retrieve as TData => Guid
            if (typeof(TData) == typeof(Uri))
            {
                _ = Uri.TryCreate(originalTextValue, UriKind.Absolute, out Uri? validUri);

                return (TData)Convert.ChangeType(validUri ?? DefaultValues.Models.EmptyUri, typeof(TData));
            }

            // Retrieve as TData => int, ushort, bool
            return (TData)Convert.ChangeType(originalTextValue, typeof(TData));
        }
        #endregion

        #region Modification
        private const string CommaSeparator = ", ";

        /// <summary>
        /// Joins the specified collection into a comma-separated <see langword="string"/>.
        /// </summary>
        /// <param name="collection">The collection to be parsed.</param>
        /// <returns>
        ///   A comma-separated string
        /// </returns>
        internal static string Join(this IEnumerable<string> collection)
        {
            return string.Join(CommaSeparator, collection);
        }
        #endregion
    }
}