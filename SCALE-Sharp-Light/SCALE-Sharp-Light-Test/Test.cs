using System.Numerics;
using System.Text;
using ScaleSharpLight;

namespace ScaleSharpLightTest
{
    public class ScaleLightTest
    {
        static void PrintBuff(Span<byte> buff)
        {
            Console.Write("0x");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write("{0:x}", buff[i]);
            }
            Console.Write("\n");
        }

        static void CheckEqual(params bool[] checks)
        {
            for (int i = 0; i < checks.Length; i++)
            {
                if (!checks[i])
                {
                    throw new Exception("Oooops!!");
                }
            }
            Console.WriteLine("Ok\n");
        }

        static void SignedIntTest()
        {
            Console.WriteLine("Signed integer test");
            sbyte i8 = -43;
            short i16 = -943;
            int i32 = -99432;
            long i64 = -94329999;
            var i128 = new BigInteger(-99999543232999);
            Span<byte> totalBuff = stackalloc byte[
                sizeof(sbyte) + sizeof(short) + sizeof(int) +
                sizeof(long) + i128.Fixed128EncodedSize()
            ];
            var pos = 0;
            pos += i8.FixedEncode(totalBuff[pos..]);
            pos += i16.FixedEncode(totalBuff[pos..]);
            pos += i32.FixedEncode(totalBuff[pos..]);
            pos += i64.FixedEncode(totalBuff[pos..]);
            i128.FixedEncode(true, totalBuff[pos..]);
            PrintBuff(totalBuff);
            totalBuff[..].FixedDecodeI8(out sbyte di8);
            totalBuff[1..].FixedDecodeI16(out short di16);
            totalBuff[3..].FixedDecodeI32(out int di32);
            totalBuff[7..].FixedDecodeI64(out long di64);
            totalBuff[15..].FixedDecodeBigInt(true, out BigInteger di128);
            Console.WriteLine($"{di8}, {di16}, {di32}, {di64}, {di128}");
            CheckEqual(i8 == di8, i16 == di16, i32 == di32, i64 == di64, i128 == di128);
        }

        static void UnsignedIntTest()
        {
            Console.WriteLine("Unsigned integer test");
            Span<byte> totalBuff = stackalloc byte[1 + 2 + 4 + 8 + 16];
            byte u8 = 39;
            ushort u16 = 9645;
            uint u32 = 99465909;
            ulong u64 = 9996435333699;
            var u128 = new BigInteger(9924943259996598299);
            totalBuff[0] = u8;
            u16.FixedEncode(totalBuff[1..]);
            u32.FixedEncode(totalBuff[3..]);
            u64.FixedEncode(totalBuff[7..]);
            u128.FixedEncode(false, totalBuff[15..]);
            PrintBuff(totalBuff);
            var pos = 0; 
            byte du8 = totalBuff[pos];
            pos += sizeof(byte);
            pos += totalBuff[pos..].FixedDecodeU16(out ushort du16);
            pos += totalBuff[pos..].FixedDecodeU32(out uint du32);
            pos += totalBuff[pos..].FixedDecodeU64(out ulong du64);
            pos += totalBuff[pos..].FixedDecodeBigInt(false, out BigInteger du128);
            Console.WriteLine($"{du8}, {du16}, {du32}, {du64}, {du128}");
            CheckEqual(u8 == du8, u16 == du16, u32 == du32, u64 == du64, u128 == du128);
        }

