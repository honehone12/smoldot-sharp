using System;
using System.Collections.ObjectModel;
using System.Linq;
using sr25519_dotnet.lib;
using sr25519_dotnet.lib.Models;

namespace SmoldotSharp.JsonRpc
{
    using SignerImpl = Sr25519DotNetLibSigner;

    public abstract class Key
    {
        // Empty because of independence from SignerImpl.
    }

    public class KeySeed : Key
    {
        public const int SeedSize = 32;
        public const string Alice = "e5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a";

        readonly string seed;

        KeySeed(string seed)
        {
            this.seed = seed;
        }

        public static (bool, KeySeed) New(string seed)
        {
            if (seed.Length != SeedSize * 2)
            {
                return (false, new KeySeed(string.Empty));
            }

            return (true, new KeySeed(seed));
        }

        public ReadOnlySpan<char> GetSeed => seed.AsSpan();
    }

    public class KeyPair : Key
    {
        public const int PublicKeySize = 32;
        public const int PrivateKeySize = 64;

        readonly byte[] publicKey;
        readonly byte[] privateKey;

        KeyPair(byte[] publicKey, byte[] privateKey)
        {
            this.publicKey = publicKey;
            this.privateKey = privateKey;
        }

        public static (bool, KeyPair) New(byte[] publickey, byte[] privateKey)
        {
            if (publickey.Length == PublicKeySize 
                && privateKey.Length == PrivateKeySize)
            {
                return (true, new KeyPair(publickey, privateKey));
            }

            return (false, new KeyPair(Array.Empty<byte>(), Array.Empty<byte>()));
        }

        public ReadOnlyCollection<byte> GetPublicKey => Array.AsReadOnly(publicKey);

        public ReadOnlyCollection<byte> GetPrivateKey => Array.AsReadOnly(privateKey);
    }

    public abstract class Signer
    {
        public abstract Signature Sign<TKey>(byte[] message, TKey key) where TKey : Key;

        public static Signer New => new SignerImpl();
    }

    class Sr25519DotNetLibSigner : Signer
    {
        public override Signature Sign<TKey>(byte[] message, TKey key)
        {
            switch (key)
            {
                case KeySeed ks:
                    var srks = SR25519.GenerateKeypairFromSeed(ks.GetSeed.ToString());
                    return new Signature(SR25519.Sign(message, srks));
                case KeyPair kp:
                    var srkp = new SR25519Keypair(
                        kp.GetPublicKey.ToArray(), kp.GetPrivateKey.ToArray());
                    return new Signature(SR25519.Sign(message, srkp));
                default:
                    throw new UnexpectedDataException();
            }
        }
    }

    public class Signature
    {
        public readonly byte keyType = 0x01; // sr mode 
        public readonly byte[] signature;

        public Signature(byte[] signature)
        {
            this.signature = signature;
        }
    }
}
