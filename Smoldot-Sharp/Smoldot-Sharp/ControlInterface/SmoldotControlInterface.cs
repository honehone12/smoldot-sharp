using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using SmoldotSharp.Msgs;

namespace SmoldotSharp
{
    public class SmoldotControlInterface
    {
        readonly Dictionary<string, int> chainTable = new Dictionary<string, int>();
        readonly Dictionary<(uint, int), Context> contextTable 
            = new Dictionary<(uint, int), Context>();
        readonly Channel<SmoldotMsg> ctrlCh;

        public event Action? OnInitialized;
        public event Action? OnPanic;
        public event Action<int, string>? OnChainAdded;
        public event Action<int, string>? OnDatabaseSaved;

        bool isBgUpdating;
        int bgDelay;

        public bool ShouldUpdate => ctrlCh.rx.Count > 0;

        public ReadOnlyDictionary<string, int> ChainTable
            => new ReadOnlyDictionary<string, int>(chainTable);

        public SmoldotControlInterface(Channel<SmoldotMsg> ctrlCh)
        {
            this.ctrlCh = ctrlCh;
        }

        public Task StartBackgroundUpdateAsync(int backgroundThreadDelay)
        {
            bgDelay = backgroundThreadDelay;
            isBgUpdating = true;
            return BackgroundUpdate();
        }

        async Task BackgroundUpdate()
        {
            while (isBgUpdating)
            {
                Update();

                await Task.Delay(bgDelay);
            }
        }

        public void StopBackgroundUpdate()
        {
            isBgUpdating = false;
        }

        public void Update()
        {
            while (ctrlCh.rx.TryDequeue(out var msg))
            {
                switch (msg)
                {
                    case OnInitializedMsg m:
                        OnInitialized?.Invoke();
                        break;
                    case OnChainAddedMsg m:
                        chainTable.Add(m.name, m.id);
                        OnChainAdded?.Invoke(m.id, m.name);
                        break;
                    case OnDatabaseSavedMsg m:
                        OnDatabaseSaved?.Invoke(m.id, m.name);
                        break;
                    case OnPanicMsg _:
                        OnPanic?.Invoke();
                        break;
                    case OnJsonRpcResultMsg m:
                        OnJsonRpcResult(m.ctxId, m.chainId, m.box);
                        break;
                    case OnSubscribeMsg m:
                        GetSubscription(m.ctxId, m.chainId)?.
                            StartSubscription(m.subscId);
                        break;
                    case OnValidatedMsg m:
                        GetSubscription(m.ctxId, m.chainId)?.
                            Handle?.OnValidated?.Invoke();
                        break;
                    case OnBloadcastedMsg m:
                        GetSubscription(m.ctxId, m.chainId)?.
                            Handle?.OnBloadcasted?.Invoke();
                        break;
                    case OnInBlockMsg m:
                        GetSubscription(m.ctxId, m.chainId)?.
                            Handle?.OnInBlock?.Invoke(m.hash);
                        break;
                    case OnFinalizedMsg m:
                        {
                            var sub = GetSubscription(m.ctxId, m.chainId);
                            if (sub != null)
                            {
                                sub.End();
                                contextTable.Remove((m.ctxId, m.chainId));
                                sub.Handle?.OnFinalized?.Invoke(m.hash);
                            }
                            break;
                        }
                    case OnTransactionErrorMsg m:
                        {
                            var sub = GetSubscription(m.ctxId, m.chainId);
                            if (sub != null)
                            {
                                sub.End();
                                contextTable.Remove((m.ctxId, m.chainId));
                                sub.Handle?.OnError?.Invoke(m.error);
                            }
                            break;
                        }
                    case OnDroppedMsg m:
                        {
                            var sub = GetSubscription(m.ctxId, m.chainId);
                            if (sub != null)
                            {
                                sub.End();
                                contextTable.Remove((m.ctxId, m.chainId));
                                sub.Handle?.OnDropped?.Invoke(m.error);
                            }
                            break;
                        }
                    case OnInvlidMsg m:
                        {
                            var sub = GetSubscription(m.ctxId, m.chainId);
                            if (sub != null)
                            {
                                sub.End();
                                contextTable.Remove((m.ctxId, m.chainId));
                                sub.Handle?.OnInvalid?.Invoke(m.error);
                            }
                            break;
                        }
                    default:
                        throw new UnexpectedMessageException(msg);
                }
            }
        }

        public void AddChain(string chainName)
        {
            ctrlCh.tx.Enqueue(new AddChainMsg(chainName));
        }

        public Task AddChainAsync(string chainName)
        {
            ctrlCh.tx.Enqueue(new AddChainMsg(chainName));
            bool done = false;
            OnChainAdded += (_, __) => done = true;
            return Task.Run(() =>
            {
                while (!done) ;
                OnChainAdded -= (_, __) => done = true;
            });
        }

        public void RemoveChain(string chainName)
        {
            if (!chainTable.ContainsKey(chainName))
            {
                throw new NoSuchChainException(chainName);
            }

            ctrlCh.tx.Enqueue(new RemoveChainMsg(chainTable[chainName]));
        }

        public void SaveDatabaseContent(string chainName)
        {
            if (!chainTable.ContainsKey(chainName))
            {
                throw new NoSuchChainException(chainName);
            }
            
            ctrlCh.tx.Enqueue(new SaveDatabaseMsg(chainTable[chainName]));
        }

        public RpcContext SendJsonRpc(string chainName, Rpc rpc)
        {
            if (!chainTable.ContainsKey(chainName))
            {
                throw new NoSuchChainException(chainName);
            }

            var chainId = chainTable[chainName];
            var ctxId = ContextId.NextId;
            ctrlCh.tx.Enqueue(new JsonRpcSendMsg(chainId, new RpcWithContext(rpc, ctxId)));
            var ctx = new RpcContext(ctxId, chainId);
            contextTable.Add((ctxId, chainId), ctx);
            return ctx;
        }

        public SubscriptionContext SendAndWatchRpc(string chainName, SignedRpc rpc)
        {
            if (!chainTable.ContainsKey(chainName))
            {
                throw new NoSuchChainException(chainName);
            }

            var chainId = chainTable[chainName];
            var ctxId = ContextId.NextId;
            ctrlCh.tx.Enqueue(new JsonRpcSendMsg(chainId, new RpcWithContext(rpc, ctxId)));
            var ctx = new SubscriptionContext(ctxId, chainId);
            contextTable.Add((ctxId, chainId), ctx);
            return ctx;
        }

        public void StartShutdown()
        {
            ctrlCh.tx.Enqueue(new StartShutdownMsg());
        }

        void OnJsonRpcResult(uint ctxId, int chainId, BoxedObject box)
        {
            var id = (ctxId, chainId);
            Debug.Assert(contextTable.ContainsKey(id)
                && contextTable[id] is RpcContext);
            if (contextTable[id] is RpcContext ctx)
            {
                ctx.SetResult(box);
                ctx.End();
                contextTable.Remove(id);
            }
        }

        SubscriptionContext? GetSubscription(uint ctxId, int chainId)
        {
            var id = (ctxId, chainId);
            Debug.Assert(contextTable.ContainsKey(id)
                && contextTable[id] is SubscriptionContext);
            if (contextTable[id] is SubscriptionContext ctx)
            {
                return ctx;
            }
            else
            {
                return null;
            }
        }
    }
}