        static void OptionTest()
        {
            Console.WriteLine("Option test");
            var none = Option.None;
            var opi8 = Option.I8(-9);
            var opi16 = Option.I16(-99);
            var opi32 = Option.I32(-9999);
            var opi64 = Option.I64(-99999999);
            var opi128 = Option.IBig(-9999999999999999);
            var opu8 = Option.U8(9);
            var opu16 = Option.U16(99);
            var opu32 = Option.U32(9999);
            var opu64 = Option.U64(99999999);
            var opu128 = Option.UBig(9999999999999999);
            Span<byte> totalBuff = stackalloc byte[
                none.EncodedPrefixSize +
                opi8.FixedEncodedSize() + opi16.FixedEncodedSize() +
                opi32.FixedEncodedSize() + opi64.FixedEncodedSize() +
                opi128.Fixed128EncodedSize() + opu8.FixedEncodedSize() +
                opu16.FixedEncodedSize() + opu32.FixedEncodedSize() +
                opu64.FixedEncodedSize() + opu128.Fixed128EncodedSize()
            ];
            var pos = 1;
            none.Encode(totalBuff[..pos]);
            pos += opi8.FixedEncode(totalBuff[pos..]);
            pos += opi16.FixedEncode(totalBuff[pos..]);
            pos += opi32.FixedEncode(totalBuff[pos..]);
            pos += opi64.FixedEncode(totalBuff[pos..]);
            pos += opi128.FixedEncode(true, totalBuff[pos..]);
            pos += opu8.FixedEncode(totalBuff[pos..]);
            pos += opu16.FixedEncode(totalBuff[pos..]);
            pos += opu32.FixedEncode(totalBuff[pos..]);
            pos += opu64.FixedEncode(totalBuff[pos..]);
            opu128.FixedEncode(false, totalBuff[pos..]);
            PrintBuff(totalBuff);
            pos = 0;
            totalBuff[..++pos].TryDecodePrefix(out var dnone);
            var w = 0;
            totalBuff[pos..].TryFixedDecodeOptionI8(out (Prefix op, sbyte n) dopi8, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionI16(out (Prefix op, short n) dopi16, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionI32(out (Prefix op, int n) dopi32, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionI64(out (Prefix op, long n) dopi64, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionBigInt(true, out (Prefix op, BigInteger n) dopi128, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionU8(out (Prefix op, byte n) dopu8, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionU16(out (Prefix op, ushort n) dopu16, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionU32(out (Prefix op, uint n) dopu32, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionU64(out (Prefix op, ulong n) dopu64, out w);
            pos += w;
            totalBuff[pos..].TryFixedDecodeOptionBigInt(false, out (Prefix op, BigInteger n) dopu128, out w);
            CheckEqual(none.IsNone());
            Console.WriteLine($"{dnone.IsNone()}");
            Console.WriteLine(
                $"{dopi8.op.IsNone()}/{dopi8.n}, {dopi16.op.IsNone()}/{dopi16.n}," +
                $"{dopi32.op.IsNone()}/{dopi32.n}, {dopi64.op.IsNone()}/{dopi64.n}," +
                $"{dopi128.op.IsNone()}/{dopi128.n}"
            );
            CheckEqual(
                opi8.Item2 == dopi8.n, opi16.Item2 == dopi16.n,opi32.Item2 == dopi32.n, 
                opi64.Item2 == dopi64.n, opi128.Item2 == dopi128.n
            );
            Console.WriteLine(
                $"{dopu8.op.IsNone()}/{dopu8.n}, {dopu16.op.IsNone()}/{dopu16.n}," +
                $"{dopu32.op.IsNone()}/{dopu32.n}, {dopu64.op.IsNone()}/{dopu64.n}," +
                $"{dopu128.op.IsNone()}/{dopu128.n}"
            );
            CheckEqual(
                opu8.Item2 == dopu8.n, opu16.Item2 == dopu16.n, opu32.Item2 == dopu32.n,
                opu64.Item2 == dopu64.n, opu128.Item2 == dopu128.n
            );
        }

        static void BooleanTest()
        {
            Console.WriteLine("Boolean test");
            var trueBool = true;
            var trueBoolLen = trueBool.EncodedSize();
            Span<byte> buff = stackalloc byte[trueBoolLen + false.EncodedSize()];
            trueBool.Encode(buff[..trueBoolLen]);
            false.Encode(buff[trueBoolLen..]);
            buff[..trueBoolLen].TryDecodeBool(out bool btrueBool);
            buff[trueBoolLen..].TryDecodeBool(out bool bfalseBool);
            Console.WriteLine($"{btrueBool}, {bfalseBool}");
            CheckEqual(btrueBool, !bfalseBool);
        }

        static void CompactTest()
        {
            Console.WriteLine("Compact test");

            var n = Compact.CompactInteger(5897439);
            var size = n.CompactEncodedSize();
            Console.WriteLine(size);
            Console.WriteLine(n.CompactEncodingMode());
            Span<byte> buff = stackalloc byte[size];
            Console.WriteLine(n.CompactEncode(buff));
            PrintBuff(buff);
            Console.WriteLine(buff.TryCompactDecode(out BigInteger d, out var r));
            Console.WriteLine($"{d}, {r}");

            var n1 = Compact.CompactInteger(265317);
            var size1 = n1.CompactEncodedSize();
            Console.WriteLine(size1);
            Console.WriteLine(n1.CompactEncodingMode());
            Span<byte> buff1 = stackalloc byte[size1];
            Console.WriteLine(n1.CompactEncode(buff1));
            PrintBuff(buff1);
            Console.WriteLine(buff1.TryCompactDecode(out BigInteger d1, out var r1));
            Console.WriteLine($"{d1}, {r1}");

            var n2 = Compact.CompactInteger(96707030);
            var size2 = n2.CompactEncodedSize();
            Console.WriteLine(size2);
            Console.WriteLine(n2.CompactEncodingMode());
            Span<byte> buff2 = stackalloc byte[size2];
            Console.WriteLine(n2.CompactEncode(buff2));
            PrintBuff(buff2);
            Console.WriteLine(buff2.TryCompactDecode(out BigInteger d2, out var r2));
            Console.WriteLine($"{d2}, {r2}");

            var n3 = Compact.CompactInteger(9685057574830383202);
            var size3 = n3.CompactEncodedSize();
            Console.WriteLine(size3);
            Console.WriteLine(n3.CompactEncodingMode());
            Span<byte> buff3 = stackalloc byte[size3];
            Console.WriteLine(n3.CompactEncode(buff3));
            PrintBuff(buff3);
            Console.WriteLine(buff3.TryCompactDecode(out BigInteger d3, out var r3));
            Console.WriteLine($"{d3}, {r3}");

            var n4 = Compact.CompactInteger(6553123423435);
            var size4 = n4.CompactEncodedSize();
            Console.WriteLine(size4);
            Console.WriteLine(n4.CompactEncodingMode());
            Span<byte> buff4 = stackalloc byte[size4];
            Console.WriteLine(n4.CompactEncode(buff4));
            PrintBuff(buff4);
            Console.WriteLine(buff4.TryCompactDecode(out BigInteger d4, out var r4));
            Console.WriteLine($"{d4}, {r4}");

            var n5 = Compact.CompactInteger(63);
            var size5 = n5.CompactEncodedSize();
            Console.WriteLine(size5);
            Console.WriteLine(n5.CompactEncodingMode());
            Span<byte> buff5 = stackalloc byte[size5];
            Console.WriteLine(n5.CompactEncode(buff5));
            PrintBuff(buff5);
            Console.WriteLine(buff5.TryCompactDecode(out BigInteger d5, out var r5));
            Console.WriteLine($"{d5}, {r5}");

            CheckEqual(n == d, n1 == d1, n2 == d2, n3 == d3, n4 == d4, n5 == d5);

            var list = new List<byte>();
            list.AddRange(buff.ToArray());
            list.AddRange(buff1.ToArray());
            list.AddRange(buff2.ToArray());
            list.AddRange(buff3.ToArray());
            list.AddRange(buff4.ToArray());
            list.AddRange(buff5.ToArray());
            var total = new Span<byte>(list.ToArray());
            var receiverL = new List<BigInteger>();
            Console.WriteLine(total.TryMultipleCompactDecode(receiverL, out r));
            for (int i = 0; i < receiverL.Count; i++)
            {
                Console.WriteLine(receiverL[i]);
            }
            CheckEqual(
                n == receiverL[0], n1 == receiverL[1], n2 == receiverL[2],
                n3 == receiverL[3], n4 == receiverL[4], n5 == receiverL[5]
            );
            var receiverA = new BigInteger[6];
            Console.WriteLine(total.TryMultipleCompactDecode(receiverA, out var success, out r));
            for (int i = 0; i < receiverA.Length; i++)
            {
                Console.WriteLine(receiverA[i]);
            }
            CheckEqual(
                success == 6,
                n == receiverA[0], n1 == receiverA[1], n2 == receiverA[2],
                n3 == receiverA[3], n4 == receiverA[4], n5 == receiverA[5]
            );
        }

        static void OptionCompactTest()
        {
            Console.WriteLine("OptionCompact test");

            var c = Option.CompactInteger(132);
            var c1 = Option.CompactInteger(69045);
            Span<byte> buff = stackalloc byte[
                c.CompactEncodedSize() + 
                c1.CompactEncodedSize() + 
                Option.None.EncodedPrefixSize
            ];
            var r = 0;
            r += c.CompactEncode(buff);
            r += Option.None.Encode(buff[r..]);
            c1.CompactEncode(buff[r..]);
            PrintBuff(buff);
            var pos = 0;
            buff.TryCompactDecodePrefixed(out (Prefix op, BigInteger n) d, out r);
            pos += r;
            buff[pos..].TryCompactDecodePrefixed(out (Prefix op, BigInteger n) d1, out r);
            pos += r;
            buff[pos..].TryCompactDecodePrefixed(out (Prefix op, BigInteger n) d2, out _);
            Console.WriteLine($"{d.IsNone()}/{d.n}, {d1.IsNone()}/{d1.n}, {d2.IsNone()}/{d2.n}");
            CheckEqual(c.Item2 == d.n, d1.IsNone(), c1.Item2 == d2.n);
        }

        static void VectorTest()
        {
            Console.WriteLine("Vector test");
            {
                var l = new List<sbyte>
                {
                    -1, -2, -3, -4, -5, -6, -7, -8, -9, -10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                };
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecI8(out sbyte[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l));
            }

            {
                var l = new List<short>
                {
                    -1, -2, -3, -4, -5, -6, -7, -8, -9, -10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                };
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecI16(out short[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l));
            }

            {
                var l = new Span<int>(new int[]
                {
                    -1, -2, -3, -4, -5, -6, -7, -8, -9, -10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                });
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecI32(out int[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l.ToArray()));
            }

            {
                var l = new Span<long>(new long[]
                {
                    -1, -2, -3, -4, -5, -6, -7, -8, -9, -10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                });
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecI64(out long[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l.ToArray()));
            }

            {
                Span<byte> l = stackalloc byte[]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
                };
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecU8(out byte[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l.ToArray()));
            }

            {
                Span<ushort> l = stackalloc ushort[]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                };
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecU16(out ushort[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l.ToArray()));
            }

            {
                var l = new List<uint>
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                };
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecU32(out uint[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l));
            }

            {
                var l = new List<ulong>
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                };
                var encSize = l.EncodedSize();
                Span<byte> buff = stackalloc byte[encSize];
                l.Encode(buff);
                PrintBuff(buff);
                buff.TryDecodeVecU64(out ulong[] decArr, out var r);
                Console.Write("[ ");
                for (int i = 0; i < decArr.Length; i++)
                {
                    Console.Write($"{decArr[i]}, ");
                }
                Console.WriteLine("]");
                CheckEqual(decArr.SequenceEqual(l));
            }

            {
                var nestedList = new List<(Prefix p, List<ulong> l)>
                {
                    new List<ulong>{0, 1, 2, 3, 4, 5}.ToOption(),
                    new List<ulong>{6, 7, 8, 9}.ToOption(),
                    Option.NoneAndNew<List<ulong>>(),
                    new List<ulong>{10, 11, 12, 13}.ToOption(),
                    new List<ulong>{14, 15}.ToOption(),
                    Option.NoneAndNew<List<ulong>>(),
                };
                var vecPre = nestedList.VectorPrefix();
                var totalSize = vecPre.CompactEncodedSize();
                for (int i = 0; i < nestedList.Count; i++)
                {
                    totalSize += nestedList[i].p.EncodedPrefixSize;
                    if (!nestedList[i].p.IsNone())
                    {
                        totalSize += nestedList[i].l.EncodedSize();
                    }
                }
                Span<byte> nestedBuff = stackalloc byte[totalSize];
                var pos = 0;
                pos += vecPre.CompactEncode(nestedBuff);
                for (int i = 0; i < nestedList.Count; i++)
                {
                    pos += nestedList[i].p.Encode(nestedBuff[pos..]);
                    if (!nestedList[i].IsNone())
                    {
                        pos += nestedList[i].l.Encode(nestedBuff[pos..]);
                    }
                }
                PrintBuff(nestedBuff);
                pos = 0;
                nestedBuff.TryDecodeVectorPrefix(out BigInteger len, out var r);
                pos += r;
                var decNested = new List<(Prefix p, List<ulong> l)>();
                for (int i = 0; i < len; i++)
                {
                    if (nestedBuff[pos..].TryDecodePrefix(out Prefix prx))
                    {
                        pos += prx.EncodedPrefixSize;
                        var lst = Array.Empty<ulong>();
                        if (!prx.IsNone())
                        {
                            if (nestedBuff[pos..].TryDecodeVecU64(out lst, out r))
                            {
                                pos += r;
                            }
                        }
                        decNested.Add((prx, lst.ToList()));
                    }
                }
                Console.Write("[ ");
                for (int i = 0; i < decNested.Count; i++)
                {
                    Console.Write($"{decNested[i].IsNone()}/");
                    Console.Write("[ ");
                    for (int j = 0; j < decNested[i].l.Count; j++)
                    {
                        Console.Write($"{decNested[i].l[j]}, ");
                    }
                    Console.Write("], ");
                }
                Console.WriteLine("]");
            }

            {
                var largeList = new List<ulong>();
                for (ulong i = 0; i < 128/*int.MaxValue / 8 - 8*/; i++)
                {
                    largeList.Add(i);
                }
                var largeSize = largeList.EncodedSize();
                var largeBuff = new Span<byte>(new byte[largeSize]);
                //Span<byte> largeBuff = stackalloc byte[largeSize];
                largeList.Encode(largeBuff);
                //PrintBuff(largeBuff);
                largeBuff.TryDecodeVecU64(out var decLargeList, out _);
                for (int i = 0; i < decLargeList.Length; i++)
                {
                    if (largeList[i] != decLargeList[i])
                    {
                        throw new Exception("Ooooops!!");
                    }
                }
            }
        }

        static void StringTest()
        {
            Console.WriteLine("String test");
            var str = "Hello! Moemoe, Kyunkyun!!";
            var strList = Encoding.UTF8.GetBytes(str).ToList();
            (Prefix op, List<byte> ls) = strList.ToOption();
            var prefixSize = op.EncodedPrefixSize;
            Span<byte> strBuff = stackalloc byte[prefixSize + ls.EncodedSize()];
            op.Encode(strBuff);
            ls.Encode(strBuff[prefixSize..]);
            PrintBuff(strBuff);
            strBuff.TryDecodePrefix(out Prefix pfx);
            strBuff[prefixSize..].TryDecodeVecU8(out var decStrList, out _);
            var decStr = Encoding.UTF8.GetString(decStrList.ToArray());
            Console.WriteLine(decStr);
            CheckEqual("Hello! Moemoe, Kyunkyun!!" == decStr);
        }

        static void TupleTest()
        {
            Console.WriteLine("Tuple test");
            var i32u32 = (32, 32u);
            Span<byte> buff0 = stackalloc byte[8];
            i32u32.Item1.FixedEncode(buff0[..4]);
            i32u32.Item2.FixedEncode(buff0[4..]);
            buff0[..4].FixedDecodeI32(out int i32);
            buff0[4..].FixedDecodeU32(out uint u32);
            CheckEqual(i32u32.Item1 == i32, i32u32.Item2 == u32);

            var u64bool = (64ul, true);
            Span<byte> buff1 = stackalloc byte[8 + true.EncodedSize()];
            u64bool.Item1.FixedEncode(buff1[..8]);
            u64bool.Item2.Encode(buff1[8..]);
            buff1[..8].FixedDecodeU64(out ulong u64);
            buff1[8..].TryDecodeBool(out bool boolean);
            CheckEqual(u64bool.Item1 == u64, u64bool.Item2 == boolean);
        }

        public enum Error : byte
        {
            Overflow,
            Underflow,
        }

        public enum CSTypes : byte
        {
            Int,
            Uint,
            Long,
            Ulong,
        }

        public static void EnumTest()
        {
            Console.WriteLine("Enum test");
            var of = Error.Overflow;
            var uf = Error.Underflow;
            Span<byte> buff = stackalloc byte[2];
            buff[0] = (byte)of;
            buff[1] = (byte)uf;
            var decOf = (Error)buff[0];
            var decUf = (Error)buff[1];
            Console.WriteLine($"{decOf}, {decUf}");
            CheckEqual(decOf == Error.Overflow, decUf == Error.Underflow);

            var elem1 = TaggedUnion.Element(CSTypes.Int, 32);
            var elem2 = TaggedUnion.Element(CSTypes.Uint, 32u);
            var elem3 = TaggedUnion.Element(CSTypes.Long, 64L);
            var elem4 = TaggedUnion.Element(CSTypes.Ulong, 64ul);
            Span<byte> buff1 = stackalloc byte[5 + 5 + 9 + 9];
            var pos = 0;
            buff1[pos++] = (byte)elem1.Item1;
            pos += elem1.Item2.FixedEncode(buff1[pos..]);
            buff1[pos++] = (byte)elem2.Item1;
            pos += elem2.Item2.FixedEncode(buff1[pos..]);
            buff1[pos++] = (byte)elem3.Item1;
            pos += elem3.Item2.FixedEncode(buff1[pos..]);
            buff1[pos++] = (byte)elem4.Item1;
            elem4.Item2.FixedEncode(buff1[pos..]);
            PrintBuff(buff1);

            pos = 0;
            int elem1dec = 0;
            uint elem2dec = 0;
            long elem3dec = 0; 
            ulong elem4dec = 0;
            var decodeFunc = (CSTypes tag, Span<byte> buff) =>
            {
                return tag switch
                {
                    CSTypes.Int => buff.FixedDecodeI32(out elem1dec),
                    CSTypes.Uint => buff.FixedDecodeU32(out elem2dec),
                    CSTypes.Long => buff.FixedDecodeI64(out elem3dec),
                    CSTypes.Ulong => buff.FixedDecodeU64(out elem4dec),
                    _ => throw new Exception("Nooooooo!!")
                };
            };

            var elem1Tag = (CSTypes)buff1[pos++];
            pos += decodeFunc(elem1Tag, buff1[pos..]);
            var elem2Tag = (CSTypes)buff1[pos++];
            pos += decodeFunc(elem2Tag, buff1[pos..]);
            var elem3Tag = (CSTypes)buff1[pos++];
            pos += decodeFunc(elem3Tag, buff1[pos..]);
            var elem4Tag = (CSTypes)buff1[pos++];
            decodeFunc(elem4Tag, buff1[pos..]);
            Console.WriteLine(
                $"{elem1Tag}/{elem1dec}, {elem2Tag}/{elem2dec}, " +
                $"{elem3Tag}/{elem3dec}, {elem4Tag}/{elem4dec}");
            CheckEqual(
                elem1Tag == CSTypes.Int, elem1dec == 32,
                elem2Tag == CSTypes.Uint, elem2dec == 32u,
                elem3Tag == CSTypes.Long, elem3dec == 64L,
                elem4Tag == CSTypes.Ulong, elem4dec == 64ul
            );
        }

        public class CallRequest
        {
            public List<byte> dest = new();
            public BigInteger value;
            public BigInteger gasLimit;
            public (Prefix, BigInteger) storageDepositLimit;
            public List<byte> inputData = new();
        }

        public static void StructuredTest()
        {
            Console.WriteLine("Structured test");
            var callRequest = new CallRequest
            {
                dest = new List<byte>
                {
                    0xd4, 0x35, 0x93, 0xc7, 0x15,  0xfd, 0xd3, 0x1c, 0x61, 0x14,
                    0x1a, 0xbd, 0x04, 0xa9, 0x9f,  0xd6, 0x82, 0x2c, 0x85, 0x58,
                    0x08, 0x54, 0xcc, 0xde, 0x39,  0xa5, 0x68, 0x4e, 0x7a, 0x56,
                    0xda, 0x27
                },
                value = new BigInteger(0),
                gasLimit = new BigInteger(37500000000ul),
                storageDepositLimit = default,
                inputData = new List<byte>(Encoding.UTF8.GetBytes("thisDataIsNice"))
            };
            Span<byte> buff = stackalloc byte[
                callRequest.dest.EncodedSize() +
                callRequest.value.CompactEncodedSize() +
                callRequest.gasLimit.CompactEncodedSize() +
                callRequest.storageDepositLimit.CompactEncodedSize() +
                callRequest.inputData.EncodedSize()    
            ];
            var pos = 0;
            pos += callRequest.dest.Encode(buff[pos..]);
            pos += callRequest.value.CompactEncode(buff[pos..]);
            pos += callRequest.gasLimit.CompactEncode(buff[pos..]);
            pos += callRequest.storageDepositLimit.CompactEncode(buff[pos..]);
            callRequest.inputData.Encode(buff[pos..]);
            PrintBuff(buff);
            pos = 0;
            if (!buff[pos..].TryDecodeVecU8(out var decDest, out int r))
            {
                throw new Exception("Ooooopppsssss!!");
            }
            pos += r;
            if (!buff[pos..].TryCompactDecode(out BigInteger decValue, out r))
            {
                throw new Exception("Ooooopppsssss!!");
            }
            pos += r;
            if (!buff[pos..].TryCompactDecode(out BigInteger decGasLim, out r))
            {
                throw new Exception("Ooooopppsssss!!");
            }
            pos += r;
            if (!buff[pos..].TryCompactDecodePrefixed(
                out (Prefix pfx, BigInteger val) decStorageDepLim, out r))
            {
                throw new Exception("Ooooopppsssss!!");
            }
            pos += r;
            if (!buff[pos..].TryDecodeVecU8(out var decInput, out r))
            {
                throw new Exception("Ooooopppsssss!!");
            }
            Console.WriteLine($"{decValue}, {decGasLim}, {decStorageDepLim}");
            CheckEqual(
                decValue == callRequest.value,
                decGasLim == callRequest.gasLimit,
                decStorageDepLim.IsNone()
            );
            for (int i = 0; i < decDest.Length; i++)
            {
                if (decDest[i] != callRequest.dest[i])
                {
                    throw new Exception("Ooooops!!");
                }
            }
            for (int i = 0; i < decInput.Length; i++)
            {
                if (decInput[i] != callRequest.inputData[i])
                {
                    throw new Exception("Ooooops!!");
                }
            }
        }

        public static void ResultTest()
        {
            var stringOk = Result.Ok(null);
            var s = stringOk.Item2;
            CheckEqual(s != null);
            var stringErr = Result.Err(null);
            s = stringErr.Item2;
            CheckEqual(s != null);

            var okEmpty = Result.Ok();
            var errEmpty = Result.Err();
            var okU128Compact = Result.Ok(Compact.CompactInteger(22222u));
            var errEnum = Result.Err((byte)Error.Underflow);
            Span<byte> buff = stackalloc byte[
                okEmpty.EncodedPrefixSize +
                errEmpty.EncodedPrefixSize +
                okU128Compact.CompactEncodedSize(true) +
                errEnum.FixedEncodedSize(true)
            ];
            var pos = 0;
            pos += okEmpty.Encode(buff[pos..]);
            pos += errEmpty.Encode(buff[pos..]);
            pos += okU128Compact.CompactEncode(buff[pos..], true);
            errEnum.FixedEncode(buff[pos..], true);
            PrintBuff(buff);
            pos = 0;
            buff[pos++..].TryDecodePrefix(out var decOk);
            buff[pos++..].TryDecodePrefix(out var decErr);
            buff[pos..].TryCompactDecodeResult(out var decOk128, out var r);
            pos += r;
            buff[pos..].TryFixedDecodeResultU8(out var decErrEnum, out r);
            Console.WriteLine($"{decOk.IsOk()}, {decErr.IsOk()}, " +
                $"{decOk128.IsOk()}/{decOk128.Item2}, {decErrEnum.IsOk()}/{decErrEnum.Item2}");
            CheckEqual(decOk.IsOk(), !decErr.IsOk(), decOk128.Item2 == okU128Compact.Item2,
                decErrEnum.Item2 == errEnum.Item2);
        }

        enum Types
        {
            Bool,
            I8,
            U8,
            I16,
            U16,
            I32,
            U32,
            I64,
            U64,
            Struct
        }

        static void Main()
        {
            if (!BitConverter.IsLittleEndian)
            {
                Console.WriteLine("Oooops!! This machine is big endian!!");
            }

            SignedIntTest();
            UnsignedIntTest();
            OptionTest();
            BooleanTest();
            CompactTest();
            OptionCompactTest();
            VectorTest();
            StringTest();
            TupleTest();
            EnumTest();
            StructuredTest();
            ResultTest();
        }
    }
}