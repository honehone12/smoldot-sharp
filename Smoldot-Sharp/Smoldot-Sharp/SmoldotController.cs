using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SmoldotSharp.Msgs;
using SmoldotSharp.JsonRpc;
using Newtonsoft.Json.Linq;

namespace SmoldotSharp
{
    using ControllerChannel = Channel<SmoldotMsg>;

    public class SmoldotController : RpcClientBasics
    {
        public event Action<ChainSpecData, DatabaseContent>? OnAddChainRequest;
        public event Action<int>? OnRemoveChainRequest;
        public event Action<int, string>? OnSendJsonRpcRequest;
        public event Action? OnStartShutdownRequest;

        readonly ChainspecProfile specProfile;
        readonly DatabaseContentStorage dbStorage;
        readonly ControllerChannel control;

        readonly Dictionary<uint, Action<BoxedObject>> resultCallbackTable
            = new Dictionary<uint, Action<BoxedObject>>();
        readonly Dictionary<int, (bool, uint)> followingTable = new Dictionary<int, (bool, uint)>();
        readonly ReadOnlyDictionary<int, (bool isRelayChain, string name)> chainTable;

        public SmoldotController(ISmoldotLogger logger, ControllerChannel control,
            ChainspecProfile specProfile, DatabaseContentStorage dbStorage,
            ReadOnlyDictionary<int, (bool, string)> chainTable)
            : base(logger)
        {
            this.chainTable = chainTable;
            this.control = control;
            this.specProfile = specProfile;
            this.dbStorage = dbStorage;
        }

        public bool ShouldUpdate => control.rx.Count > 0;

        public void Update()
        {
            while (control.rx.TryDequeue(out var msg))
            {
                switch (msg)
                {
                    case AddChainMsg m:
                        AddChain(m.name);
                        break;
                    case RemoveChainMsg m:
                        OnRemoveChainRequest?.Invoke(m.id);
                        break;
                    case SaveDatabaseMsg m:
                        SaveDatabase(m.id);
                        break;
                    case JsonRpcSendMsg m:
                        SendJsonRpc(m.chainId, m.rpc);
                        break;
                    case StartShutdownMsg m:
                        OnStartShutdownRequest?.Invoke();
                        break;
                    default:
                        throw new UnexpectedMessageException(msg);
                }
            }
        }

        public void OnInitialize()
        {
            control.tx.Enqueue(new OnInitializedMsg());
        }

        public void OnChainAdded(int id, string name)
        {
            // https://paritytech.github.io/json-rpc-interface-spec/api/chainHead_unstable_follow.html
            // we want start subscription of following blocks here as new api seem to
            // expect start subscription first.
            // but it will stop soon because smoldot is busy for warp syncing now.
            // for now calling old api is the handy way to ensure warp syncing is over.
            // need to find nicer way in the future.

            // request genesis hash
            var reqIdGen = NextRequestId;
            var rpcGen = new Rpc<string>(RpcMethodNames.GenesisHash);
            SendJsonRpc(id, reqIdGen, rpcGen);
            resultCallbackTable.Add(reqIdGen, (box) =>
            {
                var (ok, gen) = box.UnboxAsString();
                Debug.Assert(ok);
                Hash genesisHash;
                (ok, genesisHash) = Hash.New(gen);
                Debug.Assert(ok);
                genesisHashTable.Add(id, genesisHash);
            });

            // request runtime version
            var reqIdRv = NextRequestId;
            var rpcRv = new Rpc<RuntimeVersion>(RpcMethodNames.GetRuntimeVersion);
            SendJsonRpc(id, reqIdRv, rpcRv);
            resultCallbackTable.Add(reqIdRv, (box) =>
            {
                var (ok, rv) = box.UnboxAsClass<RuntimeVersion>();
                Debug.Assert(ok && rv != null);
                runtimeVersionTable.Add(id, rv);
            });

            // request metadata
            var reqId = NextRequestId;
            var rpc = new Rpc<string>(RpcMethodNames.GetMetadata);
            SendJsonRpc(id, reqId, rpc);
            resultCallbackTable.Add(reqId, (box) =>
            {
                var (ok, raw) = box.UnboxAsString();
                Debug.Assert(ok);
                ok = Metadata.DecodeMetadata(raw);
                Debug.Assert(ok);
                control.tx.Enqueue(new OnChainAddedMsg(id, name));

                // follow blocks
                // this should be first.
                var reqIdFol = NextRequestId;
                // need to put this in some config
                var getRuntimeUpdate = false;
                var rpcFol = new Rpc<string, bool>(RpcMethodNames.Follow, getRuntimeUpdate);
                SendJsonRpc(id, reqIdFol, rpcFol);
                resultCallbackTable.Add(reqIdFol, (box) =>
                {
                    var (ok, subscId) = box.UnboxAsString();
                    Debug.Assert(ok);
                    var ctxId = ContextId.NextId;
                    subscriptionTable.Add((id, subscId), ctxId);
                });
            });
        }

