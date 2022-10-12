using System;
using System.Numerics;

namespace ScaleSharpLight
{
    /// <summary>
    /// Struct for pseudo value for two variants enum of rust. ex. Option or Result.
    /// </summary>
    public readonly struct Prefix : IComparable<Prefix>, IEquatable<Prefix>
    {
        public readonly byte prefix;

        /// <summary>
        /// Fixed-ecoded size of Prefix.
        /// </summary>
        public int EncodedPrefixSize => 1;
        
        /// <summary>
        /// Create instance from flag.
        /// </summary>
        /// <param name="flag"></param>
        public Prefix(bool flag)
        {
            prefix = flag ? (byte)0x01 : (byte)0x00;
        }

        public override int GetHashCode()
        {
            return prefix.GetHashCode();
        }

        public int CompareTo(Prefix other)
        {
            return prefix.CompareTo(other.prefix);
        }

        public bool Equals(Prefix other)
        {
            return prefix.Equals(other.prefix);
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="buff">Buffer for encoding</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public int Encode(Span<byte> buff)
        {
            buff[0] = prefix;
            return 1;
        }
    }

    /// <summary>
    /// Class for factory methods creating pseudo value for Option of rust.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// Create pseudo value for None of Option.
        /// </summary>
        public static Prefix None => new Prefix(false);

        /// <summary>
        /// Create pseudo value of Some
        /// </summary>
        /// <param name="val">String contained by Some</param>
        /// <returns>Pseudo value of Some</returns>
        public static ValueTuple<Prefix, string> ToOption(this string val)
        {
            return ValueTuple.Create(
                new Prefix(string.IsNullOrEmpty(val)), val ?? string.Empty);
        }

        /// <summary>
        /// Create pseudo value of Some(T)
        /// </summary>
        /// <param name="val">T contained by Some</param>
        /// <returns>Pseudo value of Some(T)</returns>
        public static ValueTuple<Prefix, T> ToOption<T>(this T val) where T : new()
        {
            return ValueTuple.Create(new Prefix(val != null), val ?? new T());
        }

        /// <summary>
        /// Returnes pseudo value is None or not. 
        /// </summary>
        /// <param name="prefix">Pseudo value</param>
        /// <returns>true if the value is None, false if not</returns>
        public static bool IsNone(this Prefix prefix)
        {
            return prefix.prefix == 0x00;
        }

        /// <summary>
        /// Returnes pseudo value is None or not.
        /// </summary>
        /// <typeparam name="T">Type contained by Option</typeparam>
        /// <param name="op">Pseudo value</param>
        /// <returns>true if the value is None, false if not</returns>
        public static bool IsNone<T>(in this ValueTuple<Prefix, T> op)
        {
            return op.Item1.IsNone();
        }

        /// <summary>
        /// Create instance of None with new instance of T, not null.
        /// </summary>
        /// <typeparam name="T">Type contained by Option</typeparam>
        /// <returns>New instance of None</returns>
        public static ValueTuple<Prefix, T> NoneAndNew<T>() where T : new()
        {
            return new ValueTuple<Prefix, T>(new Prefix(false), new T());
        }

        /// <summary>
        /// Create instance of None with struct default.
        /// </summary>
        /// <typeparam name="T">Type contained by Option</typeparam>
        /// <returns>New instance of None</returns>
        public static ValueTuple<Prefix, T> NoneAndDefault<T>() where T : struct
        {
            return default;
        }

        /// <summary>
        /// Create instance of None with empty string, not null.
        /// </summary>
        public static ValueTuple<Prefix, string> NoneAndEmpty => (default, string.Empty);

        /// <summary>
        /// Create instance of Some from value. 
        /// </summary>
        /// <param name="flg">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, bool> Bool(bool flg)
        {
            return ValueTuple.Create(new Prefix(true), flg);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, sbyte> I8(sbyte n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, short> I16(short n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, int> I32(int n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, long> I64(long n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, byte> U8(byte n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, ushort> U16(ushort n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, uint> U32(uint n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, ulong> U64(ulong n)
        {
            return ValueTuple.Create(new Prefix(true), n);
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, BigInteger> IBig(long n)
        {
            return ValueTuple.Create(new Prefix(true), new BigInteger(n));
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, BigInteger> UBig(ulong n)
        {
            return ValueTuple.Create(new Prefix(true), new BigInteger(n));
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, BigInteger> CompactInteger(byte n)
        {
            return ValueTuple.Create(new Prefix(true), new BigInteger((uint)n));
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, BigInteger> CompactInteger(ushort n)
        {
            return ValueTuple.Create(new Prefix(true), new BigInteger((uint)n));
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, BigInteger> CompactInteger(uint n)
        {
            return ValueTuple.Create(new Prefix(true), new BigInteger(n));
        }

        /// <summary>
        /// Create instance of Some from value.
        /// </summary>
        /// <param name="n">Value contained by Some.</param>
        /// <returns>New instance of Some</returns>
        public static ValueTuple<Prefix, BigInteger> CompactInteger(ulong n)
        {
            return ValueTuple.Create(new Prefix(true), new BigInteger(n));
        }
    }

    /// <summary>
    /// Class for factory methods creating pseudo value for Result of rust.
    /// </summary>
    public static class Result
    {
        /// <summary>
        /// Returnes the pseudo value is Ok or not.
        /// </summary>
        /// <param name="prefix">Pseudo value</param>
        /// <returns>Pseudo value is Ok or not</returns>
        public static bool IsOk(this Prefix prefix)
        {
            return prefix.prefix == 0x00;
        }

        /// <summary>
        /// Returnes the pseudo value is Ok or not.
        /// </summary>
        /// <typeparam name="T">Contained type by Result</typeparam>
        /// <param name="op">Pseudo value</param>
        /// <returns>Pseudo value is Ok or not</returns>
        public static bool IsOk<T>(in this ValueTuple<Prefix, T> op)
        {
            return op.Item1.IsOk();
        }

        /// <summary>
        /// Create pseudo value for Ok of Result. 
        /// </summary>
        /// <returns>Pseudo value of Ok</returns>
        public static Prefix Ok()
        {
            return new Prefix(false);
        }

        /// <summary>
        /// Create pseudo value for Err of Result.
        /// </summary>
        /// <returns>Pseudo value of Err</returns>
        public static Prefix Err()
        {
            return new Prefix(true);
        }

        /// <summary>
        /// Create pseudo value for Ok of T. New instance of T will be created if it is null.
        /// </summary>
        /// <typeparam name="T">Type contained by Ok</typeparam>
        /// <param name="val">Value contained by Ok</param>
        /// <returns>Pseudo value of Ok with T</returns>
        public static ValueTuple<Prefix, T> Ok<T>(T val) where T : new()
        {
            return new ValueTuple<Prefix, T>(
                new Prefix(false), val ?? new T());
        }

        /// <summary>
        /// Create pseudo value for Ok of string. Empty string will be contained if it is null.
        /// </summary>
        /// <param name="val">Value contained by Ok</param>
        /// <returns>Pseudo value of Ok with string</returns>
        public static ValueTuple<Prefix, string> Ok(string val)
        {
            return new ValueTuple<Prefix, string>(
                new Prefix(false), val ?? string.Empty);
        }

        /// <summary>
        /// Create pseudo value for Err of T. New instance of T will be created if it is null.
        /// </summary>
        /// <typeparam name="T">Type contained by Err</typeparam>
        /// <param name="val">Value contained by Err</param>
        /// <returns>Pseudo value of Err with T</returns>
        public static ValueTuple<Prefix, T> Err<T>(T val) where T : new()
        {
            return new ValueTuple<Prefix, T>(
                new Prefix(true), val ?? new T());
        }

        /// <summary>
        /// Create pseudo value for Err of string. Empty string will be contained if it is null.
        /// </summary>
        /// <param name="val">Value contained by Err</param>
        /// <returns>Pseudo value of Err with string</returns>
        public static ValueTuple<Prefix, string> Err(string val)
        {
            return new ValueTuple<Prefix, string>(
                new Prefix(true), val ?? string.Empty);
        }
    }

    public static partial class ScaleLight
    {
        /// <summary>
        /// Try fixed-decode Prefix.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="prefix">Decoded Prefix</param>
        /// <returns>Successfully decoded or not.</returns>
        public static bool TryDecodePrefix(this Span<byte> buff, out Prefix prefix)
        {
            var b = buff[0];
            if (b == 0x00 || b == 0x01)
            {
                prefix = new Prefix(b != 0x00);
                return true;
            }
            else
            {
                prefix = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, bool> op, bool asResult = false)
        {
            if (asResult)
            {
                return 2;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 2;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, bool> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.Encode(buff[1..]);
            return 2;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionBool(
            this Span<byte> buff,
            out ValueTuple<Prefix, bool> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedBool(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultBool(
            this Span<byte> buff,
            out ValueTuple<Prefix, bool> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedBool(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        static bool TryFixedDecodePrefixedBool(
            this Span<byte> buff,
            out ValueTuple<Prefix, bool> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    if (buff[1..].TryDecodeBool(out bool n))
                    {
                        op = ValueTuple.Create(prefix, n);
                        numRead = 2;
                        return true;
                    }
                }
            }

            numRead = default;
            op = default;
            return false;
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, sbyte> op, bool asResult = false)
        {
            if (asResult)
            {
                return 2;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 2;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, sbyte> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.FixedEncode(buff[1..]);
            return 2;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionI8(
            this Span<byte> buff,
            out ValueTuple<Prefix, sbyte> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI8(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultI8(
            this Span<byte> buff,
            out ValueTuple<Prefix, sbyte> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI8(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedI8(
            this Span<byte> buff, 
            out ValueTuple<Prefix, sbyte> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeI8(out sbyte n);
                    op = ValueTuple.Create(prefix, n);
                    numRead = 2;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, short> op, bool asResult = false)
        {
            if (asResult)
            {
                return 3;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 3;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, short> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.FixedEncode(buff[1..]);
            return 3;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionI16(
            this Span<byte> buff,
            out ValueTuple<Prefix, short> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI16(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultI16(
            this Span<byte> buff,
            out ValueTuple<Prefix, short> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI16(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedI16(
            this Span<byte> buff, 
            out ValueTuple<Prefix, short> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeI16(out short n);
                    op = ValueTuple.Create(prefix, n);
                    numRead = 3;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, int> op, bool asResult = false)
        {
            if (asResult)
            {
                return 5;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 5;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, int> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.FixedEncode(buff[1..]);
            return 5;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionI32(
            this Span<byte> buff,
            out ValueTuple<Prefix, int> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI32(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultI32(
            this Span<byte> buff,
            out ValueTuple<Prefix, int> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI32(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedI32(
            this Span<byte> buff, 
            out ValueTuple<Prefix, int> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeI32(out int n);
                    op = ValueTuple.Create(prefix, n);
                    numRead = 5;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, long> op, bool asResult = false)
        {
            if (asResult)
            {
                return 9;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 9;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, long> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.FixedEncode(buff[1..]);
            return 9;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionI64(
            this Span<byte> buff,
            out ValueTuple<Prefix, long> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI64(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultI64(
            this Span<byte> buff,
            out ValueTuple<Prefix, long> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedI64(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedI64(
            this Span<byte> buff, 
            out ValueTuple<Prefix, long> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeI64(out long n);
                    op = ValueTuple.Create(prefix, n);
                    numRead = 9;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, byte> op, bool asResult = false)
        {
            if (asResult)
            {
                return 2;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 2;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, byte> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            buff[1] = op.Item2;
            return 2;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionU8(
            this Span<byte> buff,
            out ValueTuple<Prefix, byte> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU8(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultU8(
            this Span<byte> buff,
            out ValueTuple<Prefix, byte> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU8(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedU8(
            this Span<byte> buff, 
            out ValueTuple<Prefix, byte> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    op = ValueTuple.Create(prefix, buff[1]);
                    numRead = 2;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, ushort> op, bool asResult = false)
        {
            if (asResult)
            {
                return 3;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 3;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, ushort> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.FixedEncode(buff[1..]);
            return 3;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionU16(
            this Span<byte> buff,
            out ValueTuple<Prefix, ushort> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU16(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultU16(
            this Span<byte> buff,
            out ValueTuple<Prefix, ushort> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU16(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedU16(
            this Span<byte> buff, 
            out ValueTuple<Prefix, ushort> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeU16(out ushort n);
                    op = ValueTuple.Create(prefix, n);
                    numRead = 3;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, uint> op, bool asResult = false)
        {
            if (asResult)
            {
                return 5;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 5;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, uint> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.FixedEncode(buff[1..]);
            return 5;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionU32(
            this Span<byte> buff,
            out ValueTuple<Prefix, uint> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU32(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultU32(
            this Span<byte> buff,
            out ValueTuple<Prefix, uint> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU32(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedU32(
            this Span<byte> buff, 
            out ValueTuple<Prefix, uint> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeU32(out uint n);
                    op = ValueTuple.Create(prefix, n);
                    numRead = 5;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int FixedEncodedSize(
            this ValueTuple<Prefix, ulong> op, bool asResult = false)
        {
            if (asResult)
            {
                return 9;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 9;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int FixedEncode(
            this ValueTuple<Prefix, ulong> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            op.Item2.FixedEncode(buff[1..]);
            return 9;
        }

        /// <summary>
        /// Try fixed-decode the value as Option.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionU64(
            this Span<byte> buff,
            out ValueTuple<Prefix, ulong> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU64(out op, out numRead);
        }

        /// <summary>
        /// Try fixed-decode the value as Result.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultU64(
            this Span<byte> buff,
            out ValueTuple<Prefix, ulong> op, out int numRead)
        {
            return buff.TryFixedDecodePrefixedU64(out op, out numRead, true);
        }

        /// <summary>
        /// Try fixed-decode the value
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Decode the value as Result or Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedU64(
            this Span<byte> buff,
            out ValueTuple<Prefix, ulong> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeU64(out ulong n);
                    op = ValueTuple.Create(prefix, n);
                    numRead = 9;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes 128bit fixed-encoded size of the value.
        /// </summary>
        /// <param name="op">Value ot encode</param>
        /// <param name="asResult">Encode as Result or as Option</param>
        /// <returns>Encoded size of the value</returns>
        public static int Fixed128EncodedSize(
            this in ValueTuple<Prefix, BigInteger> op, bool asResult = false)
        {
            if (asResult)
            {
                return 17;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : 17;
            }
        }

        /// <summary>
        /// Fixed-encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="isSigned">Is value signed or not</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode the value as Result or as Option</param>
        /// <param name="size">Total fixed-encoding size</param>
        /// <returns>Successfully encoded or not</returns>
        public static int FixedEncode(
            this in ValueTuple<Prefix, BigInteger> op,
            bool isSigned, Span<byte> buff, bool asResult = false, int size = 17)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            return op.Item2.FixedEncode(isSigned, buff[1..size], size - 1) + 1;
        }

        /// <summary>
        /// Try fixed-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="isSigned">Is the value is signed or not</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <param name="size">Total size of the encoded value</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeOptionBigInt(
            this Span<byte> buff, bool isSigned,
            out ValueTuple<Prefix, BigInteger> op, out int numRead, int size = 17)
        {
            return buff.TryFixedDecodePrefixedBigInt(isSigned, out op, out numRead, false, size);
        }

        /// <summary>
        /// Try fixed-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="isSigned">Is the value is signed or not</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <param name="size">Total size of the encoded value</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodeResultBigInt(
            this Span<byte> buff, bool isSigned,
            out ValueTuple<Prefix, BigInteger> op, out int numRead, int size = 17)
        {
            return buff.TryFixedDecodePrefixedBigInt(isSigned, out op, out numRead, true, size);
        }

        /// <summary>
        /// Try fixed-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="isSigned">Is the value is signed or not</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from buffer</param>
        /// <param name="asResult">Is the value encoded as Result or as Option</param>
        /// <param name="size">Total size of the encoded value</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryFixedDecodePrefixedBigInt(
            this Span<byte> buff, bool isSigned, out ValueTuple<Prefix, BigInteger> op, 
            out int numRead, bool asResult = false, int size = 17)
        {
            if (buff.TryDecodePrefix(out Prefix prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    numRead = 1;
                    op = default;
                    return true;
                }
                else
                {
                    buff[1..].FixedDecodeBigInt(isSigned, out BigInteger n, size - 1);
                    op = ValueTuple.Create(prefix, n);
                    numRead = size;
                    return true;
                }
            }
            else
            {
                numRead = default;
                op = default;
                return false;
            }
        }

        /// <summary>
        /// Returnes compact-encoded size of the value
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="asResult">Encode the value as Result or as Option</param>
        /// <returns>Compact-encded size of the value</returns>
        public static int CompactEncodedSize(
            this in ValueTuple<Prefix, BigInteger> op, bool asResult = false)
        {
            if (asResult)
            {
                return op.Item2.CompactEncodedSize() + 1;
            }
            else
            {
                return op.Item1.IsNone() ? 1 : op.Item2.CompactEncodedSize() + 1;
            }
        }

        /// <summary>
        /// Compact encode the value.
        /// </summary>
        /// <param name="op">Value to encode</param>
        /// <param name="buff">Buffer for encoding</param>
        /// <param name="asResult">Encode the value as Reslut or as Option</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int CompactEncode(
            this ValueTuple<Prefix, BigInteger> op, Span<byte> buff, bool asResult = false)
        {
            op.Item1.Encode(buff);
            if (!asResult && op.Item1.IsNone())
            {
                return 1;
            }
            return op.Item2.CompactEncode(buff[1..]) + 1;
        }

        /// <summary>
        /// Try compact-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryCompactDecodeOption(
            this Span<byte> buff,
            out ValueTuple<Prefix, BigInteger> op, out int numRead)
        {
            return buff.TryCompactDecodePrefixed(out op, out numRead);
        }

        /// <summary>
        /// Try compact-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryCompactDecodeResult(
            this Span<byte> buff,
            out ValueTuple<Prefix, BigInteger> op, out int numRead)
        {
            return buff.TryCompactDecodePrefixed(out op, out numRead, true);
        }

        /// <summary>
        /// Try compact-decode the value.
        /// </summary>
        /// <param name="buff">Buffer for decoding</param>
        /// <param name="op">Decoded value</param>
        /// <param name="numRead">Number of bytes read from the buffer</param>
        /// <param name="asResult">Is the value encoded as Result or as Option</param>
        /// <returns>Successfully decoded or not</returns>
        public static bool TryCompactDecodePrefixed(
            this Span<byte> buff, 
            out ValueTuple<Prefix, BigInteger> op, out int numRead, bool asResult = false)
        {
            if (buff.TryDecodePrefix(out var prefix))
            {
                if (!asResult && prefix.IsNone())
                {
                    op = default;
                    numRead = 1;
                    return true;
                }
                else if (buff[1..].TryCompactDecode(out BigInteger n, out numRead))
                {
                    ++numRead;
                    op = ValueTuple.Create(prefix, n);
                    return true;
                }
            }

            numRead = default;
            op = default;
            return false;
        }
    }
}
