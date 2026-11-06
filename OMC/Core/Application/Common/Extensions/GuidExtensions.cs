// © 2026, Worth Systems.

using System.Security.Cryptography;
using System.Text;

namespace Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Guid"/>s.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Creates a name-based <see cref="Guid"/> (UUID version 5, RFC 9562): the same namespace and name always give the same <see cref="Guid"/>.
        /// </summary>
        /// <param name="namespaceId">The namespace the name belongs to.</param>
        /// <param name="name">The name, unique within the namespace.</param>
        /// <remarks>
        ///   The name can be found back from the <see cref="Guid"/> by trying candidates, so never use personal data (like a BSN) as the name.
        /// </remarks>
        public static Guid CreateNameBased(this Guid namespaceId, string name)
        {
            byte[] namespaceBytes = namespaceId.ToByteArray(bigEndian: true);
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);

            byte[] input = new byte[namespaceBytes.Length + nameBytes.Length];
            namespaceBytes.CopyTo(input, 0);
            nameBytes.CopyTo(input, namespaceBytes.Length);

#pragma warning disable CA5350  // NOTE: SHA-1 is what RFC 9562 prescribes for version 5; it is not used for security here
            byte[] hash = SHA1.HashData(input);
#pragma warning restore CA5350

            byte[] uuid = hash[..16];
            uuid[6] = (byte)((uuid[6] & 0x0F) | 0x50);  // Version 5
            uuid[8] = (byte)((uuid[8] & 0x3F) | 0x80);  // Variant RFC 9562

            return new Guid(uuid, bigEndian: true);
        }
    }
}
