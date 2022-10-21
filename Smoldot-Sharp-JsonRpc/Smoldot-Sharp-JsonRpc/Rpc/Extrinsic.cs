using ScaleSharpLight;
using System;
using System.Numerics;
using Blake2Core;
using System.Diagnostics;

namespace SmoldotSharp.JsonRpc
{
    using LatestExtrinsic = ExtrinsicV4;

    public class Call
    {
        public readonly byte version;
        public readonly CallIndex callIndex;
        public readonly byte[] args = Array.Empty<byte>();

        public Call(byte version, CallIndex callIndex, byte[]? args = null)
        {
            this.version = version;
            this.callIndex = callIndex;
            if (args != null)
            {
                this.args = args;
            }
        }

        public static (bool, Call) New(string moduleName, string callName,
            byte[]? args = null)
        {
            var query = Metadata.GetQuery;
            (var ok, var callIdx) = query.FindCallIndex(moduleName, callName);
            if (ok)
            {
                return (true, new Call(query.ExtrinsicVersion, callIdx, args));
            }

            return (false, new Call(default, default));
        }

        public int EncodedSize()
        {
            return args.Length + 2;
        }

        public int Encode(Span<byte> buff)
        {
            buff[0] = callIndex.moduleIndex;
            buff[1] = callIndex.callIndex;
            args.CopyTo(buff[2..]);
            return args.Length + 2;
        }
    }

    public class SignedExtensions
    {
        public readonly uint specVersion;
        public readonly uint transactionVersion;
        public readonly byte[] genesisHash;
        public readonly byte[] blockHash;
        public readonly Era era;
        public readonly BigInteger nonce;
        public readonly Tip tip;

        SignedExtensions(
            uint specVersion, uint transactionVersion, byte[] genesisHash, 
            Era era, ulong nonce, Tip tip, byte[]? blockHash = null)
        {
            this.specVersion = specVersion;
            this.transactionVersion = transactionVersion;
            this.genesisHash = genesisHash;
            this.blockHash = blockHash ?? genesisHash;
            this.era = era;
            this.nonce = nonce;
            this.tip = tip;
        }

        SignedExtensions()
        {
            specVersion = default;
            transactionVersion = default;
            genesisHash = Array.Empty<byte>();
            blockHash = Array.Empty<byte>();
            era = default;
            nonce = default;
            tip = default;
        }

        public static (bool, SignedExtensions) New(uint specVersion, uint transactionVersion, 
            string genesisHash, Era era, ulong nonce, Tip tip, string? blockHash = null)
        {
            Span<byte> buff = stackalloc byte[Hash.Size];
            if (!genesisHash.AsSpan().Remove0X().TryHexToBytes(buff, out var r) || r != Hash.Size)
            {
                return (false, new SignedExtensions());
            }

            var genHashBytes = buff.ToArray();
            if (blockHash != null && 
                (!blockHash.AsSpan().Remove0X().TryHexToBytes(buff, out r) || r != Hash.Size))
            {
                return (false, new SignedExtensions());
            }

            var blockHashBytes = buff.ToArray();
            return (true, new SignedExtensions(specVersion, transactionVersion,
                genHashBytes, era, nonce, tip, blockHashBytes));
        }

        public int EncodedSize()
        {
            var tipSize = tip.tipKind switch
            {
                TipKind.PlainTip => tip.tip.CompactEncodedSize(),
                TipKind.AssetTip => tip.tip.CompactEncodedSize() + tip.assetId.FixedEncodedSize(),
                _ => throw new UnexpectedDataException()
            };
            var eraSize = era.eraKind switch
            {
                EraKind.Immortal => 1,
                EraKind.Mortal => 2,
                _ => throw new UnexpectedDataException()
            };

            return 4 + 4 + Hash.Size * 2 + eraSize + nonce.CompactEncodedSize() + tipSize;
        }

