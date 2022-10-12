using SmoldotSharp.JsonRpc;

namespace SmoldotSharp.Msgs
{
    // Base of all msgs.
    public abstract class Message { };

    // Base of msgs about Smoldot event.
    public abstract class SmoldotMsg : Message { }

    public class TimerFinishedMsg : SmoldotMsg
    {
        public readonly int id;

        public TimerFinishedMsg(int i) { id = i; }
    }

    public class AddChainMsg : SmoldotMsg
    {
        public readonly string name;

        public AddChainMsg(string n) { name = n; }
    }

    public class RemoveChainMsg : SmoldotMsg
    {
        public readonly int id;

        public RemoveChainMsg(int i) { id = i; }
    }

    public class SaveDatabaseMsg : SmoldotMsg
    {
        public readonly int id;

        public SaveDatabaseMsg(int i) { id = i; }
    }

    public class JsonRpcSendMsg : SmoldotMsg
    {
        public readonly uint ctxId;
        public readonly int chainId;
        public readonly RpcWithContext rpc;

        public JsonRpcSendMsg(int i, RpcWithContext r) { chainId = i; rpc = r; }
    }

    public class OnJsonRpcResultMsg : SmoldotMsg
    {
        public readonly int chainId;
        public readonly string name;
        public readonly uint ctxId;
        public readonly BoxedObject box;

        public OnJsonRpcResultMsg(int i, string n, uint c, BoxedObject b)
        { chainId = i; name = n; ctxId = c; box = b; }
    }

    public class OnChainAddedMsg : SmoldotMsg
    {
        public readonly int id;
        public readonly string name;

        public OnChainAddedMsg(int i, string n) { id = i; name = n; }
    }

    public class OnPanicMsg : SmoldotMsg { };

    public class OnDatabaseSavedMsg : SmoldotMsg
    {
        public readonly int id;
        public readonly string name;

        public OnDatabaseSavedMsg(int i, string n) { id = i; name = n; }
    }

    public class StartShutdownMsg : SmoldotMsg { }

    // Base of msgs for subscription
    public abstract class SubscriptionMsg : SmoldotMsg 
    {
        public readonly uint ctxId;
        public readonly int chainId;
        public readonly string subscId;

        public SubscriptionMsg(uint c, int i, string s) { ctxId = c; chainId = i; subscId = s; }
    }

    public class OnInitializedMsg : SmoldotMsg { }

    public class OnSubscribeMsg : SubscriptionMsg
    {
        public OnSubscribeMsg(uint c, int i, string s) : base(c, i, s) { }
    }

    public class OnValidatedMsg : SubscriptionMsg
    {
        public OnValidatedMsg(uint c, int i, string s) : base(c, i, s) { }
    }

    public class OnBloadcastedMsg : SubscriptionMsg
    {
        public OnBloadcastedMsg(uint c, int i, string s) : base(c, i, s) { }
    }

    public class OnInBlockMsg : SubscriptionMsg
    {
        public readonly string hash;

        public OnInBlockMsg(uint c, int i, string s, string h) : base(c, i, s) { hash = h; }
    }

    public class OnFinalizedMsg : SubscriptionMsg
    {
        public readonly string hash;

        public OnFinalizedMsg(uint c, int i, string s, string h) : base(c, i, s) { hash = h; }
    }

    public class OnTransactionErrorMsg : SubscriptionMsg
    {
        public readonly string error;

        public OnTransactionErrorMsg(uint c, int i, string s, string e) : base(c, i, s) { error = e; }
    }

    public class OnDroppedMsg : SubscriptionMsg
    {
        public readonly string error;

        public OnDroppedMsg(uint c, int i, string s, string e) : base(c, i, s) { error = e; }
    }

    public class OnInvlidMsg : SubscriptionMsg
    {
        public readonly string error;

        public OnInvlidMsg(uint c, int i, string s, string e) : base(c, i, s) { error = e; }
    }

    // Base of msgs for transport.
    public abstract class TransportMsg : Message { };

    public class OnOpenMsg : TransportMsg
    {
        public readonly int id;

        public OnOpenMsg(int i) { id = i; }
    }

    public class OnReceivedMsg : TransportMsg
    {
        public readonly int connId;
        public readonly int streamId;
        public readonly byte[] data;

        public OnReceivedMsg(int conn, int stream, byte[] d)
        {
            connId = conn; streamId = stream; data = d;
        }
    }

    public class OnClosedMsg : TransportMsg
    {
        public readonly int id;
        public readonly string why;

        public OnClosedMsg(int i, string w) { id = i; why = w; }
    }

    public class OnErrorMsg : TransportMsg
    {
        public readonly int id;
        public readonly string what;

        public OnErrorMsg(int i, string w) { id = i; what = w; }
    }
}