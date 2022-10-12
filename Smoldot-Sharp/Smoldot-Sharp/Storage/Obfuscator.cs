using System;
using System.Threading.Tasks;

namespace SmoldotSharp
{
    public abstract class Obfuscator
    {
        public abstract Memory<byte> Obfuscate(string plain);

        public abstract string Deobfuscate(ReadOnlyMemory<byte> raw);
    }

    public abstract class AsyncObfuscator : Obfuscator
    {
        public abstract Task<Memory<byte>> ObfuscateAsync(string plain);

        public abstract Task<string> DeobfuscateAsync(ReadOnlyMemory<byte> raw);
    }

    public enum AesKeySize : int
    {
        Size16 = 16,
        Size24 = 24,
        Size32 = 32
    }

    public enum HmacFunc : int 
    {
        HmacSha256 = 32,
        HmacSha512 = 64
    }

    public enum HmacKeySize : int
    {
        Size32 = 32,
        Size64 = 64
    }
}