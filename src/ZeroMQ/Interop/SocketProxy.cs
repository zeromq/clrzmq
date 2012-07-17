namespace ZeroMQ.Interop
{
    using System;
    using System.Runtime.InteropServices;

    // ReSharper disable AccessToDisposedClosure
    internal class SocketProxy : IDisposable
    {
        private readonly Action<IntPtr> socketClosed;

        // From options.hpp: unsigned char identity [256];
        private const int MaxBinaryOptionSize = 255;

        private bool _disposed;

        public SocketProxy(IntPtr socketHandle, Action<IntPtr> socketClosed)
        {
            this.socketClosed = socketClosed;
            if (socketHandle == IntPtr.Zero)
            {
                throw new ArgumentException("Socket handle must be a valid pointer.", "socketHandle");
            }

            SocketHandle = socketHandle;
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

        public int Unbind(string endpoint)
        {
            return LibZmq.zmq_unbind(SocketHandle, endpoint);
        }

        public int Connect(string endpoint)
        {
            return LibZmq.zmq_connect(SocketHandle, endpoint);
        }

        public int Disconnect(string endpoint)
        {
            return LibZmq.zmq_disconnect(SocketHandle, endpoint);
        }

        public int Close()
        {
            // Allow Close to be called repeatedly without failure
            if (SocketHandle == IntPtr.Zero)
            {
                return 0;
            }

            int rc = LibZmq.zmq_close(SocketHandle);
            socketClosed(SocketHandle);
            
            SocketHandle = IntPtr.Zero;

            return rc;
        }

        public int Receive(byte[] buffer, int flags)
        {
            using (var message = new ZmqMsgT())
            {
                if (message.Init(buffer.Length) == -1)
                {
                    return -1;
                }

                int bytesReceived = RetryIfInterrupted(() => LibZmq.zmq_msg_recv(message, SocketHandle, flags));

                if (bytesReceived == 0 && LibZmq.MajorVersion < 3)
                {
                    // 0MQ 2.x does not return number of bytes received
                    bytesReceived = message.Size();
                }

                if (bytesReceived > 0)
                {
                    Marshal.Copy(message.Data(), buffer, 0, bytesReceived);
                }

                if (message.Close() == -1)
                {
                    return -1;
                }

                return bytesReceived;
            }
        }

        public byte[] Receive(byte[] buffer, int flags, out int size)
        {
            size = -1;

            using (var message = new ZmqMsgT())
            {
                if (message.Init() == -1)
                {
                    return buffer;
                }

                int bytesReceived = RetryIfInterrupted(() => LibZmq.zmq_msg_recv(message, SocketHandle, flags));

                if (bytesReceived >= 0)
                {
                    if (bytesReceived == 0 && LibZmq.MajorVersion < 3)
                    {
                        // 0MQ 2.x does not return number of bytes received
                        bytesReceived = message.Size();
                    }

                    size = bytesReceived;

                    if (buffer == null || size > buffer.Length)
                    {
                        buffer = new byte[size];
                    }

                    Marshal.Copy(message.Data(), buffer, 0, size);
                }

                if (message.Close() == -1)
                {
                    size = -1;
                }

                return buffer;
            }
        }

        public int Send(byte[] buffer, int size, int flags)
        {
            using (var message = new ZmqMsgT())
            {
                if (message.Init(size) == -1)
                {
                    return -1;
                }

                if (size > 0)
                {
                    Marshal.Copy(buffer, 0, message.Data(), size);
                }

                int bytesSent = RetryIfInterrupted(() => LibZmq.zmq_msg_send(message, SocketHandle, flags));

                if (bytesSent == 0 && LibZmq.MajorVersion < 3)
                {
                    // 0MQ 2.x does not report number of bytes sent, so this may be inaccurate/misleading
                    bytesSent = size;
                }

                if (message.Close() == -1)
                {
                    return -1;
                }

                return bytesSent;
            }
        }

        public int Forward(IntPtr destinationHandle)
        {
            using (var message = new ZmqMsgT())
            {
                if (message.Init() == -1)
                {
                    return -1;
                }

                int receiveMore;
                int bytesSent;

                do
                {
                    if (LibZmq.zmq_msg_recv(message, SocketHandle, 0) == -1)
                    {
                        return -1;
                    }

                    if (GetReceiveMore(out receiveMore) == -1)
                    {
                        return -1;
                    }

                    if ((bytesSent = LibZmq.zmq_msg_send(message, destinationHandle, receiveMore == 1 ? (int)SocketFlags.SendMore : 0)) == -1)
                    {
                        return -1;
                    }
                }
                while (receiveMore == 1);

                if (message.Close() == -1)
                {
                    return -1;
                }

                return bytesSent;
            }
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

        public int GetSocketOption(int option, out string value)
        {
            using (var optionLength = new DisposableIntPtr(IntPtr.Size))
            using (var optionValue = new DisposableIntPtr(MaxBinaryOptionSize))
            {
                Marshal.WriteInt32(optionLength, MaxBinaryOptionSize);

                int rc = RetryIfInterrupted(() => LibZmq.zmq_getsockopt(SocketHandle, option, optionValue, optionLength));

                value = rc == 0 ? Marshal.PtrToStringAnsi(optionValue) : string.Empty;

                return rc;
            }
        }

        public int SetSocketOption(int option, string value)
        {
            if (value == null)
            {
                return RetryIfInterrupted(() => LibZmq.zmq_setsockopt(SocketHandle, option, IntPtr.Zero, 0));
            }

            var encoded = System.Text.Encoding.ASCII.GetBytes(value + "\x0");
            using (var optionValue = new DisposableIntPtr(encoded.Length))
            {
                Marshal.Copy(encoded, 0, optionValue, encoded.Length);

                return RetryIfInterrupted(() => LibZmq.zmq_setsockopt(SocketHandle, option, optionValue, value.Length));
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

                return RetryIfInterrupted(() => LibZmq.zmq_setsockopt(SocketHandle, option, optionValue, sizeof(ulong)));
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

        private int GetReceiveMore(out int receiveMore)
        {
            if (LibZmq.MajorVersion >= 3)
            {
                return GetSocketOption((int)SocketOption.RCVMORE, out receiveMore);
            }

            long value;
            int rc = GetSocketOption((int)SocketOption.RCVMORE, out value);
            receiveMore = (int)value;

            return rc;
        }
    }
    // ReSharper restore AccessToDisposedClosure
}
