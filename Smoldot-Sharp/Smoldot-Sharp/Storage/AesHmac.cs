using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmoldotSharp
{
    using AesFunc = Aes;

    public class AesHmacKeys
    {
        readonly byte[] aesKey;
        readonly byte[] hmacKey;

        public ReadOnlyCollection<byte> AesKey => Array.AsReadOnly(aesKey);

        public ReadOnlyCollection<byte> HmacKey => Array.AsReadOnly(hmacKey);

        public AesHmacKeys(byte[] aesKey, byte[] hmacKey)
        {
            this.aesKey = aesKey;
            this.hmacKey = hmacKey;
        }

        public static AesHmacKeys TestAes32Hmac64Keys =>
            new AesHmacKeys(new byte[] 
            {
                22 , 13,162, 61, 97,  9,240,154,121, 21,155,219,187,182,231,166,
                113, 52, 57,192,123,213,127,223, 17, 31,217,224, 41, 82,148, 23
            }, 
            new byte[]
            {
                13 , 48,  2, 61,  0,167, 52,217,236, 76,237, 76,243,195,247,141,
                181, 65,197, 64,200, 24,156, 83,121,233,244,  2,223, 39,205,162,
                188,217,188,190,207, 24,220, 92,202, 63,254,169,109,180,154, 25,
                214,217,144,125, 12, 12,223,227, 77,175, 88, 52,234,219, 95,138
            });

        public static (byte[] aesKey, byte[] hmacKey) GenerateKeysSource(AesKeySize aesKeySize, HmacKeySize hmacKeySize)
        {
            using var aesRnd = RandomNumberGenerator.Create();
            using var hmacRnd = RandomNumberGenerator.Create();
            var aesKey = new byte[(int)aesKeySize];
            var hmacKey = new byte[(int)hmacKeySize];
            aesRnd.GetBytes(aesKey);
            hmacRnd.GetBytes(hmacKey);
            return (aesKey, hmacKey);
        }
    }

    public class AesHmac : AsyncObfuscator
    {
        readonly ISmoldotLogger logger;
        readonly AesHmacKeys keys;
        readonly HmacFunc hmacFunc;

        public AesHmac(ISmoldotLogger logger, AesHmacKeys keys, HmacFunc hmacFunc)
        {
            this.logger = logger;
            this.keys = keys;
            this.hmacFunc = hmacFunc;
        }

        HMAC NewHmacFunc
        {
            get
            {
                var keyLen = keys.HmacKey.Count;
                if (!(keyLen == 32 || keyLen == 64))
                {
                    throw new InitializationFailedException("HmacFunc");
                }

                return hmacFunc switch
                {
                    HmacFunc.HmacSha256 => new HMACSHA256(keys.HmacKey.ToArray()),
                    HmacFunc.HmacSha512 => new HMACSHA512(keys.HmacKey.ToArray()),
                    _ => throw new UnexpectedEnumValueException()
                };
            }

        }

        AesFunc NewAesFunc
        {
            get
            {
                var keyLen = keys.AesKey.Count;
                if (!(keyLen == 16 || keyLen == 24 || keyLen == 32))
                {
                    throw new InitializationFailedException("AesFunc");
                }

                return AesFunc.Create();
            }
        }

        public override Task<string> DeobfuscateAsync(ReadOnlyMemory<byte> raw)
        {
            return Task.Run(() => Deobfuscate(raw));
        }

        public override string Deobfuscate(ReadOnlyMemory<byte> raw)
        {
            using var func = NewAesFunc;
            var ivLen = func.IV.Length;
            var hashLen = (int)hmacFunc;
            var hash = raw[..hashLen].ToArray();

            using var hamc = NewHmacFunc;
            var magic = Encoding.UTF8.GetBytes(DatabaseConfig.MagicPhrase);
            var magicLen = magic.Length;
            var contentLen = raw.Length - hashLen;
            var totalBuff = new byte[contentLen + magicLen];
            raw[hashLen..].CopyTo(totalBuff);
            Buffer.BlockCopy(magic, 0, totalBuff, contentLen, magicLen);
            var toCheck = hamc.ComputeHash(totalBuff);
            if (!toCheck.SequenceEqual(hash))
            {
                logger.Log(SmoldotLogLevel.Warn, "Hash was not expected, returns empty content.");
                return "";
            }

            var iv = raw[hashLen..(hashLen + ivLen)].ToArray();
            var content = raw[(hashLen + ivLen)..].ToArray();
            var decryptor = func.CreateDecryptor(keys.AesKey.ToArray(), iv);
            var bytes = decryptor.TransformFinalBlock(content, 0, content.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        public override Task<Memory<byte>> ObfuscateAsync(string plain)
        {
            return Task.Run(() => Obfuscate(plain));
        }

        public override Memory<byte> Obfuscate(string plain)
        {
            using var aes = NewAesFunc;
            var iv = aes.IV;
            var ivLen = iv.Length;
            var encryptor = aes.CreateEncryptor(keys.AesKey.ToArray(), iv);
            var bytes = Encoding.UTF8.GetBytes(plain);
            var enc = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            var encLen = enc.Length;

            var hashLen = (int)hmacFunc;
            var mem = new Memory<byte>(new byte[hashLen + ivLen + encLen]);
            var hashMem = mem[..hashLen];
            var ivMem = mem[hashLen..(hashLen + ivLen)];
            var encMem = mem[(hashLen + ivLen)..];

            var magic = Encoding.UTF8.GetBytes(DatabaseConfig.MagicPhrase);
            var magicLen = magic.Length;
            var hmacSrc = new byte[ivLen + encLen + magicLen];
            Buffer.BlockCopy(iv, 0, hmacSrc, 0, ivLen);
            Buffer.BlockCopy(enc, 0, hmacSrc, ivLen, encLen);
            Buffer.BlockCopy(magic, 0, hmacSrc, ivLen + encLen, magicLen);
            using var hmac = NewHmacFunc;
            hmac.ComputeHash(hmacSrc).CopyTo(hashMem);
            iv.CopyTo(ivMem);
            enc.CopyTo(encMem);
            return mem;
        }
    }
}