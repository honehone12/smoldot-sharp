using System.Text;
using System.Numerics;
using ScaleSharpLight;
using System;

namespace ScaleSharpLightExamples
{
    public partial class ScaleLightExamples
    {
        static void FixedWidthIntegers()
        {
            sbyte i8 = 69;
            ushort u16 = 42;
            uint u32 = 16777215u;
            var u128 = new BigInteger(16777215u);
            Span<byte> buff = stackalloc byte[
                sizeof(sbyte) + sizeof(ushort) + sizeof(uint) +
                u128.Fixed128EncodedSize()
            ];
            var pos = 0;
            pos += i8.FixedEncode(buff);
            pos += u16.FixedEncode(buff[pos..]);
            pos += u32.FixedEncode(buff[pos..]);
            u128.FixedEncode(false, buff[pos..]);
            PrintBuff(buff);
            CheckBuff(buff, 
                0x45, 0x2a, 0x00, 0xff, 0xff, 0xff, 0x00,
                0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            );

            pos = 0;
            pos += buff[pos..].FixedDecodeI8(out var decI8);
            pos += buff[pos..].FixedDecodeU16(out var decU16);
            pos += buff[pos..].FixedDecodeU32(out var decU32);
            buff[pos..].FixedDecodeBigInt(false, out var decU128);
            CheckEqual(i8 == decI8, u16 == decU16, u32 == decU32, u128 == decU128);
        }

        static void CompactIntegers()
        {
            // CompactInteger is acually BigInteger in ScaleSharpLight.
            // Compact.CompactInteger returns Unsigned BigInteger.
 
            var cpt0 = Compact.CompactInteger(0);
            var cpt1 = Compact.CompactInteger(1);
            var cpt42 = Compact.CompactInteger(42);
            var cpt69 = Compact.CompactInteger(69);
            var cpt65535 = Compact.CompactInteger(65535);
            var cpt100000000000000 = Compact.CompactInteger(100000000000000);
            Span<byte> buff = stackalloc byte[
                cpt0.CompactEncodedSize() + cpt1.CompactEncodedSize() +
                cpt42.CompactEncodedSize() + cpt69.CompactEncodedSize() +
                cpt65535.CompactEncodedSize() + cpt100000000000000.CompactEncodedSize()
            ];
            var pos = 0;
            pos += cpt0.CompactEncode(buff);
            pos += cpt1.CompactEncode(buff[pos..]);
            pos += cpt42.CompactEncode(buff[pos..]);
            pos += cpt69.CompactEncode(buff[pos..]);
            pos += cpt65535.CompactEncode(buff[pos..]);
            cpt100000000000000.CompactEncode(buff[pos..]);
            PrintBuff(buff);
            CheckBuff(buff,
                0x00, 0x04, 0xa8, 0x15, 0x01, 0xfe, 0xff, 0x03,
                0x00, 0x0b, 0x00, 0x40, 0x7a, 0x10, 0xf3, 0x5a
            );

            pos = 0;
            // Try single decode.
            if (!buff[pos..].TryCompactDecode(out var dec0, out var numRead))
            {
                // Error
            }
            pos += numRead;
            var receiver = new List<BigInteger>();
            // Try multiple decode.
            if (!buff[pos..].TryMultipleCompactDecode(receiver, out numRead))
            {
                // Error
            }
            CheckEqual(
                cpt0 == dec0, receiver.Count == 5, receiver[0] == cpt1,
                receiver[1] == cpt42, receiver[2] == cpt69, receiver[3] == cpt65535,
                receiver[4] == cpt100000000000000
            );
        }

        static void Booleans()
        {
            var bTrue = true;
            // actually just returns 1
            var boolSize = bTrue.EncodedSize(); 
            Span<byte> buff = stackalloc byte[boolSize * 2];
            bTrue.Encode(buff);
            false.Encode(buff[boolSize..]);
            PrintBuff(buff);
            CheckBuff(buff, 0x01, 0x00);

            if(!buff.TryDecodeBool(out var decTrue))
            {
                // returns false if value is not 0x01.
                // Error
            }
            if (!buff[boolSize..].TryDecodeBool(out var decFalse))
            {
                // returns false if value is not 0x00.
                // Error
            }
            CheckEqual(decTrue, !decFalse);
        }

