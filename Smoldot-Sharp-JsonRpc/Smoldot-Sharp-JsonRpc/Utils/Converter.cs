using Blake2Core;
using SimpleBase;
using System;
using System.Numerics;

namespace SmoldotSharp.JsonRpc
{
    /// <summary>
    /// Class for coverting data.
    /// </summary>
    public static class Converter
    {
        // https://github.com/paritytech/substrate/blob/master/primitives/core/src/crypto.rs

        /// <summary>
        /// Try to decode SS58 encoded uri.
        /// </summary>
        /// <param name="uri">Encoded uri to decode</param>
        /// <param name="bytes">Buffer for the decoded bytes</param>
        /// <param name="ident">Decoded chain identity</param>
        /// <returns>The operation has been done successfully or not</returns>
        public static bool TrySS58Decode(this ReadOnlySpan<char> uri,
            out Span<byte> bytes, out ushort ident)
        {
            ident = default;
            bytes = default;
            const int CheckSumLen = 2;

            // this method is for decoding address only.
            // shouldn't be longer than stackalloc-able size anyway.
            if (uri.Length > MultiAddress.SS58UriSize)
            {
                return false;
            }

            var dec = Base58.Bitcoin.Decode(uri);
            if (dec.Length < 2)
            {
                return false;
            }

            sbyte pfxLen;
            if (dec[0] <= 63)
            {
                pfxLen = 1;
                ident = dec[0];
            }
            else if (dec[0] <= 127)
            {
                var lower = (byte)((dec[0] << 2) | (dec[1] >> 6));
                var upper = (byte)(dec[1] & 0b00111111);
                pfxLen = 2;
                ident = (ushort)((upper << 8) | lower);
            }
            else
            {
                return false;
            }

            if (dec.Length != pfxLen + MultiAddress.Size + CheckSumLen)
            {
                return false;
            }

            var checkSumIdx = dec.Length - CheckSumLen;
            Span<byte> hashPre = stackalloc byte[] { 0x53, 0x53, 0x35, 0x38, 0x50, 0x52, 0x45 };
            var hashPreLen = hashPre.Length;
            Span<byte> hashIn = stackalloc byte[hashPreLen + checkSumIdx];
            hashPre.CopyTo(hashIn[..hashPreLen]);
            dec[..checkSumIdx].CopyTo(hashIn[hashPreLen..]);
            var hashOut = new Span<byte>(Blake2B.ComputeHash(hashIn.ToArray()));
            var checkSum = hashOut[..CheckSumLen];
            var givenCheckSum = dec[checkSumIdx..];
            if (!checkSum.SequenceEqual(givenCheckSum))
            {
                return false;
            }

            bytes = dec[pfxLen..checkSumIdx];
            return true;
        }

        /// <summary>
        /// Check if the string has 0x prefix.
        /// </summary>
        /// <param name="hex">Hex-string to check</param>
        /// <param name="onlyLower">Check only lower case or not.</param>
        /// <returns>Has 0x prefix or not.</returns>
        public static bool Has0XPrefix(this string hex, bool onlyLower = false)
        {
            if (onlyLower)
            {
                return hex[0] == '0' && hex[1] == 'x';
            }

            return hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X');
        }

        /// <summary>
        /// Make the hex string even number in length.
        /// </summary>
        /// <param name="hex">Hex string that is odd number in length.</param>
        /// <returns>New string</returns>
        public static string Padding(this string hex)
        {
            if (hex.Has0XPrefix())
            {
                return hex.Insert(2, "0");
            }
            else
            {
                return hex.PadLeft(hex.Length + 1, '0');
            }
        }

