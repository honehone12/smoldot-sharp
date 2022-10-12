using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmoldotSharp.JsonRpc;

namespace SmoldotSharp
{
    public static class ContextId
    {
        private readonly static HashSet<uint> ContextIdSet = new HashSet<uint>();

        public static uint NextId
        {
            get
            {
                var ctxId = (uint)ContextIdSet.Count;
                while (ContextIdSet.Contains(ctxId))
                {
                    ++ctxId;
                }
                ContextIdSet.Add(ctxId);
                return ctxId;
            }
        }

        public static bool IsAlive(uint ctxId) => ContextIdSet.Contains(ctxId);

        public static bool End(uint ctxId)
        {
            return ContextIdSet.Remove(ctxId);
        }
    }

    public enum ContextTypes : byte
    {
        Rpc,
        Subscription
    }

    public abstract class Context
    {
        public readonly uint contextId;
        public readonly int chainId;

        public bool IsAlive => ContextId.IsAlive(contextId);

        public Context(uint contextId, int chainId)
        {
            this.contextId = contextId;
            this.chainId = chainId;
        }

        public void End()
        {
            ContextId.End(contextId);
        }
    }

    public class RpcContext : Context
    {
        public bool IsDone { get; private set; }
        public BoxedObject? Result { get; private set; }

        public RpcContext(uint contextId, int chainId)
            : base(contextId, chainId)
        { /*Empty*/ }

        public void SetResult(BoxedObject result)
        {
            Result = result;
            IsDone = true;
        }

        public Task<BoxedObject?> GetResultAsync()
        {
            return Task.Run(() =>
            {
                while (!IsDone && IsAlive) ;
                return Result;
            });
        }
    }

    public class SubscriptionContext : Context
    {
        public bool IsReady { get; private set; }

        public TransactionWatchingHandle? Handle { get; private set; }

        public SubscriptionContext(uint contextId, int chainId) 
            : base(contextId, chainId)
        { /*Empty*/ }

        public void StartSubscription(string subscId)
        {
            IsReady = true;
            Handle = new TransactionWatchingHandle(subscId);
        }
    }
}