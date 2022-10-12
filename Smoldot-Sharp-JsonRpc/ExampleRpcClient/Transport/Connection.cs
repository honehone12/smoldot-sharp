namespace SimpleRpcClient
{
    public abstract class Connection : IDisposable
    {
        public abstract void Connect();

        public abstract void SendRpc(string json);

        public abstract event Action<int, string> OnRpcResponse;

        public abstract void Dispose();
    }


}
