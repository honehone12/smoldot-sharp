using System;

namespace SmoldotSharp
{
    public abstract class SmoldotTransport 
    {
        public Action<int>? OnOpen;

        public Action<int, string>? OnClose;

        public Action<int, string>? OnError;

        public Action<int, int, byte[]>? OnReceived;

        public abstract bool ShouldUpdate { get; }

        public abstract void Update();

        public abstract (bool, string) NewConnection(int id, string addr);

        public abstract void Message(int connId, int streamId, Span<byte> data);

        public abstract void Close(int id);

        public abstract void CloseAll();
    }
}