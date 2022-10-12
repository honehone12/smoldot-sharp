using ScaleSharpLight;

namespace ScaleSharpLightExamples
{
    public partial class ScaleLightExamples
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

        static void CheckBuff(Span<byte> buff, params int[] hex)
        {
            for (int i = 0; i < buff.Length; i++)
            {
                if (buff[i] != hex[i])
                {
                    throw new Exception("Oooops!!");
                }
            }
            Console.WriteLine("Ok\n");
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

        static void Main()
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new Exception("Oooops!! This machine is big endian!!");
            }

            FixedWidthIntegers();
            CompactIntegers();
            Booleans();
            Options();
            Results();
            BasicVectors();
            Strings();
            BasicEnums();
            Tuples();
            AdvancedEnums();
            Structs();
            AdvancedVectors();
        }
    }
}