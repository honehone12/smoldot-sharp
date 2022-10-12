using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScaleSharpLight;
using SmoldotSharp.JsonRpc;
using System.Diagnostics;
using System.Net.Security;
using SimpleTimer = System.Timers.Timer;

namespace SimpleRpcClient
{
    public partial class SimpleClient : IDisposable
    {
        const int RequestTimeOut = 1_000;

        readonly List<Connection> connList = new();
        readonly List<List<uint>> requestList = new();
        readonly Dictionary<(int, uint), Action<string>> callbackTable = new();
        readonly Dictionary<(int, string), ExtrinsicWatchingHandle> subscriptionTable = new();

        public int AddWebSocketConnection(string addr, 
            RemoteCertificateValidationCallback? validationCallback = null)
        {
            var nextIdx = connList.Count;
            var wsCon = new WebSocketConnection(nextIdx, addr, OnRpcResponse);
            if (validationCallback != null)
            {
                wsCon.SetCertificateValidation(validationCallback);
            }

            connList.Add(wsCon);
            requestList.Add(new List<uint>());
            wsCon.Connect();
            return connList.IndexOf(wsCon);
        }

        public async Task<(bool, T)> SendRpc<T>(string request, int connId, uint reqId)
        {
            return await Task.Run(() =>
            {
                T? result = default;
                var ok = false;
                var done = false;
                callbackTable.Add((connId, reqId), (json) =>
                {
                    var res = json.Deserialize<ResultFormat<T>>();
                    Debug.Assert(res != null);
                    if (res != null)
                    {
                        result = res.result;
                        ok = true;
                    }
                    done = true;
                });

                connList[connId].SendRpc(request);
                var timer = new SimpleTimer
                {
                    Interval = RequestTimeOut,
                    AutoReset = false
                };
                timer.Elapsed += (_, _) =>
                {
                    ok = false;
                    done = true;
                };
                timer.Start();

                while (!done) ;
                timer.Close();

                Debug.Assert(ok && result is not null);
                return (ok, result);
            });
        }

        public async Task RequestMetadata(int connId)
        {
            var reqId = NextRequestId(connId);
            var (ok, raw) = await SendRpc<string>(
                Json.GetJson(RpcMethodNames.GetMetadata, reqId), connId, reqId
            );
            Debug.Assert(ok && raw != null);
            ok = Metadata.DecodeMetadata(raw);
            Debug.Assert(ok);
        }

        public async Task<(bool, uint)> RequestAccountNextIndex(string uri, int connId)
        {
            var reqId = NextRequestId(connId);
            return await SendRpc<uint>(
                Json.GetJsonWithParams(RpcMethodNames.AccountNextIndex, reqId, uri), connId, reqId);
        }

        public async Task<(bool, Hash)> RequestBlockHash((Prefix, uint param) blockNumber, int connId)
        {
            var reqId = NextRequestId(connId);
            (var ok, var rawHash) = await SendRpc<string>(blockNumber.IsNone() 
                ? Json.GetJson(RpcMethodNames.GetBlockHash, reqId)
                : Json.GetJsonWithParams(RpcMethodNames.GetBlockHash, reqId, blockNumber.param),
                connId, reqId);
            if (!ok)
            {
                return (ok, new Hash());
            }

            return Hash.New(rawHash);
        }

        public async Task<(bool, Hash)> RequestFinalizedHead(int connId)
        {
            var reqId = NextRequestId(connId);
            (var ok, var rawHash) = await SendRpc<string>(
                Json.GetJson(RpcMethodNames.GetFinalizedHead, reqId), connId, reqId);
            if (!ok)
            {
                return (ok, new Hash());
            }

            return Hash.New(rawHash);
        }

        public async Task<(bool, Header)> RequestHeader((Prefix, Hash param) hash, int connId)
        {
            var reqId = NextRequestId(connId);
            return await SendRpc<Header>(hash.IsNone() 
                ? Json.GetJson(RpcMethodNames.GetHeader, reqId)
                : Json.GetJsonWithParams(RpcMethodNames.GetHeader, reqId, hash.param.hash),
                connId, reqId);
        }