        public int Encode(Span<byte> buff, out byte[] miniExtensions)
        {
            var pos = 0;

            switch (era.eraKind)
            {
                case EraKind.Immortal:
                    buff[pos++] = (byte)era.eraKind;
                    break;
                case EraKind.Mortal:
                    pos += era.encoded.FixedEncode(buff[pos..]);
                    break;
                default:
                    throw new UnexpectedDataException();
            }
            pos += nonce.CompactEncode(buff[pos..]);

            switch (tip.tipKind)
            {
                case TipKind.PlainTip:
                    pos += tip.tip.CompactEncode(buff[pos..]);
                    break;
                case TipKind.AssetTip:
                    pos += tip.tip.CompactEncode(buff[pos..]);
                    pos += tip.assetId.FixedEncode(buff[pos..]);
                    break;
                default:
                    throw new UnexpectedDataException();
            }

            miniExtensions = new byte[pos];
            buff[..pos].CopyTo(miniExtensions);

            pos += specVersion.FixedEncode(buff[pos..]);
            pos += transactionVersion.FixedEncode(buff[pos..]);
            genesisHash.CopyTo(buff[pos..]);
            pos += Hash.Size;
            blockHash.CopyTo(buff[pos..]);
            pos += Hash.Size;
            return pos;
        }
    }

    public static class Extrinsic
    {
        public static LatestExtrinsic SignLatestVersion(MultiAddress multiAddress, Key key,
            Call call, SignedExtensions signedExtensions)
        {
            return LatestExtrinsic.Sign(multiAddress, call, signedExtensions, key);
        }
    }

    public class ExtrinsicV4
    {
        public readonly string encodedHex;

        ExtrinsicV4(byte version, MultiAddress accountId, Signature signature, 
            byte[] miniExtensions, byte[] call)
        {
            const int MaxStackAllocLen = 256;

            var sig = signature.GetSignature;
            var sigLen = sig.Length;
            var exLen = miniExtensions.Length;
            var callLen = call.Length;
            var totalLen = 1 + 1 + MultiAddress.Size + 1 + sigLen + exLen + callLen;
            var sizePfx = Compact.CompactInteger((uint)totalLen);
            totalLen += sizePfx.CompactEncodedSize();
            var buff = totalLen >= MaxStackAllocLen ? 
                new Span<byte>(new byte[totalLen]) : stackalloc byte[totalLen];
            var pos = 0;

            pos += sizePfx.CompactEncode(buff);
            buff[pos++] = (byte)(version | 0b1000_0000); // signed only
            buff[pos++] = accountId.multiAddrPrefix; 
            accountId.GetAccountId.CopyTo(buff[pos..]);
            pos += MultiAddress.Size;
            buff[pos++] = signature.keyType; 
            sig.CopyTo(buff[pos..]);
            pos += sigLen;
            miniExtensions.CopyTo(buff[pos..]);
            pos += exLen;
            call.CopyTo(buff[pos..]);
            pos += callLen;
            Debug.Assert(pos == totalLen);

            var totalCharLen = 2 + totalLen * 2;
            var charBuff = totalCharLen >= MaxStackAllocLen ?
                new Span<char>(new char[totalCharLen]) : stackalloc char[totalCharLen];
            buff.BytesToHex(charBuff.Put0X(), out var w);
            Debug.Assert(w == totalCharLen - 2);
            encodedHex = charBuff.ToString();
        }

        public static ExtrinsicV4 Sign(MultiAddress multiAddress, Call call, 
            SignedExtensions signedExtensions, Key key)
        {
            Debug.Assert(call.version == 4);
            var payloadSize = call.EncodedSize() + signedExtensions.EncodedSize();
            var payload = new byte[payloadSize];
            var buff = new Span<byte>(payload);
            var pos = 0;
            pos += call.Encode(buff);
            var callAsBytes = new byte[pos];
            buff[..pos].CopyTo(callAsBytes);

            pos += signedExtensions.Encode(buff[pos..], out var miniEx);
            Debug.Assert(pos == payloadSize);

            //https://github.com/paritytech/subxt/blob/06287fc1192ab7169a45839b7445a1560f644736/subxt/src/tx/tx_client.rs
            if (payloadSize > 256)
            {
                var config = new Blake2BConfig { OutputSizeInBits = 256 };
                payload = Blake2B.ComputeHash(payload, config);
            }
;
            var sig = Signer.Sign(payload, key);
            return new ExtrinsicV4(call.version, multiAddress, sig, miniEx, callAsBytes);
        }
    }
}