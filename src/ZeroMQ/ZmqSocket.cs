namespace ZeroMQ
{
    using System;

    using ZeroMQ.Interop;

    /// <summary>
    /// Sends and receives messages across various transports to potentially multiple endpoints
    /// using the ZMQ protocol.
    /// </summary>
    public class ZmqSocket
    {
        private readonly SocketProxy _socketProxy;

        private bool _closed;
        private bool _disposed;

        internal ZmqSocket(SocketProxy socketProxy, SocketType socketType)
        {
            if (socketProxy == null)
            {
                throw new ArgumentNullException("socketProxy");
            }

            _socketProxy = socketProxy;
            SocketType = socketType;
        }

        /// <summary>
        /// Gets the <see cref="ZeroMQ.SocketType"/> value for the current socket.
        /// </summary>
        public SocketType SocketType { get; private set; }

        /// <summary>
        /// Gets or sets the I/O thread affinity for newly created connections on this socket.
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public ulong Affinity
        {
            get { return GetSocketOptionUInt64(SocketOption.AFFINITY); }
            set { SetSocketOption(SocketOption.AFFINITY, value); }
        }

        /// <summary>
        /// Gets or sets the maximum length of the queue of outstanding peer connections. (Default = 100 connections).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public int Backlog
        {
            get { return GetSocketOptionInt32(SocketOption.BACKLOG); }
            set { SetSocketOption(SocketOption.BACKLOG, value); }
        }

        /// <summary>
        /// Gets or sets the identity of the current socket.
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public byte[] Identity
        {
            get { return GetSocketOptionBytes(SocketOption.IDENTITY); }
            set { SetSocketOption(SocketOption.IDENTITY, value); }
        }

        /// <summary>
        /// Gets or sets the linger period for socket shutdown. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public TimeSpan Linger
        {
            get { return TimeSpan.FromMilliseconds(GetSocketOptionInt32(SocketOption.LINGER)); }
            set { SetSocketOption(SocketOption.LINGER, (int)value.TotalMilliseconds); }
        }

        /// <summary>
        /// Gets or sets the maximum size for inbound messages (bytes). (Default = -1, no limit).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public long MaxMessageSize
        {
            get { return GetSocketOptionInt64(SocketOption.MAX_MSG_SIZE); }
            set { SetSocketOption(SocketOption.MAX_MSG_SIZE, value); }
        }

        /// <summary>
        /// Gets or sets the time-to-live field in every multicast packet sent from this socket (network hops). (Default = 1 hop).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public int MulticastHops
        {
            get { return GetSocketOptionInt32(SocketOption.MULTICAST_HOPS); }
            set { SetSocketOption(SocketOption.MULTICAST_HOPS, value); }
        }

        /// <summary>
        /// Gets or sets the maximum send or receive data rate for multicast transports (kbps). (Default = 100 kbps).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public int MulticastRate
        {
            get { return GetSocketOptionInt32(SocketOption.RATE); }
            set { SetSocketOption(SocketOption.RATE, value); }
        }

        /// <summary>
        /// Gets or sets the recovery interval for multicast transports. (Default = 10 seconds).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public TimeSpan MulticastRecoveryInterval
        {
            get { return TimeSpan.FromMilliseconds(GetSocketOptionInt32(SocketOption.RECOVERY_IVL)); }
            set { SetSocketOption(SocketOption.RECOVERY_IVL, (int)value.TotalMilliseconds); }
        }

        /// <summary>
        /// Gets or sets the underlying kernel receive buffer size for the current socket (bytes). (Default = 0, OS default).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public int ReceiveBufferSize
        {
            get { return GetSocketOptionInt32(SocketOption.RCVBUF); }
            set { SetSocketOption(SocketOption.RCVBUF, value); }
        }

        /// <summary>
        /// Gets or sets the high water mark for inbound messages (number of messages). (Default = 0, no limit).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public int ReceiveHighWatermark
        {
            get { return GetSocketOptionInt32(SocketOption.RCVHWM); }
            set { SetSocketOption(SocketOption.RCVHWM, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the multi-part message currently being read has more message parts to follow.
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public bool ReceiveMore
        {
            get { return GetSocketOptionInt32(SocketOption.RCVMORE) == 1; }
        }

        /// <summary>
        /// Gets or sets the timeout for receive operations. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public TimeSpan ReceiveTimeout
        {
            get { return TimeSpan.FromMilliseconds(GetSocketOptionInt32(SocketOption.RCVTIMEO)); }
            set { SetSocketOption(SocketOption.RCVTIMEO, (int)value.TotalMilliseconds); }
        }

        /// <summary>
        /// Gets or sets the initial reconnection interval. (Default = 100 milliseconds).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public TimeSpan ReconnectInterval
        {
            get { return TimeSpan.FromMilliseconds(GetSocketOptionInt32(SocketOption.RECONNECT_IVL)); }
            set { SetSocketOption(SocketOption.RECONNECT_IVL, (int)value.TotalMilliseconds); }
        }

        /// <summary>
        /// Gets or sets the maximum reconnection interval. (Default = 0, only use <see cref="ReconnectInterval"/>).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public TimeSpan ReconnectIntervalMax
        {
            get { return TimeSpan.FromMilliseconds(GetSocketOptionInt32(SocketOption.RECONNECT_IVL_MAX)); }
            set { SetSocketOption(SocketOption.RECONNECT_IVL_MAX, (int)value.TotalMilliseconds); }
        }

        /// <summary>
        /// Gets or sets the underlying kernel transmit buffer size for the current socket (bytes). (Default = 0, OS default).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public int SendBufferSize
        {
            get { return GetSocketOptionInt32(SocketOption.SNDBUF); }
            set { SetSocketOption(SocketOption.SNDBUF, value); }
        }

        /// <summary>
        /// Gets or sets the high water mark for outbound messages (number of messages). (Default = 0, no limit).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public int SendHighWatermark
        {
            get { return GetSocketOptionInt32(SocketOption.SNDHWM); }
            set { SetSocketOption(SocketOption.SNDHWM, value); }
        }

        /// <summary>
        /// Gets or sets the timeout for send operations. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public TimeSpan SendTimeout
        {
            get { return TimeSpan.FromMilliseconds(GetSocketOptionInt32(SocketOption.SNDTIMEO)); }
            set { SetSocketOption(SocketOption.SNDTIMEO, (int)value.TotalMilliseconds); }
        }

        /// <summary>
        /// Gets or sets the supported socket protocol(s) when using TCP transports. (Default = <see cref="ProtocolType.Ipv4Only"/>).
        /// </summary>
        /// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public ProtocolType SupportedProtocol
        {
            get { return (ProtocolType)GetSocketOptionInt32(SocketOption.IPV4_ONLY); }
            set { SetSocketOption(SocketOption.IPV4_ONLY, (int)value); }
        }

        internal IntPtr SocketHandle
        {
            get { return _socketProxy.SocketHandle; }
        }

        /// <summary>
        /// Destroy the current socket.
        /// </summary>
        /// <remarks>
        /// Any outstanding messages physically received from the network but not yet received by the application
        /// with <see cref="Receive"/> shall be discarded. The behaviour for discarding messages sent by the application
        /// with <see cref="Send"/> but not yet physically transferred to the network depends on the value of
        /// the <see cref="Linger"/> socket option.
        /// </remarks>
        /// <exception cref="ZmqSocketException">The underlying socket object is not valid.</exception>
        public void Close()
        {
            if (_disposed || _closed)
            {
                return;
            }

            HandleProxyResult(_socketProxy.Close());

            _closed = true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ZmqSocket"/> class.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal int GetSocketOptionInt32(SocketOption option)
        {
            EnsureNotDisposed();

            int value;

            HandleProxyResult(_socketProxy.GetSocketOption((int)option, out value));

            return value;
        }

        internal long GetSocketOptionInt64(SocketOption option)
        {
            EnsureNotDisposed();

            long value;

            HandleProxyResult(_socketProxy.GetSocketOption((int)option, out value));

            return value;
        }

        internal ulong GetSocketOptionUInt64(SocketOption option)
        {
            EnsureNotDisposed();

            ulong value;

            HandleProxyResult(_socketProxy.GetSocketOption((int)option, out value));

            return value;
        }

        internal byte[] GetSocketOptionBytes(SocketOption option)
        {
            EnsureNotDisposed();

            byte[] value;

            HandleProxyResult(_socketProxy.GetSocketOption((int)option, out value));

            return value;
        }

        internal void SetSocketOption(SocketOption option, int value)
        {
            EnsureNotDisposed();

            HandleProxyResult(_socketProxy.SetSocketOption((int)option, value));
        }

        internal void SetSocketOption(SocketOption option, long value)
        {
            EnsureNotDisposed();

            HandleProxyResult(_socketProxy.SetSocketOption((int)option, value));
        }

        internal void SetSocketOption(SocketOption option, ulong value)
        {
            EnsureNotDisposed();

            HandleProxyResult(_socketProxy.SetSocketOption((int)option, value));
        }

        internal void SetSocketOption(SocketOption option, byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            EnsureNotDisposed();

            HandleProxyResult(_socketProxy.SetSocketOption((int)option, value));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ZmqSocket"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                Close();
            }

            _disposed = true;
        }

        private static void HandleProxyResult(int result)
        {
            // Context termination (ETERM) is an allowable error state, occurring when the
            // ZmqContext was terminated during a socket method.
            if (result == -1 && !ErrorProxy.ContextWasTerminated)
            {
                throw ErrorProxy.GetLastSocketError();
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
