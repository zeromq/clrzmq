namespace ZeroMQ.Interop
{
    using System;
    using System.Runtime.InteropServices;

    internal class SocketProxy : IDisposable
    {
        private const int MaxBinaryOptionSize = 255;

        // From zmq.h:
        // typedef struct {unsigned char _ [32];} zmq_msg_t;
        private static readonly int ZmqMsgTSize = IntPtr.Size + 32;

        private IntPtr _message;
        private bool _disposed;

        public SocketProxy(IntPtr socketHandle)
        {
            if (socketHandle == IntPtr.Zero)
            {
                throw new ArgumentException("Socket handle must be a valid pointer.", "socketHandle");
            }

            SocketHandle = socketHandle;
            _message = Marshal.AllocHGlobal(ZmqMsgTSize);
        }

        ~SocketProxy()
        {
            Dispose(false);
        }

        public IntPtr SocketHandle { get; private set; }

        public int Bind(string endpoint)
        {
            return LibZmq.zmq_bind(SocketHandle, endpoint);
        }

        public int Connect(string endpoint)
        {
            return LibZmq.zmq_connect(SocketHandle, endpoint);
        }

        public int Close()
        {
            // Allow Close to be called repeatedly without failure
            if (SocketHandle == IntPtr.Zero)
            {
                return 0;
            }

            int rc = LibZmq.zmq_close(SocketHandle);

            SocketHandle = IntPtr.Zero;

            return rc;
        }

        public int Receive(byte[] buffer, int flags)
        {
            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            if (LibZmq.zmq_msg_init_data(_message, bufferHandle.AddrOfPinnedObject(), buffer.Length, FreeBufferHandle, IntPtr.Zero) == -1)
            {
                return -1;
            }

            int bytesReceived = -1;
            RetryIfInterrupted(() => bytesReceived = LibZmq.zmq_recvmsg(SocketHandle, _message, flags));

            return bytesReceived;
        }

        public byte[] Receive(byte[] buffer, int flags, out int size)
        {
            size = -1;

            if (LibZmq.zmq_msg_init(_message) == -1)
            {
                return buffer;
            }

            int bytesReceived = -1;
            RetryIfInterrupted(() => bytesReceived = LibZmq.zmq_recvmsg(SocketHandle, _message, flags));

            if (bytesReceived >= 0)
            {
                size = bytesReceived;

                if (buffer == null || size > buffer.Length)
                {
                    buffer = new byte[size];
                }

                Marshal.Copy(LibZmq.zmq_msg_data(_message), buffer, 0, size);
            }

            LibZmq.zmq_msg_close(_message);

            return buffer;
        }

        public int Send(byte[] buffer, int size, int flags)
        {
            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            if (LibZmq.zmq_msg_init_data(_message, bufferHandle.AddrOfPinnedObject(), size, FreeBufferHandle, IntPtr.Zero) == -1)
            {
                return -1;
            }

            int bytesSent = -1;
            RetryIfInterrupted(() => bytesSent = LibZmq.zmq_sendmsg(SocketHandle, _message, flags));

            return bytesSent;
        }

        public int GetSocketOption(int option, out int value)
        {
            using (var optionLength = new DisposableIntPtr(IntPtr.Size))
            using (var optionValue = new DisposableIntPtr(Marshal.SizeOf(typeof(int))))
            {
                Marshal.WriteInt32(optionLength, sizeof(int));

                int rc = RetryIfInterrupted(() => LibZmq.zmq_getsockopt(SocketHandle, option, optionValue, optionLength));
                value = Marshal.ReadInt32(optionValue);

                return rc;
            }
        }

        public int GetSocketOption(int option, out long value)
        {
            using (var optionLength = new DisposableIntPtr(IntPtr.Size))
            using (var optionValue = new DisposableIntPtr(Marshal.SizeOf(typeof(long))))
            {
                Marshal.WriteInt32(optionLength, sizeof(long));

                int rc = RetryIfInterrupted(() => LibZmq.zmq_getsockopt(SocketHandle, option, optionValue, optionLength));
                value = Marshal.ReadInt64(optionValue);

                return rc;
            }
        }

        public int GetSocketOption(int option, out ulong value)
        {
            using (var optionLength = new DisposableIntPtr(IntPtr.Size))
            using (var optionValue = new DisposableIntPtr(Marshal.SizeOf(typeof(ulong))))
            {
                Marshal.WriteInt32(optionLength, sizeof(ulong));

                int rc = RetryIfInterrupted(() => LibZmq.zmq_getsockopt(SocketHandle, option, optionValue, optionLength));
                value = unchecked(Convert.ToUInt64(Marshal.ReadInt64(optionValue)));

                return rc;
            }
        }

        public int GetSocketOption(int option, out byte[] value)
        {
            using (var optionLength = new DisposableIntPtr(IntPtr.Size))
            using (var optionValue = new DisposableIntPtr(MaxBinaryOptionSize))
            {
                Marshal.WriteInt32(optionLength, MaxBinaryOptionSize);

                int rc = RetryIfInterrupted(() => LibZmq.zmq_getsockopt(SocketHandle, option, optionValue, optionLength));

                value = new byte[Marshal.ReadInt32(optionLength)];
                Marshal.Copy(optionValue, value, 0, value.Length);

                return rc;
            }
        }

        public int SetSocketOption(int option, int value)
        {
            using (var optionValue = new DisposableIntPtr(Marshal.SizeOf(typeof(int))))
            {
                Marshal.WriteInt32(optionValue, value);

                return RetryIfInterrupted(() => LibZmq.zmq_setsockopt(SocketHandle, option, optionValue, sizeof(int)));
            }
        }

        public int SetSocketOption(int option, long value)
        {
            using (var optionValue = new DisposableIntPtr(Marshal.SizeOf(typeof(long))))
            {
                Marshal.WriteInt64(optionValue, value);

                return RetryIfInterrupted(() => LibZmq.zmq_setsockopt(SocketHandle, option, optionValue, sizeof(long)));
            }
        }

        public int SetSocketOption(int option, ulong value)
        {
            using (var optionValue = new DisposableIntPtr(Marshal.SizeOf(typeof(ulong))))
            {
                Marshal.WriteInt64(optionValue, unchecked(Convert.ToInt64(value)));

                return RetryIfInterrupted(() => LibZmq.zmq_setsockopt(SocketHandle, option, optionValue, sizeof(int)));
            }
        }

        public int SetSocketOption(int option, byte[] value)
        {
            using (var optionValue = new DisposableIntPtr(value.Length))
            {
                Marshal.Copy(value, 0, optionValue, value.Length);

                return RetryIfInterrupted(() => LibZmq.zmq_setsockopt(SocketHandle, option, optionValue, value.Length));
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_message != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_message);
                    _message = IntPtr.Zero;
                }

                Close();
            }

            _disposed = true;
        }

        private static int RetryIfInterrupted(Func<int> func)
        {
            int rc;

            do
            {
                rc = func();
            }
            while (rc == -1 && LibZmq.zmq_errno() == ErrorCode.EINTR);

            return rc;
        }

        private static void FreeBufferHandle(IntPtr data, IntPtr hint)
        {
            GCHandle bufferHandle = GCHandle.FromIntPtr(data);
            bufferHandle.Free();
        }
    }
}
