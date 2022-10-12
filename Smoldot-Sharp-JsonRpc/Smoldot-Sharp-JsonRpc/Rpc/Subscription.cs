using Newtonsoft.Json.Linq;
using System;

namespace SmoldotSharp.JsonRpc
{
    public class SubscriptionHandle
    {
        public readonly string id;

        public SubscriptionHandle(string id)
        {
            this.id = id;
        }
    }

    public class ExtrinsicSubscriptionHandle : SubscriptionHandle
    {
        public Action? OnBloadcasted;
        public Action<string>? OnInBlock;
        public Action<string>? OnFinalized;
        public Action<string>? OnDropped;
        public Action<string>? OnInvalid;

        public ExtrinsicSubscriptionHandle(string id)
            : base(id) { /*Empty*/ }
    }

    public class ExtrinsicWatchingHandle : ExtrinsicSubscriptionHandle
    {
        public Action? OnReady;
        public Action<string>? OnRetracted;
        public Action<string>? OnFinalityTimeout;
        public Action<string>? OnUsurped;

        public ExtrinsicWatchingHandle(string id)
            : base(id) { /*Empty*/ }
    }

    public class TransactionWatchingHandle : ExtrinsicSubscriptionHandle
    {
        public Action? OnValidated;
        public Action<string>? OnError;

        public TransactionWatchingHandle(string id)
            : base(id) { /*Empty*/ }
    }
}