        public void OnPanic()
        {
            control.tx.Enqueue(new OnPanicMsg());
        }

        public void OnJsonRpcRespond(int chainId, string chainName, string json)
        {
            var (isSubsc, res) = IsSubscription(json);
            if (isSubsc)
            {
                var (isNotInternal, msg) = HandleSubscription(chainId, res, json);
                if (isNotInternal)
                {
                    control.tx.Enqueue(msg);
                }
            }
            else
            {
                var (ctx, box) = HandleResult(chainId, res, json);
                if (box == null)
                {
                    ContextId.End(ctx);
                    return;
                }
                if (resultCallbackTable.ContainsKey(res.id))
                {
                    var ok = resultCallbackTable.Remove(res.id, out var callback);
                    Debug.Assert(ok);
                    callback.Invoke(box);
                }
                else
                {
                    control.tx.Enqueue(new OnJsonRpcResultMsg(chainId, chainName, ctx, box));
                }
            }
        }

        void AddChain(string name)
        {
            var (ok, spec) = specProfile.ReadOneSource(name);
            Debug.Assert(ok);
            if (!ok)
            {
                logger.Log(SmoldotLogLevel.Error, $"Could not find spec source {name}. Skipped");
                return;
            }

            var dbContent = dbStorage.Read(name);
            OnAddChainRequest?.Invoke(spec, dbContent);
        }

        void SendJsonRpc(int chainId, RpcWithContext rpcCtx)
        {
            if (rpcCtx.rpc is SignedRpc signed)
            {
                SubmitExtrinsic(chainId, signed, rpcCtx.contextId);
                return;
            }

            SendJsonRpc(chainId, NextRequestId, rpcCtx.rpc, rpcCtx.contextId);
        }

        void SendJsonRpc(int chainId, uint reqId, Rpc rpc)
        {
            SendJsonRpc(chainId, reqId, rpc, 0u);
        }

        void SendJsonRpc(int chainId, uint reqId, Rpc rpc, uint ctxId)
        {
            var json = rpc.ToRequestJson(reqId);
            callbackTable.Add(reqId, (ctxId, rpc.Callback));
            OnSendJsonRpcRequest?.Invoke(chainId, json);
        }

        void SaveDatabase(int chainId)
        {
            var reqId = NextRequestId;
            var rpc = new Rpc<string>(RpcMethodNames.DatabaseContent);
            SendJsonRpc(chainId, reqId, rpc);
            resultCallbackTable.Add(reqId, (box) =>
            {
                var (ok, content) = box.UnboxAsString();
                Debug.Assert(ok && chainTable.ContainsKey(chainId));
                var (_, chainName) = chainTable[chainId];
                dbStorage.Write(new DatabaseContent(chainName, content));
                control.tx.Enqueue(new OnDatabaseSavedMsg(chainId, chainName));
            });
        }

        void SubmitExtrinsic(int chainId, SignedRpc signedRpc, uint ctxId)
        {
            // request nonce
            var reqIdNonce = NextRequestId;
            var rpcNonce = new Rpc<uint, string>(RpcMethodNames.AccountNextIndex,
                signedRpc.senderUri);
            SendJsonRpc(chainId, reqIdNonce, rpcNonce);
            resultCallbackTable.Add(reqIdNonce, (box) =>
            {
                var (ok, nonce) = box.UnboxAsUnmanaged<uint>();
                Debug.Assert(ok);

                // request finalized block hash
                var reqIdFinalized = NextRequestId;
                var rpcFinalized = new Rpc<string>(RpcMethodNames.GetFinalizedHead);
                SendJsonRpc(chainId, reqIdFinalized, rpcFinalized);
                resultCallbackTable.Add(reqIdFinalized, (box) =>
                {
                    var (ok, finanalizedHash) = box.UnboxAsString();
                    Debug.Assert(ok);

                    // request finalized header
                    var reqIdHeader = NextRequestId;
                    var rpcHeader = new Rpc<Header, string>(RpcMethodNames.GetHeader, finanalizedHash);
                    SendJsonRpc(chainId, reqIdHeader, rpcHeader);
                    resultCallbackTable.Add(reqIdHeader, (box) =>
                    {
                        var (ok, head) = box.UnboxAsClass<Header>();
                        Debug.Assert(ok);
                        ok = head.number.TryDeserialize(out var finalizedHeaderNum);
                        Debug.Assert(ok);

                        // sign
                        var hex = SignatureProcess(chainId, finanalizedHash, 
                            (ulong)finalizedHeaderNum, nonce, signedRpc);
                        if (string.IsNullOrEmpty(hex))
                        {
                            logger.Log(SmoldotLogLevel.Error, "Signature process failed. Skipped.");
                            return;
                            // or throw ??
                        }

                        // submit extrinsic
                        var reqIdExt = NextRequestId;
                        var rpcExt = new Rpc<string, string>(
                            RpcMethodNames.TransactionSubmitAndWatch, hex);
                        SendJsonRpc(chainId, reqIdExt, rpcExt);
                        resultCallbackTable.Add(reqIdExt, (box) =>
                        {
                            var (ok, subscId) = box.UnboxAsString();
                            Debug.Assert(ok);
                            subscriptionTable.Add((chainId, subscId), ctxId);
                            control.tx.Enqueue(new OnSubscribeMsg(ctxId, chainId, subscId));
                        });
                    });
                });
            });
        }

