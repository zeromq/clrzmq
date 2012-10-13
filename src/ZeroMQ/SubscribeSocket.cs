namespace ZeroMQ
{
    using System;
    using Interop;

    internal class SubscribeSocket : ZmqSocket
    {
        internal SubscribeSocket(SocketProxy socketProxy, SocketType socketType)
            : base(socketProxy, socketType)
        {
        }

        public override int Send(byte[] buffer, int size, SocketFlags flags)
        {
            throw new NotSupportedException();
        }
    }
}