        /// <summary>
        /// Try deserialize the number that is serialized as bytes.
        /// </summary>
        /// <param name="hex">Serialized bytes as hex string</param>
        /// <param name="num">Deserialized number</param>
        /// <param name="isSigned">The number is signed value or not</param>
        /// <param name="isBigEndian">The number is serialized as BigEndian or not.</param>
        /// <returns>The operation has been done Successfully or not</returns>
        public static bool TryDeserialize(this string hex, out BigInteger num,
            bool isSigned = false, bool isBigEndian = true)
        {
            num = default;
            
            // only support 256bit integer (32byte * 2) + "0x" as max.
            // suold not be longer than stackalloc-able length anyway.
            var len = hex.Length;
            if (hex.Length > 66)
            {
                return false;
            }

            if (len % 2 != 0)
            {
                hex = hex.Padding();
            }

            var asChars = hex.AsSpan().Remove0X();
            len = asChars.Length;
            var buffSize = len / 2;
            Span<byte> buff = stackalloc byte[buffSize];
            if (!asChars.TryHexToBytes(buff, out var w) || w != buffSize)
            {
                return false;
            }

            num = new BigInteger(buff, !isSigned, isBigEndian);
            return true;
        }

        /// <summary>
        /// Remove 0x prefix from the string.
        /// </summary>
        /// <param name="src">Target string as ReadOnlySpan of char</param>
        /// <returns>New ReadOnlySpan of char without 0x prefix</returns>
        public static ReadOnlySpan<char> Remove0X(this ReadOnlySpan<char> src)
        {
            if (src[0] == '0' && (src[1] == 'x' || src[1] == 'x'))
            {
                return src[2..];
            }
            else
            {
                return src;
            }
        }

        /// <summary>
        /// Put 0x prefix to the string.
        /// </summary>
        /// <param name="src">Target string as Span of char</param>
        /// <returns>New Span of char with 0x prefix</returns>
        public static Span<char> Put0X(this Span<char> src)
        {
            src[0] = '0';
            src[1] = 'x';
            return src[2..];
        }

        /// <summary>
        /// Try to convert hex as ReadOnlySpan of char to bytes.
        /// </summary>
        /// <param name="src">Hex string without 0x prefix as ReadOnlySpan of char</param>
        /// <param name="dest">Buffer for the bytes that has loger than src / 2 in length</param>
        /// <param name="numWritten">Number of bytes written to the buffer</param>
        /// <returns>Successfully converted or not</returns>
        public static bool TryHexToBytes(this ReadOnlySpan<char> src, 
            Span<byte> dest, out int numWritten)
        {
            numWritten = 0;
            var srcLen = src.Length;
            if (srcLen % 2 != 0)
            {
                return false;
            }

            Span<byte> digits = stackalloc byte[2];
            for (int i = 0, j = 0; i < srcLen; j++)
            {
                for (int k = 0; k < 2; k++, i++)
                {
                    if (src[i] >= '0' && src[i] <= '9')
                    {
                        digits[k] = (byte)(src[i] - '0');
                    }
                    else if (src[i] >= 'a' && src[i] <= 'f')
                    {
                        digits[k] = (byte)(src[i] - 'a' + 10);
                    }
                    else if (src[i] >= 'A' && src[i] <= 'F')
                    {
                        digits[k] = (byte)(src[i] - 'A' + 10);
                    }
                    else
                    {
                        return false;
                    }
                }

                dest[j] = (byte)((digits[0] << 4) | digits[1]);
                numWritten++;
            }
            return true;
        }

        /// <summary>
        /// Convert bytes to hex as Span of char.
        /// </summary>
        /// <param name="src">Bytes as Span of byte</param>
        /// <param name="dest">Buffer for the coverted char that has longer than 2 * src in length</param>
        /// <param name="numWritten">Number of bytes written to the buffer</param>
        public static void BytesToHex(this Span<byte> src, Span<char> dest, out int numWritten)
        {
            numWritten = 0;
            var srcLen = src.Length;
            char upper;
            char lower;
            byte b;
            for (int i = 0, j = 0; i < srcLen; i++, j += 2)
            {
                b = (byte)(src[i] >> 4);
                upper = (char)(b > 9 ? b - 10 + 'a' : b + '0');
                b = (byte)(src[i] & 0b00001111);
                lower = (char)(b > 9 ? b - 10 + 'a' : b + '0');

                dest[j] = upper;
                dest[j + 1] = lower;
                numWritten += 2;
            }
        }
    }
}