        (bool, SubscriptionMsg) HandleSubscription(int chainId, ResponseFormat res, string json)
        {
            Debug.Assert(res.method != null);
            var notification = json.Deserialize<SubscriptionFormat>();
            Debug.Assert(notification != null && notification.param != null);
            var subscId = notification.param.subscription;
            Debug.Assert(!string.IsNullOrEmpty(subscId));
            var id = (chainId, subscId);
            var ctx = subscriptionTable[id];
            var result = notification.param.result;
            Debug.Assert(result != null);

            switch (res.method)
            {
                case "author_extrinsicUpdate":
                    // this may be a unstable behavor
                    if (result["Broadcast"] == null)
                    {
                        throw new NotImplementedException();
                    }
                    return (true, new OnBloadcastedMsg(ctx, chainId, subscId));
                case "transaction_unstable_watchEvent":
                    return (true, HandleTransactionSubscription(chainId, result, ctx, subscId));
                case "chainHead_unstable_followEvent":
                    HandleFollowSubscription(chainId, result, ctx, subscId);
                    return (false, new InternalSubscriptionMsg());
                default:
                    throw new NotImplementedException();
            }
        }

        void HandleFollowSubscription(int chainId, JObject result,
            uint ctxId, string subscId)
        {
            switch (result["event"]?.ToString())
            {
                case "initialized":
                    break;
                case "newBlock":
                    break;
                case "bestBlockChanged":
                    break;
                case "finalized":
                    //smoldot stop emitting the events in a few minutes.
                    break;
                case "stop":
                    logger.Log(SmoldotLogLevel.Warn,
                        $"Subscription id {subscId} : chain {chainId} is stopped.");
                    Debug.Assert(subscriptionTable.ContainsKey((chainId, subscId)));
                    ContextId.End(ctxId);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        SubscriptionMsg HandleTransactionSubscription(int chainId, JObject result, 
            uint ctxId, string subscId)
        {
            switch (result["event"]?.ToString())
            {
                case "validated":
                    return new OnValidatedMsg(ctxId, chainId, subscId);
                case "broadcasted":
                    return new OnBloadcastedMsg(ctxId, chainId, subscId);
                case "bestChainBlockIncluded":
                    {
                        var hash = result["block"]?["hash"]?.ToString() ?? string.Empty;
                        return new OnInBlockMsg(ctxId, chainId, subscId, hash);
                    }
                case "finalized":
                    {
                        // no more events 
                        subscriptionTable.Remove((chainId, subscId));
                        var hash = result["block"]?["hash"]?.ToString() ?? string.Empty;
                        return new OnFinalizedMsg(ctxId, chainId, subscId, hash);
                    }
                case "error":
                    {
                        // no more events ??
                        subscriptionTable.Remove((chainId, subscId));
                        var err = result["error"]?.ToString() ?? string.Empty;
                        return new OnTransactionErrorMsg(ctxId, chainId, subscId, err);
                    }
                case "invalid":
                    {
                        // no more events ??
                        subscriptionTable.Remove((chainId, subscId));
                        var err = result["error"]?.ToString() ?? string.Empty;
                        return new OnInvlidMsg(ctxId, chainId, subscId, err);
                    }
                case "dropped":
                    {
                        // no more events 
                        subscriptionTable.Remove((chainId, subscId));
                        var err = result["error"]?.ToString() ?? string.Empty;
                        return new OnDroppedMsg(ctxId, chainId, subscId, err);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

    }
}