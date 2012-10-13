namespace ZeroMQ
{
    using System;
    using Interop;

    internal class SendSocket : ZmqSocket
    {
        internal SendSocket(SocketProxy socketProxy, SocketType socketType)
            : base(socketProxy, socketType)
        {
        }

        public override int Receive(byte[] buffer, SocketFlags flags)
        {
            throw new NotSupportedException();
        }

        public override byte[] Receive(byte[] frame, SocketFlags flags, out int size)
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
