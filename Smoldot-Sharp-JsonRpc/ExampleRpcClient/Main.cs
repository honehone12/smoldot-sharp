using ScaleSharpLight;
using SmoldotSharp.JsonRpc;
using System.Diagnostics;

namespace SimpleRpcClient
{
    internal class ClientMain
    {
        const string LocalAddress = "ws://127.0.0.1:9944";
        const string AliceUri = "5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY";
        const string BobUri =   "5FHneW46xGXgs5mUiveU4sbTyGBzmstUspZC92UhjJM694ty";


        static async Task Main()
        {
            using var client = new SimpleClient();
            var connId = client.AddWebSocketConnection(LocalAddress);
            await client.RequestMetadata(connId);
            //PrintMetadata();

            (var ok, var genesisHash) = await client.RequestBlockHash(Option.U32(0), connId);
            Debug.Assert(ok);
            (ok, var runtimeVer) =  await client.RequestRuntimeVersion(Option.NoneAndNew<Hash>(), connId);
            Debug.Assert(ok);
            (ok, var nonce) = await client.RequestAccountNextIndex(AliceUri, connId);
            Debug.Assert(ok);
            (ok, var finalizedHead) = await client.RequestFinalizedHead(connId);
            Debug.Assert(ok);
            (ok, var header) = await client.RequestHeader(finalizedHead.ToOption(), connId);
            Debug.Assert(ok);
            ok = header.number.TryDeserialize(out var headNum);
            Debug.Assert(ok);

            var extrinsic = MakeExtrinsic(genesisHash, finalizedHead, runtimeVer, nonce, (ulong)headNum);
            (ok, var handle) = await client.SubmitAndWatchExtrinsic(extrinsic.encodedHex, connId);
            Debug.Assert(ok);
            var done = false;
            handle.OnReady += () => Console.WriteLine($"{handle.id} Ready");
            handle.OnInBlock += (h) => Console.WriteLine($"{handle.id} InBlock {h}");
            handle.OnFinalized += (h) => {
                Console.WriteLine($"{handle.id} Finalized {h}");
                done = true;
            };

            while (!done)
            {
                Thread.Sleep(10);
            }
        }

        static byte[] MakeCallRequest()
        {
            var value = Compact.CompactInteger(100000000000000ul);
            var ok = BobUri.AsSpan().TrySS58Decode(out var destPub, out _);
            Debug.Assert(ok);
            (ok, var dest) = MultiAddress.New(destPub.ToArray());
            Debug.Assert(ok);
            var data = new byte[1 + MultiAddress.Size + value.CompactEncodedSize()];
            var dataBuff = new Span<byte>(data);
            var pos = 0;
            dataBuff[pos++] = dest.multiAddrPrefix;
            dest.AccountIdAsSpan.CopyTo(dataBuff[pos..]);
            pos += MultiAddress.Size;
            pos += value.CompactEncode(dataBuff[pos..]);
            Debug.Assert(pos == data.Length);
            return data;
        }

        static ExtrinsicV4 MakeExtrinsic(Hash genesisHash, Hash blockHash, RuntimeVersion rtVer, 
            uint nonce, ulong finalized)
        {
            var ok = AliceUri.AsSpan().TrySS58Decode(out var alicePubKey, out var code);
            Debug.Assert(code == 42);
            Debug.Assert(ok);

            (ok, var alice) = MultiAddress.New(alicePubKey.ToArray());
            Debug.Assert(ok);
            (ok, var call) = Call.New("Balances", "transfer", MakeCallRequest());
            Debug.Assert(ok);
            (ok, var signedEx) = SignedExtensions.New(rtVer.specVersion, rtVer.transactionVersion,
                genesisHash.hash, Era.New(finalized), nonce, new Tip(0), blockHash.hash);
            Debug.Assert(ok);

            (ok, var aliceKey) = KeySeed.New(KeySeed.Alice);
            Debug.Assert(ok);

            return Extrinsic.SignLatestVersion(alice, aliceKey, call, signedEx);
        }

        static void PrintMetadata()
        {
            var print = Metadata.String();
            if (print != null)
            {
                foreach (var item in print)
                {
                    Console.WriteLine(item);
                    Console.WriteLine();
                }
            }
        }
    }
}