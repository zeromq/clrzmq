namespace ZeroMQ
{
    using System;
    using Interop;

    internal class SubscribeExtSocket : ZmqSocket
    {
        public const byte SubscribePrefix = 1;
        public const byte UnsubscribePrefix = 0;

        internal SubscribeExtSocket(SocketProxy socketProxy, SocketType socketType)
            : base(socketProxy, socketType)
        {
        }

        public override void Subscribe(byte[] prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }

            this.SendWithPrefix(prefix, SubscribePrefix);
        }

        public override void Unsubscribe(byte[] prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }

            this.SendWithPrefix(prefix, UnsubscribePrefix);
        }

        private void SendWithPrefix(byte[] buffer, byte prefix)
        {
            var prefixedBuffer = new byte[buffer.Length + 1];

            prefixedBuffer[0] = prefix;
            buffer.CopyTo(prefixedBuffer, 1);

            this.Send(prefixedBuffer, prefixedBuffer.Length, SocketFlags.None);
        }
    }
}
