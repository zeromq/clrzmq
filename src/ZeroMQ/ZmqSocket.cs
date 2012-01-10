namespace ZeroMQ
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using ZeroMQ.Interop;

    /// <summary>
    /// Sends and receives messages across various transports to potentially multiple endpoints
    /// using the ZMQ protocol.
    /// </summary>
    public class ZmqSocket : IDisposable
    {
        private static readonly int ProcessorCount = Environment.ProcessorCount;

        private readonly SocketProxy _socketProxy;

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

        ~ZmqSocket()
        {
            Dispose(false);
        }

        /// <summary>
        /// Occurs when at least one message may be received from the socket without blocking.
        /// </summary>
        public event EventHandler<SocketEventArgs> ReceiveReady;

        /// <summary>
        /// Occurs when at least one message may be sent via the socket without blocking.
        /// </summary>
        public event EventHandler<SocketEventArgs> SendReady;

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

        /// <summary>
        /// Gets the status of the last Receive operation.
        /// </summary>
        public ReceiveStatus ReceiveStatus { get; private set; }

        /// <summary>
        /// Gets the status of the last Send operation.
        /// </summary>
        public SendStatus SendStatus { get; private set; }

        internal IntPtr SocketHandle
        {
            get { return _socketProxy.SocketHandle; }
        }

        /// <summary>
        /// Create an endpoint for accepting connections and bind it to the current socket.
        /// </summary>
        /// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred binding the socket to an endpoint.</exception>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public void Bind(string endpoint)
        {
            EnsureNotDisposed();

            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            if (endpoint == string.Empty)
            {
                throw new ArgumentException("Unable to Bind to an empty endpoint.", "endpoint");
            }

            HandleProxyResult(_socketProxy.Bind(endpoint));
        }

        /// <summary>
        /// Connect the current socket to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred connecting the socket to a remote endpoint.</exception>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        public void Connect(string endpoint)
        {
            EnsureNotDisposed();

            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            if (endpoint == string.Empty)
            {
                throw new ArgumentException("Unable to Connect to an empty endpoint.", "endpoint");
            }

            HandleProxyResult(_socketProxy.Connect(endpoint));
        }

        /// <summary>
        /// Destroy the current socket.
        /// </summary>
        /// <remarks>
        /// Any outstanding messages physically received from the network but not yet received by the application
        /// with Receive shall be discarded. The behaviour for discarding messages sent by the application
        /// with Send but not yet physically transferred to the network depends on the value of
        /// the <see cref="Linger"/> socket option.
        /// </remarks>
        /// <exception cref="ZmqSocketException">The underlying socket object is not valid.</exception>
        public void Close()
        {
            HandleProxyResult(_socketProxy.Close());
        }

        /// <summary>
        /// Receive a single message-part from a remote socket in blocking mode.
        /// </summary>
        /// <remarks>
        /// Warning: This overload will only receive as much data as can fit in the supplied <paramref name="buffer"/>.
        /// It is intended to be used when the maximum messaging performance is required, as it does not perform
        /// any unnecessary memory allocation, copying or marshalling.
        /// If the maximum message size is not known in advance, use the <see cref="Receive(ZeroMQ.Frame)"/> overload.
        /// </remarks>
        /// <param name="buffer">A <see cref="byte"/> array that will store the received data.</param>
        /// <returns>The number of bytes contained in the resulting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public int Receive(byte[] buffer)
        {
            return Receive(buffer, SocketFlags.None);
        }

        /// <summary>
        /// Receive a single message-part from a remote socket in non-blocking mode with a specified timeout.
        /// </summary>
        /// <remarks>
        /// Warning: This overload will only receive as much data as can fit in the supplied <paramref name="buffer"/>.
        /// It is intended to be used when the maximum messaging performance is required, as it does not perform
        /// any unnecessary memory allocation, copying or marshalling.
        /// If the maximum message size is not known in advance, use the <see cref="Receive(ZeroMQ.Frame,TimeSpan)"/> overload.
        /// </remarks>
        /// <param name="buffer">A <see cref="byte"/> array that will store the received data.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> specifying the receive timeout.</param>
        /// <returns>
        /// The number of bytes contained in the resulting message or -1 if the timeout expired or an interrupt occurred.
        /// See <see cref="ReceiveStatus"/> for details.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public int Receive(byte[] buffer, TimeSpan timeout)
        {
            return timeout == TimeSpan.MaxValue
                       ? Receive(buffer)
                       : ExecuteWithTimeout(() => Receive(buffer, SocketFlags.DontWait), timeout);
        }

        /// <summary>
        /// Receive a single frame from a remote socket in blocking mode.
        /// </summary>
        /// <remarks>
        /// This overload will receive all available data in the message-part. If the buffer size of <paramref name="frame"/>
        /// is insufficient, a new buffer will be allocated.
        /// </remarks>
        /// <param name="frame">A <see cref="Frame"/> that will store the received data.</param>
        /// <returns>A <see cref="Frame"/> containing the data received from the remote endpoint.</returns>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public Frame Receive(Frame frame)
        {
            return Receive(frame, SocketFlags.None);
        }

        /// <summary>
        /// Receive a single frame from a remote socket in non-blocking mode with a specified timeout.
        /// </summary>
        /// <remarks>
        /// This overload will receive all available data in the message-part. If the buffer size of <paramref name="frame"/>
        /// is insufficient, a new buffer will be allocated.
        /// </remarks>
        /// <param name="frame">A <see cref="Frame"/> that will store the received data.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> specifying the receive timeout.</param>
        /// <returns>A <see cref="Frame"/> containing the data received from the remote endpoint.</returns>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public Frame Receive(Frame frame, TimeSpan timeout)
        {
            return timeout == TimeSpan.MaxValue
                       ? Receive(frame)
                       : ExecuteWithTimeout(() => Receive(frame, SocketFlags.DontWait), timeout);
        }

        /// <summary>
        /// Queue a single-part (or final multi-part) message buffer to be sent by the socket in blocking mode.
        /// </summary>
        /// <param name="buffer">A <see cref="byte"/> array that contains the message to be sent.</param>
        /// <param name="size">The size of the message to send.</param>
        /// <param name="flags">A combination of <see cref="SocketFlags"/> values to use when sending.</param>
        /// <returns>The number of bytes sent by the socket.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is a negative value or is larger than the length of <paramref name="buffer"/>.</exception>
        /// <exception cref="ZmqSocketException">An error occurred sending data to a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Send operations.</exception>
        public virtual int Send(byte[] buffer, int size, SocketFlags flags)
        {
            EnsureNotDisposed();

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (size < 0 || size > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("size", "Expected a non-negative value less than or equal to the buffer length.");
            }

            int sentBytes = _socketProxy.Send(buffer, size, (int)flags);

            if (sentBytes >= 0)
            {
                SendStatus = SendStatus.Sent;
                return sentBytes;
            }

            if (ErrorProxy.ShouldTryAgain)
            {
                SendStatus = SendStatus.TryAgain;
                return -1;
            }

            if (ErrorProxy.ContextWasTerminated)
            {
                SendStatus = SendStatus.Interrupted;
                return -1;
            }

            throw new ZmqSocketException(ErrorProxy.GetLastError());
        }

        /// <summary>
        /// Queue a single-part (or final multi-part) message buffer to be sent by the socket in non-blocking mode with a specified timeout.
        /// </summary>
        /// <param name="buffer">A <see cref="byte"/> array that contains the message to be sent.</param>
        /// <param name="size">The size of the message to send.</param>
        /// <param name="flags">A combination of <see cref="SocketFlags"/> values to use when sending.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> specifying the send timeout.</param>
        /// <returns>The number of bytes sent by the socket.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is a negative value or is larger than the length of <paramref name="buffer"/>.</exception>
        /// <exception cref="ZmqSocketException">An error occurred sending data to a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Send operations.</exception>
        public int Send(byte[] buffer, int size, SocketFlags flags, TimeSpan timeout)
        {
            return timeout == TimeSpan.Zero
                       ? Send(buffer, size, flags & ~SocketFlags.DontWait)
                       : ExecuteWithTimeout(() => Send(buffer, size, flags | SocketFlags.DontWait), timeout);
        }

        /// <summary>
        /// Subscribe to all messages.
        /// </summary>
        /// <remarks>
        /// Only applies to <see cref="ZeroMQ.SocketType.SUB"/> and <see cref="ZeroMQ.SocketType.XSUB"/> sockets.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
        public void SubscribeAll()
        {
            Subscribe(new byte[0]);
        }

        /// <summary>
        /// Subscribe to messages that begin with a specified prefix.
        /// </summary>
        /// <remarks>
        /// Only applies to <see cref="ZeroMQ.SocketType.SUB"/> and <see cref="ZeroMQ.SocketType.XSUB"/> sockets.
        /// </remarks>
        /// <param name="prefix">Prefix for subscribed messages.</param>
        /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
        public virtual void Subscribe(byte[] prefix)
        {
            SetSocketOption(SocketOption.SUBSCRIBE, prefix);
        }

        /// <summary>
        /// Unsubscribe from all messages.
        /// </summary>
        /// <remarks>
        /// Only applies to <see cref="ZeroMQ.SocketType.SUB"/> and <see cref="ZeroMQ.SocketType.XSUB"/> sockets.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
        public void UnsubscribeAll()
        {
            Unsubscribe(new byte[0]);
        }

        /// <summary>
        /// Unsubscribe from messages that begin with a specified prefix.
        /// </summary>
        /// <remarks>
        /// Only applies to <see cref="ZeroMQ.SocketType.SUB"/> and <see cref="ZeroMQ.SocketType.XSUB"/> sockets.
        /// </remarks>
        /// <param name="prefix">Prefix for subscribed messages.</param>
        /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
        public virtual void Unsubscribe(byte[] prefix)
        {
            SetSocketOption(SocketOption.UNSUBSCRIBE, prefix);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ZmqSocket"/> class.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal virtual int Receive(byte[] buffer, SocketFlags flags)
        {
            EnsureNotDisposed();

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            int receivedBytes = _socketProxy.Receive(buffer, (int)flags);

            if (receivedBytes >= 0)
            {
                ReceiveStatus = ReceiveStatus.Received;
                return receivedBytes;
            }

            if (ErrorProxy.ShouldTryAgain)
            {
                ReceiveStatus = ReceiveStatus.TryAgain;
                return -1;
            }

            if (ErrorProxy.ContextWasTerminated)
            {
                ReceiveStatus = ReceiveStatus.Interrupted;
                return -1;
            }

            throw new ZmqSocketException(ErrorProxy.GetLastError());
        }

        internal virtual Frame Receive(Frame frame, SocketFlags flags)
        {
            EnsureNotDisposed();

            if (frame == null)
            {
                frame = new Frame(0);
            }

            int receivedBytes;

            frame.Buffer = _socketProxy.Receive(frame.Buffer, (int)flags, out receivedBytes);

            if (receivedBytes >= 0)
            {
                frame.ReceiveStatus = ReceiveStatus = ReceiveStatus.Received;
                frame.MessageSize = receivedBytes;
                frame.HasMore = ReceiveMore;

                return frame;
            }

            if (ErrorProxy.ShouldTryAgain)
            {
                frame.ReceiveStatus = ReceiveStatus = ReceiveStatus.TryAgain;
                return frame;
            }

            if (ErrorProxy.ContextWasTerminated)
            {
                frame.ReceiveStatus = ReceiveStatus = ReceiveStatus.Interrupted;
                return frame;
            }

            throw new ZmqSocketException(ErrorProxy.GetLastError());
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

        internal void InvokePollEvents(PollEvents readyEvents)
        {
            if (readyEvents.HasFlag(PollEvents.PollIn))
            {
                InvokeReceiveReady(readyEvents);
            }

            if (readyEvents.HasFlag(PollEvents.PollOut))
            {
                InvokeSendReady(readyEvents);
            }
        }

        internal virtual PollEvents GetPollEvents()
        {
            return PollEvents.PollIn | PollEvents.PollOut;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ZmqSocket"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _socketProxy.Dispose();
                }
            }

            _disposed = true;
        }

        private static void HandleProxyResult(int result)
        {
            // Context termination (ETERM) is an allowable error state, occurring when the
            // ZmqContext was terminated during a socket method.
            if (result == -1 && !ErrorProxy.ContextWasTerminated)
            {
                throw new ZmqSocketException(ErrorProxy.GetLastError());
            }
        }

        private TResult ExecuteWithTimeout<TResult>(Func<TResult> method, TimeSpan timeout)
        {
            TResult receiveResult;

            int iterations = 0;
            var timeoutMilliseconds = (int)timeout.TotalMilliseconds;
            var timer = Stopwatch.StartNew();

            do
            {
                receiveResult = method();

                if (ReceiveStatus != ReceiveStatus.TryAgain || timeoutMilliseconds <= 1)
                {
                    break;
                }

                if (iterations < 20 && ProcessorCount > 1)
                {
                    // If we have a short wait (< 20 iterations) we SpinWait to allow other threads
                    // on HyperThreaded CPUs to use the CPU. The more CPUs we have, the longer it's
                    // acceptable to SpinWait since we stall the overall system less.
                    Thread.SpinWait(100 * ProcessorCount);
                }
                else
                {
                    Thread.Yield();
                }

                ++iterations;
            }
            while (timer.Elapsed < timeout);

            return receiveResult;
        }

        private void InvokeReceiveReady(PollEvents readyEvents)
        {
            EventHandler<SocketEventArgs> handler = ReceiveReady;
            if (handler != null)
            {
                handler(this, new SocketEventArgs(this, readyEvents));
            }
        }

        private void InvokeSendReady(PollEvents readyEvents)
        {
            EventHandler<SocketEventArgs> handler = SendReady;
            if (handler != null)
            {
                handler(this, new SocketEventArgs(this, readyEvents));
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
