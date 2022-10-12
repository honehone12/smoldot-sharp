using System.Collections.Concurrent;

namespace SmoldotSharp
{
    public class OneWayChannel<T> where T : Msgs.Message
    {
        readonly ConcurrentQueue<T> channelQueue = new ConcurrentQueue<T>();

        public class Tx
        {
            readonly ConcurrentQueue<T> q;

            public Tx(ConcurrentQueue<T> q)
            {
                this.q = q;
            }

            public void Enqueue(T msg)
            {
                q.Enqueue(msg);
            }
        }

        public class Rx
        {
            readonly ConcurrentQueue<T> q;

            public Rx(ConcurrentQueue<T> q)
            {
                this.q = q;
            }

            public int Count => q.Count;

            public bool TryDequeue(out T msg)
            {
                return q.TryDequeue(out msg);
            }
        }

        public (Tx, Rx) Open()
        {
            return (new Tx(channelQueue), new Rx(channelQueue));
        }
    }

    public class TwoWayChannel<T> where T : Msgs.Message
    {
        readonly OneWayChannel<T> monoChanA = new OneWayChannel<T>();
        readonly OneWayChannel<T> monoChanB = new OneWayChannel<T>();

        public (Channel<T>, Channel<T>) Open()
        {
            var chanA = monoChanA.Open();
            var chanB = monoChanB.Open();
            return (
                new Channel<T>(chanA.Item1, chanB.Item2),
                new Channel<T>(chanB.Item1, chanA.Item2));
        }
    }

    public class Channel<T> where T : Msgs.Message
    {
        public readonly OneWayChannel<T>.Tx tx;
        public readonly OneWayChannel<T>.Rx rx;

        public Channel(OneWayChannel<T>.Tx tx, OneWayChannel<T>.Rx rx)
        {
            this.tx = tx;
            this.rx = rx;
        }
    }
}
