namespace ZeroMQ.Interop
{
    using System;

    internal class ZmqMsgT : DisposableIntPtr
    {
        private bool _initialized;

        public ZmqMsgT()
            : base(LibZmq.ZmqMsgTSize)
        {
        }

        public static implicit operator IntPtr(ZmqMsgT msg)
        {
            return msg.Ptr;
        }

        public int Init()
        {
            int rc = LibZmq.zmq_msg_init(Ptr);

            _initialized = true;

            return rc;
        }

        public int Init(int size)
        {
            int rc = LibZmq.zmq_msg_init_size(Ptr, size);

            _initialized = true;

            return rc;
        }

        public int Close()
        {
            int rc = LibZmq.zmq_msg_close(Ptr);

            _initialized = false;

            return rc;
        }

        public int Size()
        {
            return LibZmq.zmq_msg_size(Ptr);
        }

        public IntPtr Data()
        {
            return LibZmq.zmq_msg_data(Ptr);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _initialized)
            {
                Close();
            }

            base.Dispose(disposing);
        }
    }
}