        static void Options()
        {
            // Option<T> and Result<T> are sigle byte prefixed value in scale.
            // Thus, these two has same structure and actually both are just tuple of
            // struct named Prefix and value. (Prefix, T) in ScaleSharpLight.

            var some1u64 = Option.U64(42); // this is (Prefix, 1ul)
            var none = Option.None; // this is just Prefix;
            Span<byte> buff = stackalloc byte[
                some1u64.FixedEncodedSize() + // prefix size + value size 
                none.EncodedPrefixSize // just prefix size
            ];
            var pos = 0;
            pos += some1u64.FixedEncode(buff); // encode as Option
            none.Encode(buff[pos..]); // encode as Option
            PrintBuff(buff);
            CheckBuff(buff, 0x01, 0x2a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);

            pos = 0;
            if (!buff.TryFixedDecodeOptionU64(out var decOp, out var numRead))
            {
                // Error
            }
            pos += numRead;
            if (buff[pos..].TryDecodePrefix(out var decNone))
            {
                // Error
            }
            CheckEqual(decNone.IsNone(), !decOp.IsNone(), decOp.Item2 == some1u64.Item2);
        }

        static void Results()
        {
            // To encode as Result, give asResult flag for
            // FixedEncodedSize and FixedEncode functions.
            // This will change behavior for same prefix between Result and Option.

            var ok = Result.Ok();
            var ok42u32 = Result.Ok(42u);
            var errFalse = Result.Err(false);
            var err = Result.Err();
            Span<byte> buff = stackalloc byte[
                ok.EncodedPrefixSize + // just prefix
                ok42u32.FixedEncodedSize(true) + // prefix + value
                errFalse.FixedEncodedSize(true) + // prefix + value
                err.EncodedPrefixSize // just prefix
            ];
            var pos = 0;
            pos += ok.Encode(buff);
            pos += ok42u32.FixedEncode(buff[pos..], true); 
            pos += errFalse.FixedEncode(buff[pos..], true);
            err.Encode(buff[pos..]);
            PrintBuff(buff);
            CheckBuff(buff, 0x00, 0x00, 0x2a, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01);

            pos = 0;
            if (!buff.TryDecodePrefix(out var decOk))
            {
                // Error
            }
            if (!buff[++pos..].TryFixedDecodeResultU32(out var decOk42, out var numRead))
            {
                // Error
            }
            pos += numRead;
            if (!buff[pos..].TryFixedDecodeResultBool(out var decErrFalse, out numRead))
            {
                // Error
            }
            pos += numRead;
            if (!buff[pos..].TryDecodePrefix(out var decErr))
            {
                // Error
            }
            CheckEqual(
                decOk.IsOk(), decOk42.IsOk(), ok42u32.Item2 == decOk42.Item2,
                !decErrFalse.IsOk(), !decErrFalse.Item2, !decErr.IsOk()
            );
        }

        static void BasicVectors()
        {
            // Vec<T> is List<T> or Span<T> while encoding.

            var vecU16 = new List<ushort>
            {
                4, 8, 15, 16, 23, 42
            };
            Span<byte> buff = stackalloc byte[vecU16.EncodedSize()];
            vecU16.Encode(buff);
            PrintBuff(buff);
            CheckBuff(buff, 
                0x18, 0x04, 0x00, 0x08, 0x00, 0x0f, 0x00, 0x10,
                0x00, 0x17, 0x00, 0x2a, 0x00
            );

            // T[] for Vec<T> or List<(Prefix, T)> for Vec<Option<T>> while encoding.

            buff.TryDecodeVecU16(out var decVec, out var numRead);
            CheckEqual(vecU16.Count == decVec.Length, 
                vecU16[0] == decVec[0], vecU16[1] == decVec[1], vecU16[2] == decVec[2], 
                vecU16[3] == decVec[3], vecU16[4] == decVec[4], vecU16[5] == decVec[5]
            );
        }

        static void Strings()
        {
            // Encode as Vec<u8>

            var str = "ScaleSharpLight";
            var strList = new List<byte>(Encoding.UTF8.GetBytes(str));
            Span<byte> buff = stackalloc byte[strList.EncodedSize()];
            strList.Encode(buff);
            PrintBuff(buff);

            buff.TryDecodeVecU8(out var decList, out var numRead);
            var decStr = Encoding.UTF8.GetString(decList.ToArray());
            Console.WriteLine(decStr);
            CheckEqual(decStr == str);
        }

        public enum Color : byte
        {
            Blue,
            Yellow,
            Green,
            Red
        }

        public static void BasicEnums()
        {
            // simple enum can be encoded as byte

            var blue = Color.Blue;
            var red = Color.Red;
            Span<byte> buff = stackalloc byte[2];
            buff[0] = (byte)blue;
            buff[1] = (byte)red;
            PrintBuff(buff);

            var decBlue = (Color)buff[0];
            var decRed = (Color)buff[1];
            CheckEqual(decBlue == Color.Blue, decRed == Color.Red);
        }
    }
}
