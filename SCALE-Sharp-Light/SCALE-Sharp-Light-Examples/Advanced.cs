using System;
using System.Numerics;
using System.Text;
using ScaleSharpLight;

namespace ScaleSharpLightExamples
{
    public partial class ScaleLightExamples
    {
        public static void Tuples()
        {
            // Just encode each values in tuple

            (BigInteger idx, bool val) = (3, false);
            Span<byte> buff = stackalloc byte[idx.CompactEncodedSize() + 1];
            var pos = 0;
            pos += idx.CompactEncode(buff);
            val.Encode(buff[pos..]);
            PrintBuff(buff);
            CheckBuff(buff, 0x0c, 0x00);

            pos = 0;
            if (!buff.TryCompactDecode(out var decIdx, out var numRead))
            {
                // Error
            }
            pos += numRead;
            if (!buff.TryDecodeBool(out var decVal))
            {
                // Error
            }
            CheckEqual(decIdx == idx, decVal == val);
        }

        public class CallRequest
        {
            public byte[] dest = Array.Empty<byte>();
            public BigInteger value;
            public BigInteger gasLimit;
            public (Prefix, BigInteger) storageDepositLimit;
            public List<byte> inputData = new();
        }

        public static void Structs()
        {
            // Just encode each fields of class or struct

            var callRequest = new CallRequest
            {
                dest = new byte[32]
                {
                    0x22, 0x22, 0x22, 0x22, 0x22,  0x22, 0x22, 0x22, 0x22, 0x22,
                    0x22, 0x22, 0x22, 0x22, 0x22,  0x22, 0x22, 0x22, 0x22, 0x22,
                    0x22, 0x22, 0x22, 0x22, 0x22,  0x22, 0x22, 0x22, 0x22, 0x22,
                    0x22, 0x22
                },
                value = new BigInteger(0),
                gasLimit = new BigInteger(37500000000ul),
                storageDepositLimit = default,
                inputData = new List<byte>(Encoding.UTF8.GetBytes("thisDataIsNice"))
            };
            var destAsSpan = new Span<byte>(callRequest.dest);
            Span<byte> buff = stackalloc byte[
                destAsSpan.EncodedSize() +
                callRequest.value.CompactEncodedSize() +
                callRequest.gasLimit.CompactEncodedSize() +
                callRequest.storageDepositLimit.CompactEncodedSize() +
                callRequest.inputData.EncodedSize()
            ];
            var pos = 0;
            pos += destAsSpan.Encode(buff[pos..]);
            pos += callRequest.value.CompactEncode(buff[pos..]);
            pos += callRequest.gasLimit.CompactEncode(buff[pos..]);
            pos += callRequest.storageDepositLimit.CompactEncode(buff[pos..]);
            callRequest.inputData.Encode(buff[pos..]);
            PrintBuff(buff);
            
            pos = 0;
            if (!buff[pos..].TryDecodeVecU8(out var decDest, out int r))
            {
                // Error
            }
            pos += r;
            if (!buff[pos..].TryCompactDecode(out BigInteger decValue, out r))
            {
                // Error
            }
            pos += r;
            if (!buff[pos..].TryCompactDecode(out BigInteger decGasLim, out r))
            {
                // Error
            }
            pos += r;
            if (!buff[pos..].TryCompactDecodePrefixed(
                out (Prefix pfx, BigInteger val) decStorageDepLim, out r))
            {
                // Error
            }
            pos += r;
            if (!buff[pos..].TryDecodeVecU8(out var decInput, out r))
            {
                // Error
            }

            CheckEqual(
                decValue == callRequest.value, decGasLim == callRequest.gasLimit,
                decStorageDepLim.IsNone(), decDest.SequenceEqual(callRequest.dest), 
                decInput.SequenceEqual(callRequest.inputData)
            );
        }

        public enum IntOrBool : byte
        {
            Int,
            Bool,
        }

        public static (IntOrBool, (byte, bool)) AdvancedEnums()
        {
            // Structured enum can be encoded as tagged value by one-byte-prefix.
            // Thus, they are just tuple of enum and value in ScaleSharpLight. (enum, T)

            var int42 = TaggedUnion.Element(IntOrBool.Int, (byte)42);
            var boolTrue = TaggedUnion.Element(IntOrBool.Bool, true);
            Span<byte> buff = stackalloc byte[1 + 1 + 1 + 1];
            var pos = 0;
            buff[pos++] = (byte)int42.Item1;
            buff[pos++] = int42.Item2;
            buff[pos++] = (byte)boolTrue.Item1;
            boolTrue.Item2.Encode(buff[pos..]);
            PrintBuff(buff);
            CheckBuff(buff, 0x00, 0x2a, 0x01, 0x01);

            // To decode unknown value types, need to prepare all possible buffer.
            byte decByte = 0;
            bool decBool = false;
            var decodeFunc = (IntOrBool tag, Span<byte> buff) =>
            {
                if (tag == IntOrBool.Int)
                {
                    decByte = buff[0];
                    return 1;
                }
                else if (tag == IntOrBool.Bool)
                {
                    if (!buff.TryDecodeBool(out decBool))
                    {
                        // Error
                        return 0;
                    }
                    return 1;
                }
                else
                {
                    // Error
                    return 0;
                }
            };
            pos = 0;
            var tag1 = (IntOrBool)buff[pos++];
            pos += decodeFunc(tag1, buff[pos..]);
            var tag2 = (IntOrBool)buff[pos++];
            decodeFunc(tag2, buff[pos..]);
            CheckEqual(
                int42.Item1 == tag1,
                int42.Item2 == decByte,
                boolTrue.Item1 == tag2,
                decBool
            );

            // These two are even same type.
            var tupled1 = TaggedUnion.Element(tag1, (decByte, default(bool)));
            var tupled2 = TaggedUnion.Element(tag2, (default(byte), decBool));
            return tupled2;
        }

        public static void AdvancedVectors()
        {
            // Vec<T> is actually values prefixed by CompactInteger of length.
            // Just put another prefix makes List<List<T>> encodable.
            // And same way can make Option<List<T>> encodable. 

            var nestedList = new List<(Prefix p, List<ulong> l)>
            {
                new List<ulong>{0, 1, 2, 3, 4, 5}.ToOption(),
                new List<ulong>{6, 7, 8, 9}.ToOption(),
                Option.NoneAndNew<List<ulong>>(), // for class, NonAndDefault for struct
                new List<ulong>{10, 11, 12, 13}.ToOption(),
                new List<ulong>{14, 15}.ToOption(),
                Option.NoneAndNew<List<ulong>>(),
            };
            // returns BigInteger
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
    }
}
