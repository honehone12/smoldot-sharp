using System.Net.Security;
using WebSocketSharp;

namespace SimpleRpcClient
{
    public class WebSocketConnection : Connection
    {
        readonly WebSocket ws;
        readonly int connId;

        public override event Action<int, string> OnRpcResponse;
        
        public WebSocketConnection(int id, string addr, 
            Action<int, string> onRpcResponse)
        {
            ws = new WebSocket(addr);
            connId = id;
            onRpcResponse ??= (id, str) => 
                Console.WriteLine($"Response : {id}", str);
            OnRpcResponse += onRpcResponse;
            ws.OnMessage += OnWsMessage;
            ws.OnError += (_, arg) =>
            {
                Console.WriteLine(arg.Message);
            };
        }

        public void SetCertificateValidation(RemoteCertificateValidationCallback validation)
        {
            ws.SslConfiguration.ServerCertificateValidationCallback = validation;
        }

        public override void Connect()
        {
            ws.Connect();
        }

        void OnWsMessage(object? _, MessageEventArgs args)
        {
            if (args.IsText)
            {
                //Console.WriteLine(args.Data);
                OnRpcResponse?.Invoke(connId, args.Data);
            }
        }

        public override void SendRpc(string json)
        {
            if (ws.IsAlive)
            {
                //Console.WriteLine(json);
                ws.Send(json);
            }
        }

        public override void Dispose()
        {
            ws.Close();
        }
    }
}
