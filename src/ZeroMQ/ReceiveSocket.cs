namespace ZeroMQ
{
    using System;
    using Interop;

    internal class ReceiveSocket : ZmqSocket
    {
        internal ReceiveSocket(SocketProxy socketProxy, SocketType socketType)
            : base(socketProxy, socketType)
        {
        }

        public override int Send(byte[] buffer, int size, SocketFlags flags)
        {
            throw new NotSupportedException();
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