        public async Task<(bool, RuntimeVersion)> RequestRuntimeVersion((Prefix, Hash param) blockHash, int connId)
        {
            var reqId = NextRequestId(connId);
            return await SendRpc<RuntimeVersion>(blockHash.IsNone() 
                ? Json.GetJson(RpcMethodNames.GetRuntimeVersion, reqId)
                : Json.GetJsonWithParams(RpcMethodNames.GetRuntimeVersion, reqId, blockHash.param.hash),
                connId, reqId);
        }

        public async Task<(bool, Hash)> SubmitExtrinsic(string hexEncoded, int connId)
        {
            var reqId = NextRequestId(connId);
            (var ok, var rawHash) = await SendRpc<string>(
                Json.GetJsonWithParams(RpcMethodNames.SubmitExtrinsic, reqId, hexEncoded),
                connId, reqId);
            if (!ok)
            {
                return (ok, new Hash());
            }

            return Hash.New(rawHash);
        }

        public async Task<(bool, ExtrinsicWatchingHandle)> SubmitAndWatchExtrinsic(
            string hexEncoded, int connId)
        {
            var reqId = NextRequestId(connId);
            (var ok, var subscriptionId) = await SendRpc<string>(
                Json.GetJsonWithParams(RpcMethodNames.SubmitAndWatchExtrinsic, reqId, hexEncoded),
                connId, reqId);

            if (!ok)
            {
                return (ok, new ExtrinsicWatchingHandle(string.Empty));
            }

            var handle = new ExtrinsicWatchingHandle(subscriptionId);
            subscriptionTable.Add((connId, subscriptionId), handle);
            return (ok, handle);
        }

        void OnRpcResponse(int connId, string json)
        {
            var res = json.Deserialize<ResponseFormat>();
            Debug.Assert(res != null);

            if (res.error != null)
            {
                Console.WriteLine("Error is returned. : " + json);
                Debug.Assert(false);
            }

            if (res.method != null)
            {
                HandleSubscription(connId, res.method, json);
                return;
            }

            var reqList = requestList[connId];
            Debug.Assert(reqList.Contains(res.id));
            reqList.Remove(res.id);
            Debug.Assert(callbackTable.ContainsKey((connId, res.id)));

            callbackTable[(connId, res.id)].Invoke(json);
            callbackTable.Remove((connId, res.id));
            //Console.WriteLine(res.result);
        }

        void HandleSubscription(int connId, string method, string json)
        {
            var notification = JsonConvert.DeserializeObject<JObject>(json);
            Debug.Assert(notification != null);
            var param = notification["params"];
            Debug.Assert(param != null);
            var subId = param["subscription"]?.ToString();
            Debug.Assert(!string.IsNullOrEmpty(subId));
            (int, string) id = (connId, subId);
            var result = param["result"];
            Debug.Assert(result != null);

            Debug.Assert(subscriptionTable.ContainsKey(id));
            
            // This simple example client does NOT cover all status.
            if (result is JObject o)
            {
                if (o.ContainsKey("inBlock"))
                {
                    var h = o["inBlock"]?.ToString();
                    Debug.Assert(h != null);
                    (var ok, var hash) = Hash.New(h);
                    Debug.Assert(ok);
                    subscriptionTable[id].OnInBlock?.Invoke(hash.hash);
                }
                else if (o.ContainsKey("finalized"))
                {
                    var h = o["finalized"]?.ToString();
                    Debug.Assert(h != null);
                    (var ok, var hash) = Hash.New(h);
                    Debug.Assert(ok);
                    subscriptionTable[id].OnFinalized?.Invoke(hash.hash);
                    subscriptionTable.Remove(id);
                }
            }
            else
            {
                var s = result.ToString();
                if (s.Equals("ready"))
                {
                    subscriptionTable[id].OnReady?.Invoke();
                }
            }
        }

        uint NextRequestId(int connId)
        {
            var reqList = requestList[connId];
            var reqId = (uint)reqList.Count;
            while (reqList.Contains(reqId))
            {
                ++reqId;
            }
            reqList.Add(reqId);
            return reqId;
        }

        public void Dispose()
        {
            connList.ForEach((conn) => conn.Dispose());
        }
    }
}
