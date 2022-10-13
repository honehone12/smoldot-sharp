using System;
using System.Numerics;
using System.Collections.Generic;

namespace ScaleSharpLight
{
    public static partial class ScaleLight
    {
        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<sbyte> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<sbyte> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(this List<sbyte> l, Span<byte> buff)
        {
            var span = new Span<sbyte>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static int Encode(this Span<sbyte> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecI8(
            this Span<byte> buff, out sbyte[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new sbyte[(int)len];
                var numBytes = (int)len;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<sbyte>();
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<short> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 2;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<short> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 2;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(this List<short> l, Span<byte> buff)
        {
            var span = new Span<short>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static int Encode(this Span<short> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len * 2;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecI16(
            this Span<byte> buff, out short[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new short[(int)len];
                var numBytes = (int)len * 2;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<short>();
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<int> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 4;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<int> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 4;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(this List<int> l, Span<byte> buff)
        {
            var span = new Span<int>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static int Encode(this Span<int> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len * 4;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecI32(
            this Span<byte> buff, out int[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new int[(int)len];
                var numBytes = (int)len * 4;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<int>();
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<long> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 8;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<long> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 8;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(this List<long> l, Span<byte> buff)
        {
            var span = new Span<long>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static int Encode(this Span<long> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len * 8;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecI64(
            this Span<byte> buff, out long[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new long[(int)len];
                var numBytes = (int)len * 8;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<long>();
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<byte> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<byte> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(this List<byte> l, Span<byte> buff)
        {
            var span = new Span<byte>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static int Encode(this Span<byte> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecU8(
            this Span<byte> buff, out byte[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new byte[(int)len];
                var numBytes = (int)len;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<byte>();
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<ushort> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 2;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<ushort> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 2;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(this List<ushort> l, Span<byte> buff)
        {
            var span = new Span<ushort>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static int Encode(this Span<ushort> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len * 2;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecU16(
            this Span<byte> buff, out ushort[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new ushort[(int)len];
                var numBytes = (int)len * 2;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<ushort>();
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<uint> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 4;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<uint> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 4;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(this List<uint> l, Span<byte> buff)
        {
            var span = new Span<uint>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static int Encode(this Span<uint> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len * 4;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecU32(
            this Span<byte> buff, out uint[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new uint[(int)len];
                var numBytes = (int)len * 4;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<uint>();
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<ulong> l)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 8;
        }

        /// <summary>
        /// Returnes total encoded size. 
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this Span<ulong> l)
        {
            var len = l.Length;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * 8;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public static int Encode(this List<ulong> l, Span<byte> buff)
        {
            var span = new Span<ulong>(l.ToArray());
            return span.Encode(buff);
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public unsafe static int Encode(this Span<ulong> l, Span<byte> buff)
        {
            var len = l.Length;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            var numBytes = len * 8;
            if (buff.Length < numWritten + numBytes)
            {
                throw new BufferOutOfRangeException();
            }

            fixed (void* srcPtr = l, destPtr = buff[numWritten..])
            {
                Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
            }
            numWritten += numBytes;
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Successfully decoded or not</returns>
        /// <exception cref="BufferOutOfRangeException">
        /// Thrown when the buffer is smaller than expected.
        /// </exception>
        public unsafe static bool TryDecodeVecU64(
            this Span<byte> buff, out ulong[] l, out int numRead)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                l = new ulong[(int)len];
                var numBytes = (int)len * 8;
                if (buff.Length < numRead + numBytes)
                {
                    throw new BufferOutOfRangeException();
                }

                fixed (void* srcPtr = buff[numRead..], destPtr = l)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, numBytes, numBytes);
                }
                numRead += numBytes;
                return true;
            }

            l = Array.Empty<ulong>(); ;
            numRead = -1;
            return false;
        }

        /// <summary>
        /// Returnes total encoded size of the value
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="eachSize">Encoding size of each element</param>
        /// <returns>Total encoded size</returns>
        public static int EncodedSize(this List<BigInteger> l, int eachSize = 16)
        {
            var len = l.Count;
            var compactSize = new BigInteger((uint)len).CompactEncodedSize();
            return compactSize + len * eachSize;
        }

        /// <summary>
        /// Encode the value.
        /// </summary>
        /// <param name="l">Value to encode</param>
        /// <param name="isSigned">Are all elements signed or not</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="eachSize">Encoding size of the each element</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Encode(
            this List<BigInteger> l, bool isSigned, Span<byte> buff, int eachSize = 16)
        {
            var len = l.Count;
            var compact = new BigInteger((uint)len);
            var numWritten = compact.CompactEncode(buff);
            if (numWritten < 0)
            {
                return -1;
            }

            for (int i = 0; i < len; i++)
            {
                numWritten += l[i].FixedEncode(isSigned, buff[numWritten..], eachSize);
            }
            return numWritten;
        }

        /// <summary>
        /// Try decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="isSigned">Are all elements signed or not</param>
        /// <param name="l">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <param name="eachSize"></param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryDecodeVecFixedBigInt(
            this Span<byte> buff, bool isSigned, 
            List<BigInteger> l, out int numRead, int eachSize = 16)
        {
            if (buff.TryCompactDecode(out BigInteger len, out numRead))
            {
                for (int i = 0; i < len; i++)
                {
                    numRead += buff[numRead..].FixedDecodeBigInt(
                        isSigned, out BigInteger n, eachSize);
                    l.Add(n);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Create new instance of vector prefix.
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="l">Value to encode</param>
        /// <returns>BigInteger ready for compact-encoding as vector prefix</returns>
        public static BigInteger VectorPrefix<T>(this List<T> l)
        {
            return new BigInteger((uint)l.Count);
        }

        /// <summary>
        /// Try decode vecor prefix.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="vecLen">Decoded BigInteger as vector prefix</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <returns>Succcessfully decoded or not</returns>
        public static bool TryDecodeVectorPrefix(
            this Span<byte> buff, out BigInteger vecLen, out int numRead)
        {
            return buff.TryCompactDecode(out vecLen, out numRead);
        }
    }
}
