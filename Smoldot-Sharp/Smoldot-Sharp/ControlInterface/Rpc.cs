using System;
using System.Diagnostics;
using SmoldotSharp.JsonRpc;

namespace SmoldotSharp
{
    public class RpcWithContext
    {
        public readonly Rpc rpc;
        public readonly uint contextId;

        public RpcWithContext(Rpc rpc, uint contextId)
        {
            this.rpc = rpc;
            this.contextId = contextId;
        }
    }

    public abstract class Rpc
    {
        public abstract string ToRequestJson(uint reqId);

        public abstract Func<int, string, BoxedObject?> Callback { get; }
    }

    public class Rpc<TResult> : Rpc
    {
        public readonly string methodName;

        public override Func<int, string, BoxedObject?> Callback => (i, json) =>
        {
            var res = json.Deserialize<ResultFormat<TResult>>();
            Debug.Assert(res != null);
            var result = res.result;
            if (result == null)
            {
                return null;
            }

            return new BoxedObject(result);
        };

        public Rpc(string methodName)
        {
            this.methodName = methodName;
        }

        public override string ToRequestJson(uint reqId)
        {
            return Json.GetJson(methodName, reqId);
        }
    }

    public class Rpc<TResult, TParam> : Rpc<TResult>
    {
        public readonly TParam[] param;

        public Rpc(string methodName, params TParam[] param)
            : base(methodName)
        {
            this.param = param;
        }

        public override string ToRequestJson(uint reqId)
        {
            return Json.GetJsonWithParams(methodName, reqId, param);
        }
    }

    public class SignedRpc : Rpc
    {
        public readonly string senderUri;
        public readonly Key key;
        public readonly string moduleName;
        public readonly string callName;
        public readonly uint lifetimeOfRpc;
        public readonly Tip tip;
        public readonly byte[]? arg;

        public SignedRpc(string senderUri, Key key, string moduleName, string callName,
            byte[]? arg = null, ulong tip = 0ul, uint lifetimeOfRpc = 64u)
        {
            this.senderUri = senderUri;
            this.key = key;
            this.moduleName = moduleName;
            this.callName = callName;
            this.arg = arg;
            this.lifetimeOfRpc = lifetimeOfRpc;
            this.tip = new Tip(tip);
        }

        public SignedRpc(string senderUri, Key key, string moduleName, string callName,
            byte[]? arg, Tip tip, uint lifetimeOfRpc = 64u)
        {
            this.senderUri = senderUri;
            this.key = key;
            this.moduleName = moduleName;
            this.callName = callName;
            this.arg = arg;
            this.lifetimeOfRpc = lifetimeOfRpc;
            this.tip = tip;
        }

        public override Func<int, string, BoxedObject> Callback 
            => throw new UnExpectedDataUsageException(
                "This class has to be consumed in signature process to generate some new Rpc classes.");

        public override string ToRequestJson(uint reqId)
        {
            throw new UnExpectedDataUsageException(
                "This class has to be consumed in signature process to generate some new Rpc classes.");
        }
    }
}