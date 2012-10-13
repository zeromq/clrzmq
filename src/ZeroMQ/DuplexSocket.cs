namespace ZeroMQ
{
    using System;
    using Interop;

    internal class DuplexSocket : ZmqSocket
    {
        internal DuplexSocket(SocketProxy socketProxy, SocketType socketType)
            : base(socketProxy, socketType)
        {
        }

        public override void Subscribe(byte[] prefix)
        {
            throw new NotSupportedException();
        }

        public override void Unsubscribe(byte[] prefix)
        {
            throw new NotSupportedException();
        }
    }
}
