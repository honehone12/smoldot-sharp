using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Diagnostics;
using WebSocketSharp;
using SmoldotSharp.Msgs;
using System.ComponentModel;

namespace SmoldotSharp
{
    using CallbackChannel = OneWayChannel<TransportMsg>;

    public class WebSocketTransport : SmoldotTransport, IDisposable
    {
        readonly ISmoldotLogger logger;
        readonly CallbackChannel callbackQ;
        readonly (CallbackChannel.Tx tx, CallbackChannel.Rx rx) callbackCh;
        readonly Dictionary<int, WebSocket> webSocketTable = new Dictionary<int, WebSocket>();
        RemoteCertificateValidationCallback? validator;

        public WebSocketTransport(ISmoldotLogger logger,
            RemoteCertificateValidationCallback? validationCallback = null) 
        {
            this.logger = logger;
            callbackQ = new CallbackChannel();
            callbackCh = callbackQ.Open();
            validator = validationCallback;
        }

        public override bool ShouldUpdate => callbackCh.rx.Count > 0;

        public void SetCertificationValidator(RemoteCertificateValidationCallback validator)
        {
            this.validator = validator;
        }

        public override void Update()
        {
            while (callbackCh.rx.TryDequeue(out var msg))
            {
                switch (msg)
                {
                    case OnReceivedMsg m:
                        OnReceived?.Invoke(m.connId, m.streamId, m.data);
                        break;
                    case OnOpenMsg m:
                        OnOpen?.Invoke(m.id);
                        break;
                    case OnClosedMsg m:
                        OnClose?.Invoke(m.id, m.why);
                        webSocketTable.Remove(m.id);
                        break;
                    case OnErrorMsg m:
                        OnError?.Invoke(m.id, m.what);
                        break;
                    default:
                        throw new UnexpectedMessageException(msg);
                }
            }
        }

        public override void Close(int id)
        {
            if (webSocketTable.ContainsKey(id))
            {
                webSocketTable[id].Close();
                webSocketTable.Remove(id);
                logger.Log(SmoldotLogLevel.Debug, $"websocket close. {id}");
            }
        }

        public override void CloseAll()
        {
            foreach (var ws in webSocketTable.Values)
            {
                ws.Close();
            }
            webSocketTable.Clear();
        }

        public void Dispose()
        {
            try
            {
                CloseAll();
            }
            catch (Exception e)
            {
                logger.Log(SmoldotLogLevel.Error, $"Error on Dispose(): {e.Message}");
                Debug.Fail("Error on Dispose().");
            }
        }

        public override void Message(int connId, int streamId, Span<byte> data)
        {
            if (webSocketTable.ContainsKey(connId))
            {
                if (webSocketTable[connId].IsAlive)
                {
                    webSocketTable[connId].Send(data.ToArray());
                }
                else
                {
                    logger.Log(SmoldotLogLevel.Warn, $"A message is skipped. Connection is not alive.");
                }
            }
            else
            {
                logger.Log(SmoldotLogLevel.Warn, $"A message is skipped. No such connection ID.");
            }
        }

        public override (bool, string) NewConnection(int id, string addr)
        {
            try
            {
                // TODO: support ipv6
                if (addr.StartsWith("/ip6"))
                {
                    throw new Exception($"Currently only ip4 is supported. addr: {addr}");
                }

                var modAddr = addr
                .Replace("/ip4/", "")
                .Replace("/ws", "")
                .Replace("/tcp", "")
                .Replace('/', ':')
                .Insert(0, "ws://");

                logger.Log(SmoldotLogLevel.Info, $"websocket new connection. addr: {modAddr} raw:{addr}");

                var ws = new WebSocket(modAddr);
                webSocketTable.Add(id, ws);
                if (validator != null)
                {
                    ws.SslConfiguration.ServerCertificateValidationCallback = validator;
                }

                ws.OnMessage += (_, e) =>
                {
                    if (e.IsBinary)
                    {
                        callbackCh.tx.Enqueue(new OnReceivedMsg(id, 0, e.RawData));
                    }
                };
                ws.OnOpen += (_, _e) => callbackCh.tx.Enqueue(new OnOpenMsg(id));
                ws.OnClose += (_, e) => callbackCh.tx.Enqueue(new OnClosedMsg(id, e.Reason));
                ws.OnError += (_, e) => callbackCh.tx.Enqueue(new OnErrorMsg(id, e.Message));
                ws.Connect();
            }
            catch (Exception e)
            {
                logger.Log(SmoldotLogLevel.Error, e.Message);
                //Debug.Fail(e.Message);
                return (false, e.Message);
            }

            return (true, string.Empty);
        }
    }
}