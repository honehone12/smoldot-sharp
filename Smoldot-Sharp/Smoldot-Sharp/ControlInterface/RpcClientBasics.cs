using System;
using System.Collections.Generic;
using System.Diagnostics;
using SmoldotSharp.JsonRpc;

namespace SmoldotSharp
{
    using RpcResponseCallback = Func<int, string, BoxedObject>;

    public class RpcClientBasics
    {
        protected readonly ISmoldotLogger logger;
        protected readonly HashSet<uint> requestSet = new HashSet<uint>();
        protected readonly Dictionary<uint, (uint ctx, RpcResponseCallback callback)> callbackTable
            = new Dictionary<uint, (uint, RpcResponseCallback)>();
        protected readonly Dictionary<(int, string), uint> subscriptionTable 
            = new Dictionary<(int, string), uint>();
        protected readonly Dictionary<int, Hash> genesisHashTable
            = new Dictionary<int, Hash>();
        protected readonly Dictionary<int, RuntimeVersion> runtimeVersionTable
            = new Dictionary<int, RuntimeVersion>();

        public RpcClientBasics(ISmoldotLogger logger)
        {
            this.logger = logger;
        }

        protected uint NextRequestId
        {
            get
            {
                var reqId = (uint)requestSet.Count;
                while (requestSet.Contains(reqId))
                {
                    ++reqId;
                }
                requestSet.Add(reqId);
                return reqId;
            }
        }

        protected (bool, ResponseFormat) IsSubscription(string json)
        {
            var res = json.Deserialize<ResponseFormat>();
            Debug.Assert(res != null);

            if (res.error != null)
            {
                logger.Log(SmoldotLogLevel.Error, "Error is returned. : " + json);
                Debug.Fail("Error is returned.");
                // need error handling.
            }

            if (res.method != null)
            {
                return (true, res);
            }

            return (false, res);
        }

        protected (uint, BoxedObject) HandleResult(int chainId, ResponseFormat res, string json)
        {
            Debug.Assert(requestSet.Contains(res.id));
            requestSet.Remove(res.id);

            var ok = callbackTable.Remove(res.id, out var cb);
            Debug.Assert(ok);
            return (cb.ctx, cb.callback.Invoke(chainId, json));
        }

        protected string SignatureProcess(int chainId, string finalizedBlockHash, 
            ulong finalizedHeaderNum, ulong nonce,　SignedRpc signedRpc)
        {
            Debug.Assert(genesisHashTable.ContainsKey(chainId)
                && runtimeVersionTable.ContainsKey(chainId));

            var ok = signedRpc.senderUri.AsSpan().
                TrySS58Decode(out var publicKey, out var chainCode);
            // should have chain code check here.
            Debug.Assert(ok);
            if (!ok)
            {
                logger.Log(SmoldotLogLevel.Error, "Extrinsic uri decoding error. Skipped.");
                return string.Empty;
            }

            MultiAddress addr;
            (ok, addr) = MultiAddress.New(publicKey.ToArray());
            Debug.Assert(ok);
            if (!ok)
            {
                logger.Log(SmoldotLogLevel.Error, "Extrinsic address error. Skipped.");
                return string.Empty;
            }

            Call call;
            (ok, call) = Call.New(signedRpc.moduleName, signedRpc.callName, signedRpc.arg);
            Debug.Assert(ok);
            if (!ok)
            {
                logger.Log(SmoldotLogLevel.Error, "Extrinsic call error. Skipped.");
                return string.Empty;
            }

            var era = Era.New(signedRpc.lifetimeOfRpc, finalizedHeaderNum);
            var rv = runtimeVersionTable[chainId];
            SignedExtensions signedEx;
            (ok, signedEx) = SignedExtensions.New(rv.specVersion, rv.transactionVersion,
                genesisHashTable[chainId].hash, era, nonce, signedRpc.tip, finalizedBlockHash);
            Debug.Assert(ok);
            if (!ok)
            {
                logger.Log(SmoldotLogLevel.Error, "Signed extensions error. Skipped.");
                return string.Empty;
            }

            var extrinsic = Extrinsic.SignLatestVersion(addr, signedRpc.key, call, signedEx);
            return extrinsic.encodedHex;
        }
    }
}