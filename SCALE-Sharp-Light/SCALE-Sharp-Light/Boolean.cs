using System;

namespace ScaleSharpLight
{
    public static partial class ScaleLight
    {
        /// <summary>
        /// Returns fixed-encoded size.
        /// </summary>
        /// <param name="_">Value to encode</param>
        /// <returns>Encoded size</returns>
        public static int EncodedSize(this bool _) => 1;

        /// <summary>
        /// Fixed-Encode the value.
        /// </summary>
        /// <param name="flag">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        public static void Encode(this bool flag, Span<byte> buff)
        {
            buff[0] = flag ? (byte)0b0001 : (byte)0b0000;
        }

        /// <summary>
        /// Try fixed-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="flag">Decoded value</param>
        /// <returns>True if Successfully decoded, false if not.</returns>
        public static bool TryDecodeBool(this Span<byte> buff, out bool flag)
        {
            var b = buff[0];
            flag = (b & 0b0001) == 0b0001;
            return b == 0b0000 || b == 0b0001;
        }
    }
}
