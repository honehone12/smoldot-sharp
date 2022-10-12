using SmoldotSharp.JsonRpc;
using ScaleSharpLight;

namespace SmoldotSharpJsonTest
{
    internal class Test
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

        static void HexToBytesConvertTest()
        {
            var hex = "01020304050a0b0c0d0eaabbccddee5566778899";
            Span<byte> buff = stackalloc byte[hex.Length / 2];
            var ok = hex.AsSpan().TryHexToBytes(buff, out var w);
            PrintBuff(buff);
            Span<byte> expected = stackalloc byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
                0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0x55, 0x66, 0x77, 0x88, 0x99
            };
            CheckEqual(ok, w == 20, buff.SequenceEqual(expected));
        }

        static void SS58DecodeTest()
        {
            var addr = "5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY";
            var pubKeyHex = "d43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d";
            Span<byte> pubKey = stackalloc byte[pubKeyHex.Length / 2];
            var ok = pubKeyHex.AsSpan().TryHexToBytes(pubKey, out var w);
            ok = addr.AsSpan().TrySS58Decode(out var bytes, out var ident);
            CheckEqual(ok, ident == 42, pubKey.SequenceEqual(bytes));
        }

        static void BytesToHexTest()
        {
            Span<byte> bytes = stackalloc byte[]
            {
                0x00, 0x01, 0x0a, 0x11, 0xaa, 0xaf, 0xf0, 0xff
            };
            var expected = "00010a11aaaff0ff";
            Span<char> buff = stackalloc char[bytes.Length * 2];
            bytes.BytesToHex(buff, out var w);
            CheckEqual(w == bytes.Length * 2, buff.ToString().Equals(expected));
        }

        static void DeserializeTest()
        {
            var ok = "0x34f".TryDeserialize(out var n, false);
            Console.WriteLine($"{n}");
            CheckEqual(ok, n == 847);
            ok = "0x1e".TryDeserialize(out n, false);
            Console.WriteLine($"{n}");
            CheckEqual(ok, n == 30);

        }

        static void EraTest()
        {
            {
                var e = Era.New(32768, 20000);
                Console.WriteLine(e.encoded);

                Span<byte> buff = stackalloc byte[2];
                e.encoded.FixedEncode(buff);
                for (int i = 0; i < 2; i++)
                {
                    Console.WriteLine(buff[i]);
                }
                CheckEqual(buff[0] == 14 + 2500 % 16 * 16, buff[1] == 2500 / 16);
            }

            {
                var e = Era.New(64, 42);
                Console.WriteLine(e.encoded);

                Span<byte> buff = stackalloc byte[2];
                e.encoded.FixedEncode(buff);
                for (int i = 0; i < 2; i++)
                {
                    Console.WriteLine(buff[i]);
                }
                CheckEqual(buff[0] == 5 + 42 % 16 * 16, buff[1] == 42 / 16);
            }

            {
                var e = Era.New(12345, 42);
                Console.WriteLine(e.encoded);

                Span<byte> buff = stackalloc byte[2];
                e.encoded.FixedEncode(buff);
                for (int i = 0; i < 2; i++)
                {
                    Console.WriteLine(buff[i]);
                }
                CheckEqual(buff[0] == 173, buff[1] == 0);
            }

            {
                var e = Era.New(321, 86);
                Console.WriteLine(e.encoded);

                Span<byte> buff = stackalloc byte[2];
                e.encoded.FixedEncode(buff);
                for (int i = 0; i < 2; i++)
                {
                    Console.WriteLine(buff[i]);
                }
                CheckEqual(buff[0] == 104, buff[1] == 5);
            }
        }

        static void Main(string[] args)
        {
            HexToBytesConvertTest();
            SS58DecodeTest();
            BytesToHexTest();
            DeserializeTest();
            EraTest();
        }
    }
}