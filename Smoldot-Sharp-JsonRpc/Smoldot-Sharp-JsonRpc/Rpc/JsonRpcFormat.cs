using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SmoldotSharp.JsonRpc
{
    public abstract class JsonRpcFormat 
    {
        // Empty
    }

    public class RequestFormat : JsonRpcFormat
    {
        public readonly uint id;
        public readonly string jsonrpc = "2.0";
        public readonly string method;
        [JsonProperty(PropertyName = "params")]
        public readonly JArray phantomParam = new JArray();

        public RequestFormat(string method, uint id)
        {
            this.id = id;
            this.method = method;
        }
    }

    public class RequestFormatWithParams<T> : JsonRpcFormat
    {
        public readonly uint id;
        public readonly string jsonrpc = "2.0";
        public readonly string method;
        [JsonProperty(PropertyName = "params")]
        public readonly T[]? paramsList;

        public RequestFormatWithParams(string method, uint id, params T[] param)
        {
            this.id = id;
            this.method = method;
            paramsList = param;
        }
    }

    public class ResponseFormat : JsonRpcFormat
    {
        public readonly uint id;
        public readonly string jsonrpc;
        public readonly JObject? error;
        public readonly string? method;

        public ResponseFormat(uint id, string jsonrpc, JObject? error, string? method)
        {
            this.id = id;
            this.jsonrpc = jsonrpc;
            this.error = error;
            this.method = method;
        }
    }

    public class ResultFormat<T> : JsonRpcFormat
    {
        public readonly T result;

        public ResultFormat(T result)
        {
            this.result = result;
        }
    }

    public class SubscriptionFormat : JsonRpcFormat
    {
        [JsonProperty(PropertyName = "params")]
        public readonly SubscriptionResult param;

        public SubscriptionFormat(SubscriptionResult param)
        {
            this.param = param;
        }
    }

    public class SubscriptionResult
    {
        public readonly string subscription;
        public readonly JObject result;

        public SubscriptionResult(string subscription, JObject result)
        {
            this.subscription = subscription;
            this.result = result;
        }
    }
}
