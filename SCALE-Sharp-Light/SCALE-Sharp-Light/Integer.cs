using System;
using System.Numerics;
using System.Collections.Generic;

namespace ScaleSharpLight
{
    /// <summary>
    /// Class for scale-encoding methods.
    /// </summary>
    public static partial class ScaleLight
    {
        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(this sbyte n, Span<byte> buff)
        {
            if (buff.Length < 1)
            {
                throw new BufferOutOfRangeException();
            }

            buff[0] = *(byte*)&n;
            return 1;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value</param>
        /// <returns>Number of read bytes from the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for decoding. 
        /// </exception>
        public unsafe static int FixedDecodeI8(this Span<byte> buff, out sbyte n)
        {
            if (buff.Length < 1)
            {
                throw new BufferOutOfRangeException();
            }

            var b = buff[0];
            n = *(sbyte*)&b;
            return 1;
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(this short n, Span<byte> buff)
        {
            if (buff.Length < 2)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                *(short*)ptr = n;
            }
            return 2;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value</param>
        /// <returns>Number of read bytes from the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for decoding. 
        /// </exception>
        public unsafe static int FixedDecodeI16(this Span<byte> buff, out short n)
        {
            if (buff.Length < 2)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                n = *(short*)ptr;
            }
            return 2;
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(this int n, Span<byte> buff)
        {
            if (buff.Length < 4)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                *(int*)ptr = n;
            }
            return 4;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value</param>
        /// <returns>Number of read bytes from the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for decoding. 
        /// </exception>
        public unsafe static int FixedDecodeI32(this Span<byte> buff, out int n)
        {
            if (buff.Length < 4)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                n = *(int*)ptr;
            }
            return 4;
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(this long n, Span<byte> buff)
        {
            if (buff.Length < 8)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                *(long*)ptr = n;
            }
            return 8;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value</param>
        /// <returns>Number of read bytes from the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for decoding. 
        /// </exception>
        public unsafe static int FixedDecodeI64(this Span<byte> buff, out long n)
        {
            if (buff.Length < 8)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                n = *(long*)ptr;
            }
            return 8;
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(this ushort n, Span<byte> buff)
        {
            if (buff.Length < 2)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                *(ushort*)ptr = n;
            }
            return 2;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value</param>
        /// <returns>Number of read bytes from the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for decoding. 
        /// </exception>
        public unsafe static int FixedDecodeU16(this Span<byte> buff, out ushort n)
        {
            if (buff.Length < 2)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                n = *(ushort*)ptr;
            }
            return 2;
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(this uint n, Span<byte> buff)
        {
            if (buff.Length < 4)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                *(uint*)ptr = n;
            }
            return 4;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value</param>
        /// <returns>Number of read bytes from the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for decoding. 
        /// </exception>
        public unsafe static int FixedDecodeU32(this Span<byte> buff, out uint n)
        {
            if (buff.Length < 4)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                n = *(uint*)ptr;
            }
            return 4;
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(this ulong n, Span<byte> buff)
        {
            if (buff.Length < 8)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                *(ulong*)ptr = n;
            }
            return 8;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value</param>
        /// <returns>Number of read bytes from the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for decoding. 
        /// </exception>
        public unsafe static int FixedDecodeU64(this Span<byte> buff, out ulong n)
        {
            if (buff.Length < 8)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (byte* ptr = buff)
            {
                n = *(ulong*)ptr;
            }
            return 8;
        }

        /// <summary>
        /// Returnes the size of 128bit-fixed encoded size. 
        /// </summary>
        /// <param name="_">Value to encode</param>
        /// <returns>Encoded size</returns>
        public static int Fixed128EncodedSize(this in BigInteger _) => 16;

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="isSigned">Is value signed or not</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="size">Fixed-ecoding size</param>
        /// <returns>Number of written bytes to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when buffer is smaller than needed for encoding.
        /// </exception>
        public unsafe static int FixedEncode(
            this in BigInteger n, bool isSigned, Span<byte> buff, int size = 16)
        {
            if (isSigned && n < 0)
            {
                if (size % 8 == 0)
                {
                    if (buff.Length < size)
                    {
                        throw new BufferOutOfRangeException();
                    }

                    fixed (byte* ptr = buff)
                    {
                        ulong* p = (ulong*)ptr;
                        for (int i = 0; i < size / 8; i++, p++)
                        {
                            *p = 0xffff_ffff_ffff_ffff;
                        }
                    }
                } 
                else
                {
                    for (int i = 0; i < size; i++)
                    {
                        buff[i] = 0xff;
                    }
                }
            }

            if (!n.TryWriteBytes(buff[..size], out _, !isSigned))
            {
                throw new BufferOutOfRangeException();
            }

            return size;
        }

        /// <summary>
        /// Fixed-decode the value. 
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="isSigned">Is value signed or not</param>
        /// <param name="n">Decoded value</param>
        /// <param name="size">Fixed-encoded size</param>
        /// <returns>Number of read bytes from the buffer</returns>
        public static int FixedDecodeBigInt(
            this Span<byte> buff, bool isSigned, out BigInteger n, int size = 16)
        {
            n = new BigInteger(buff[..size], !isSigned);
            return size;
        }

        /// <summary>
        /// Returnes compact-encoding mode for the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <returns>Encoding mode for the value</returns>
        public static CompactModeKind CompactEncodingMode(this in BigInteger n)
        {
            if (n < 0)
            {
                return CompactModeKind.Error;
            }

            if (n <= 0x3f)
            {
                return CompactModeKind.SingleByteMode;
            }
            else if (n <= 0x3fff)
            {
                return CompactModeKind.TwoByteMode;
            }
            else if (n <= 0x3fff_ffff)
            {
                return CompactModeKind.FourByteMode;
            }
            else
            {
                return CompactModeKind.BigIntegerMode;
            }
        }

        /// <summary>
        /// Returnes compact-encoded size for the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <returns>compact-encoded size for the value</returns>
        /// <exception cref="NotExpectedDataException">
        /// Thrown when the value is not expected. ex. n < 0
        /// </exception>
        public static int CompactEncodedSize(this in BigInteger n)
        {
            return n.CompactEncodingMode() switch
            {
                CompactModeKind.SingleByteMode => 1,
                CompactModeKind.TwoByteMode => 2,
                CompactModeKind.FourByteMode => 4,
                CompactModeKind.BigIntegerMode => n.GetByteCount(true) + 1,
                _ => throw new NotExpectedDataException()
            };
        }

        /// <summary>
        /// Compact-encode the value.
        /// </summary>
        /// <param name="n">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Numbere of bytes written to the buffer</returns>
        /// <exception cref="NotExpectedDataException">
        /// Thrown when the value is not expected. ex. n < 0
        /// </exception>
        public static int CompactEncode(this BigInteger n, Span<byte> buff)
        {
            switch (n.CompactEncodingMode())
            {
                case CompactModeKind.SingleByteMode:
                    n <<= 2;
                    if (!n.TryWriteBytes(buff, out var written1byte, true))
                    {
                        return 0;
                    }
                    return written1byte == 1 ? 1 : 0;
                case CompactModeKind.TwoByteMode:
                    n <<= 2;
                    n |= 0b0001;
                    if (!n.TryWriteBytes(buff, out var written2byte, true))
                    {
                        return 0;
                    }
                    return written2byte == 2 ? 2 : 0;
                case CompactModeKind.FourByteMode:
                    n <<= 2;
                    n |= 0b0010;
                    if (!n.TryWriteBytes(buff, out var written3or4byte, true))
                    {
                        return 0;
                    }
                    return (written3or4byte == 3 || written3or4byte == 4) ? 4 : 0;
                case CompactModeKind.BigIntegerMode:
                    var numBytes = (byte)(n.GetByteCount(true) - 4);
                    numBytes <<= 2;
                    numBytes |= 0b0011;
                    n <<= 8;
                    n |= numBytes;
                    return n.TryWriteBytes(buff, out var numWritten, true) ? numWritten : 0;
                default:
                    throw new NotExpectedDataException();
            }
        }

        /// <summary>
        /// Try comapct-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="n">Decoded value.</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Decoded successfully or not</returns>
        /// <exception cref="NotExpectedDataException">
        /// Thrown when encoded value is not expected.
        /// </exception>
        public static bool TryCompactDecode(
            this Span<byte> buff, out BigInteger n, out int numRead)
        {
            var encodedMode = buff[0] & 0b0011;
            switch(encodedMode)
            {
                case 0b0000:
                    n = new BigInteger((uint)(buff[0] >> 2));
                    numRead = 1;
                    return true;
                case 0b0001:
                    n = new BigInteger(buff[..2], true);
                    n >>= 2;
                    numRead = 2;
                    return true;
                case 0b0010:
                    n = new BigInteger(buff[..4], true);
                    n >>= 2;
                    numRead = 4;
                    return true;
                case 0b0011:
                    numRead = (buff[0] >> 2) + 4;
                    n = new BigInteger(buff[1..++numRead], true);
                    return true;
                default:
                    throw new NotExpectedDataException();
            }
        }

        /// <summary>
        /// Try compact-decode already-known times. Values are written to the receiver index 0.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="receiverLengthKnown">
        /// Decoded values initialized as already-known length
        /// </param>
        /// <param name="numSucceeded">Number of succeeded decoding</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded all or not</returns>
        public static bool TryMultipleCompactDecode(
            this Span<byte> buff, BigInteger[] receiverLengthKnown, 
            out int numSucceeded, out int numRead)
        {
            numRead = 0;
            var iter = receiverLengthKnown.Length;
            if (iter < 1)
            {
                numSucceeded = 0;
                return false;
            }

            var slice = buff[numRead..];
            for (int i = 0; i < iter; i++)
            { 
                if (!slice.TryCompactDecode(out BigInteger n, out var r))
                {
                    numSucceeded = i;
                    return false;
                }

                receiverLengthKnown[i] = n;
                numRead += r;
                slice = buff[numRead..];
            }
            numSucceeded = iter;
            return true;
        }

        /// <summary>
        /// Try compact-decode unknown times. Values are added to the receiver.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="receiverLengthUnknown">Decoded values</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded all or not</returns>
        public static bool TryMultipleCompactDecode(
            this Span<byte> buff, List<BigInteger> receiverLengthUnknown, out int numRead)
        {
            numRead = 0;
            if (receiverLengthUnknown == null)
            {
                return false;
            }

            var iter = 0;
            var slice = buff[numRead..];
            while (slice.TryCompactDecode(out BigInteger n, out var r))
            {
                receiverLengthUnknown.Add(n);
                numRead += r;
                slice = buff[numRead..];
                if (slice.Length < 1)
                {
                    break;
                }
                ++iter;
            }
            return iter > 0;
        }
    }

    /// <summary>
    /// Enum expressing compact-encoding mode.
    /// </summary>
    public enum CompactModeKind : byte
    {
        Error,
        SingleByteMode,
        TwoByteMode,
        FourByteMode,
        BigIntegerMode
    }

    /// <summary>
    /// Class for factory method of compact-encoding integer, enusuring the integer is unsigned.
    /// </summary>
    public static class Compact
    {
        /// <summary>
        /// Create BigInteger instance from unsigned integer.
        /// </summary>
        /// <param name="n">Unsigned integer</param>
        /// <returns>BigInteger ready for compact-encoding</returns>
        public static BigInteger CompactInteger(byte n) => new BigInteger((uint)n);

        /// <summary>
        /// Create BigInteger instance from unsigned integer.
        /// </summary>
        /// <param name="n">Unsigned integer</param>
        /// <returns>BigInteger ready for compact-encoding</returns>
        public static BigInteger CompactInteger(ushort n) => new BigInteger((uint)n);

        /// <summary>
        /// Create BigInteger instance from unsigned integer.
        /// </summary>
        /// <param name="n">Unsigned integer</param>
        /// <returns>BigInteger ready for compact-encoding</returns>
        public static BigInteger CompactInteger(uint n) => new BigInteger(n);

        /// <summary>
        /// Create BigInteger instance from unsigned integer.
        /// </summary>
        /// <param name="n">Unsigned integer</param>
        /// <returns>BigInteger ready for compact-encoding</returns>
        public static BigInteger CompactInteger(ulong n) => new BigInteger(n);
    }
}
