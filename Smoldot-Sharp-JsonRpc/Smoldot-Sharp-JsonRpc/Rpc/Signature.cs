using System;
using sr25519_dotnet.lib;
using sr25519_dotnet.lib.Models;

namespace SmoldotSharp.JsonRpc
{
    public abstract class Key
    {
        public const int PublicKeySize = 32;
        public const int PrivateKeySize = 64;
        public const int SeedSize = 32;

        public abstract SR25519Keypair GetSR25519Keypair { get; }
    }

    public class KeySeed : Key
    {
        public const string Alice = "e5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a";

        readonly string seed;

        KeySeed(string seed)
        {
            this.seed = seed;
        }

        public override SR25519Keypair GetSR25519Keypair
            => SR25519.GenerateKeypairFromSeed(seed);

        public static (bool, KeySeed) New(string seed)
        {
            if (seed.Length != SeedSize * 2)
            {
                return (false, new KeySeed(string.Empty));
            }

            return (true, new KeySeed(seed));
        }
    }

    public class KeyPair : Key
    {
        readonly byte[] publicKey;
        readonly byte[] privateKey;

        KeyPair(byte[] publicKey, byte[] privateKey)
        {
            this.publicKey = publicKey;
            this.privateKey = privateKey;
        }

        public override SR25519Keypair GetSR25519Keypair
        {
            get
            {
                var tmp = new byte[PrivateKeySize];
                privateKey.CopyTo(tmp, 0);
                FromEd25519PrivateKey(tmp);
                return new SR25519Keypair(publicKey, tmp);
            }
        }

        public static (bool, KeyPair) New(byte[] publicKey, byte[] privateKey)
        {
            if (publicKey.Length == PublicKeySize 
                && privateKey.Length == PrivateKeySize)
            {
                return (true, new KeyPair(publicKey, privateKey));
            }

            return (false, new KeyPair(Array.Empty<byte>(), Array.Empty<byte>()));
        }

        // belowes are from schnorkel.
        // see https://github.com/w3f/schnorrkel

        void FromEd25519PrivateKey(Span<byte> secretKey)
        {
            var key = secretKey[..32];
            DivideScalarBytesByCofactor(key);
            MakeScalar(key);
        }

        void DivideScalarBytesByCofactor(Span<byte> scalar)
        {
            byte low = 0;
            for (int i = 31; i >= 0; i--)
            {
                byte r = (byte)(scalar[i] & 0b0000_0111);
                scalar[i] >>= 3;
                scalar[i] += low;
                low = (byte)(r << 5);
            }
        }

        void MakeScalar(Span<byte> bytes)
        {
            bytes[31] &= 0b0111_1111;
        }
    }

    public static class Signer
    {
        public static Signature Sign<TKey>(byte[] message, TKey key)
            where TKey : Key
        {
            var kp = key.GetSR25519Keypair;
            return new Signature(SR25519.Sign(message, kp));
        }
    }

    public class Signature
    {
        public readonly byte keyType = 0x01; // sr mode 
        readonly byte[] signature;

        public ReadOnlySpan<byte> GetSignature => signature;

        public Signature(byte[] signature)
        {
            this.signature = signature;
        }
    }
}
