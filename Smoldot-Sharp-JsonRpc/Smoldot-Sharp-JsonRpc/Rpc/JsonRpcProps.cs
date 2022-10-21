using Newtonsoft.Json.Linq;
using ScaleSharpLight;
using System;
using System.Collections.ObjectModel;
using System.Numerics;

namespace SmoldotSharp.JsonRpc
{
    public class Hash
    {
        public const int Size = 32;

        public readonly string hash;

        public Hash()
        {
            hash = string.Empty;
        }

        Hash(string hash)
        {
            this.hash = hash;
        }

        public static (bool, Hash) New(string hash)
        {
            if (hash.Has0XPrefix(true) && hash.Length == Size * 2 + 2)
            {
                return (true, new Hash(hash));
            }

            return (false, new Hash(string.Empty));
        }
    }

    public class Header
    {
        public readonly JObject digest;
        public readonly string extrinsicRoot;
        public readonly string number;
        public readonly string parentHash;
        public readonly string stateRoot;

        public Header(JObject digest, string extrinsicRoot,
            string number, string parentHash, string stateRoot)
        {
            this.digest = digest;
            this.extrinsicRoot = extrinsicRoot;
            this.number = number;
            this.parentHash = parentHash;
            this.stateRoot = stateRoot;
        }
    }

    public readonly struct CallIndex
    {
        public readonly byte moduleIndex;
        public readonly byte callIndex;

        public CallIndex(byte moduleIndex, byte callIndex)
        {
            this.moduleIndex = moduleIndex;
            this.callIndex = callIndex;
        }
    }

    public class MultiAddress
    {
        public const int Size = 32;
        public const int SS58UriSize = 48;

        public readonly byte multiAddrPrefix = 0x00;
        readonly byte[] accountId;

        MultiAddress(byte[] accountId)
        {
            this.accountId = accountId;
        }

        public ReadOnlySpan<byte> GetAccountId => accountId;

        public static (bool, MultiAddress) New(byte[] accountId)
        {
            return accountId.Length == Size ? (true, new MultiAddress(accountId))
                : (false, new MultiAddress(Array.Empty<byte>()));
        }
    }

    public enum EraKind : byte
    {
        Immortal,
        Mortal
    }

    public readonly struct Era
    {
        public readonly EraKind eraKind;
        public readonly ushort encoded;

        public Era(EraKind eraKind, ushort encoded = default)
        {
            this.eraKind = eraKind;
            this.encoded = encoded;
        }

        public static Era New(ulong currentFinalized)
        {
            // same default with polkadot js app.
            return New(128u, currentFinalized);
        }

        public static Era New(uint lifetimePeriod, ulong currentFinalized)
        {
            // https://github.com/paritytech/substrate/blob/master/primitives/runtime/src/generic/era.rs

            lifetimePeriod = lifetimePeriod < 4 ? 4 : lifetimePeriod;
            lifetimePeriod = lifetimePeriod > 65536 ? 65536 : lifetimePeriod;

            if ((lifetimePeriod & (lifetimePeriod - 1)) != 0)
            {
                // target is ceil (next) powerof2 of input here.
                var input = lifetimePeriod;
                lifetimePeriod = 2;
                while ((input >>= 1) > 0)
                {
                    lifetimePeriod <<= 1;
                }
            }

            var phase = currentFinalized % lifetimePeriod;
            var quantizedFactor = lifetimePeriod >> 12 > 1 ? lifetimePeriod >> 12 : 1;
            phase = phase / quantizedFactor * quantizedFactor;

            var upper = 0u;
            // count traling zeros.
            // Too simple ??
            while ((lifetimePeriod & 0b1) == 0)
            {
                ++upper;
                lifetimePeriod >>= 1;
            }
            --upper;
            upper = upper < 1 ? 1 : upper;
            upper = upper > 15 ? 15 : upper;
            var lower = (uint)(phase / quantizedFactor) << 4;
            var enc = (ushort)(upper | lower);
            return new Era(EraKind.Mortal, enc);
        }
    }

    public enum TipKind
    {
        PlainTip,
        AssetTip
    }

    public readonly struct Tip
    {
        public readonly TipKind tipKind;
        public readonly BigInteger tip;
        public readonly (Prefix, uint) assetId;

        public Tip(ulong tip, (Prefix, uint) assetId)
        {
            tipKind = TipKind.AssetTip;
            this.tip = tip;
            this.assetId = assetId;
        }

        public Tip(ulong tip)
        {
            this.tipKind = TipKind.PlainTip;
            this.tip = tip;
            assetId = default;
        }
    }

    public class RuntimeVersion
    {
        public readonly JArray apis;
        public readonly uint authoringVersion;
        public readonly string implName;
        public readonly uint implVersion;
        public readonly string specName;
        public readonly uint specVersion;
        public readonly uint transactionVersion;
        public readonly uint stateVersion;

        public RuntimeVersion(
            JArray apis, uint authoringVersion, string implName, uint implVersion,
            string specName, uint specVersion, uint transactionVersion, uint stateVersion)
        {
            this.apis = apis;
            this.authoringVersion = authoringVersion;
            this.implName = implName;
            this.implVersion = implVersion;
            this.specName = specName;
            this.specVersion = specVersion;
            this.transactionVersion = transactionVersion;
            this.stateVersion = stateVersion;
        }
    }
}