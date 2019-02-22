using System;
using System.Security.Cryptography;

namespace TaskZero.Console
{
    /// <summary>
    /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
    /// http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html
    /// https://github.com/eventflow/EventFlow/blob/70dc3b469531fab9473dafa0e797b57d17f5bbeb/Source/EventFlow/Core/GuidFactories.cs
    /// </summary>
    public static class Deterministic
    {
        public static class Namespaces
        {
            public static readonly Guid Events = Guid.Parse("387F5B61-9E98-439A-BFF1-15AD0EA91EA0");
            public static readonly Guid Commands = Guid.Parse("4286D89F-7F92-430B-8E00-E468FE3C3F59");
        }

        public static Guid Create(Guid namespaceId, byte[] nameBytes)
        {
            // Always use version 5 (version 3 is MD5, version 5 is SHA1)
            const int version = 5;

            if (namespaceId == default(Guid)) throw new ArgumentNullException(nameof(namespaceId));
            if (nameBytes.Length == 0) throw new ArgumentNullException(nameof(nameBytes));

            // Convert the namespace UUID to network order (step 3)
            var namespaceBytes = namespaceId.ToByteArray();
            SwapByteOrder(namespaceBytes);

            // Comput the hash of the name space ID concatenated with the name (step 4)
            byte[] hash;
            using (var algorithm = SHA1.Create())
            {
                var combinedBytes = new byte[namespaceBytes.Length + nameBytes.Length];
                Buffer.BlockCopy(namespaceBytes, 0, combinedBytes, 0, namespaceBytes.Length);
                Buffer.BlockCopy(nameBytes, 0, combinedBytes, namespaceBytes.Length, nameBytes.Length);

                hash = algorithm.ComputeHash(combinedBytes);
            }

            // Most bytes from the hash are copied straight to the bytes of the new
            // GUID (steps 5-7, 9, 11-12)
            var newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            // Set the four most significant bits (bits 12 through 15) of the time_hi_and_version
            // field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
            newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));

            // Set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved
            // to zero and one, respectively (step 10)
            newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

            // Convert the resulting UUID to local byte order (step 13)
            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        internal static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        internal static void SwapBytes(byte[] guid, int left, int right)
        {
            var temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }
    }
}
