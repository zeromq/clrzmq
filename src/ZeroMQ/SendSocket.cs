namespace ZeroMQ
{
    using System;
    using ZeroMQ.Interop;

    internal class SendSocket : ZmqSocket
    {
        internal SendSocket(SocketProxy socketProxy, SocketType socketType)
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

        internal override int Receive(byte[] buffer, SocketFlags flags)
        {
            throw new NotSupportedException();
        }

        internal override Frame Receive(Frame frame, SocketFlags flags)
        {
            throw new NotSupportedException();
        }

        internal override PollEvents GetPollEvents()
        {
            return PollEvents.PollOut;
        }
    }
}
