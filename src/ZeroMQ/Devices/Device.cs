namespace ZeroMQ.Devices
{
    using System;
    using System.Threading;

    /// <summary>
    /// Forwards messages received by a front-end socket to a back-end socket, from which
    /// they are then sent.
    /// </summary>
    /// <remarks>
    /// The base implementation of <see cref="Device"/> is <b>not</b> threadsafe. Do not construct
    /// a device with sockets that were created in separate threads or separate contexts.
    /// </remarks>
    public abstract class Device : IDevice
    {
        /// <summary>
        /// The polling interval in milliseconds.
        /// </summary>
        protected const int PollingIntervalMsec = 500;

        /// <summary>
        /// The frontend socket that will normally pass messages to <see cref="BackendSocket"/>.
        /// </summary>
        protected readonly ZmqSocket FrontendSocket;

        /// <summary>
        /// The backend socket that will normally receive messages from (and possibly send replies to) <see cref="FrontendSocket"/>.
        /// </summary>
        protected readonly ZmqSocket BackendSocket;

        private volatile bool _isRunning;

        private bool _isInitialized;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        /// <param name="frontendSocket">
        /// A <see cref="ZmqSocket"/> that will pass incoming messages to <paramref name="backendSocket"/>.
        /// </param>
        /// <param name="backendSocket">
        /// A <see cref="ZmqSocket"/> that will receive messages from (and optionally send replies to) <paramref name="frontendSocket"/>.
        /// </param>
        protected Device(ZmqSocket frontendSocket, ZmqSocket backendSocket)
        {
            if (frontendSocket == null)
            {
                throw new ArgumentNullException("frontendSocket");
            }

            if (backendSocket == null)
            {
                throw new ArgumentNullException("backendSocket");
            }

            FrontendSocket = frontendSocket;
            BackendSocket = backendSocket;
            DoneEvent = new ManualResetEvent(false);
            ReadyEvent = new ManualResetEvent(false);
        }

        ~Device()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a value indicating whether the device loop is running.
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            private set { _isRunning = value; }
        }

        /// <summary>
        /// Gets a <see cref="ManualResetEvent"/> that can be used to block while the device is running.
        /// </summary>
        public ManualResetEvent DoneEvent { get; private set; }

        /// <summary>
        /// Gets a <see cref="ManualResetEvent"/> that signals when the device is completely ready.
        /// </summary>
        protected ManualResetEvent ReadyEvent { get; private set; }

        /// <summary>
        /// Initializes the frontend and backend sockets. Called automatically when starting the device,
        /// but will only execute once.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            InitializeSockets();
            _isInitialized = true;
        }

        /// <summary>
        /// Start the device in the current thread.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The <see cref="Device"/> has already been disposed.</exception>
        public virtual void Start()
        {
            Run();
        }

        /// <summary>
        /// Blocks the calling thread until the device terminates.
        /// </summary>
        public virtual void Join()
        {
            DoneEvent.WaitOne();
        }

        /// <summary>
        /// Blocks the calling thread until the device terminates or the specified time elapses.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> set to the amount of time to wait for the device to terminate.
        /// </param>
        /// <returns>
        /// true if the device terminated; false if the device has not terminated after
        /// the amount of time specified by <paramref name="timeout"/> has elapsed.
        /// </returns>
        public virtual bool Join(TimeSpan timeout)
        {
            return DoneEvent.WaitOne(timeout);
        }

        /// <summary>
        /// Stop the device in such a way that it can be restarted.
        /// </summary>
        public virtual void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Stop the device and safely terminate the underlying sockets.
        /// </summary>
        public virtual void Close()
        {
            if (IsRunning)
            {
                Stop();
                Join(TimeSpan.FromMilliseconds(PollingIntervalMsec * 2));
            }

            FrontendSocket.Close();
            BackendSocket.Close();
        }

        /// <summary>
        /// Releases all resources used by the current instance, including the frontend and backend sockets.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes sockets before the device starts.
        /// </summary>
        /// <remarks>
        /// Use this method to Bind, Connect and set socket subscriptions as necessary.
        /// </remarks>
        protected abstract void InitializeSockets();

        /// <summary>
        /// Invoked when a message has been received by the frontend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
        protected abstract void FrontendHandler(SocketEventArgs args);

        /// <summary>
        /// Invoked when a message has been received by the backend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
        protected abstract void BackendHandler(SocketEventArgs args);

        /// <summary>
        /// Start the device in the current thread. Should be used by implementations of
        /// the <see cref="Start"/> method.
        /// </summary>
        /// <remarks>
        /// Initializes the sockets prior to starting the device with <see cref="InitializeSockets"/>.
        /// </remarks>
        protected void Run()
        {
            EnsureNotDisposed();

            Initialize();

            FrontendSocket.ReceiveReady += (sender, args) => FrontendHandler(args);
            BackendSocket.ReceiveReady += (sender, args) => BackendHandler(args);

            var poller = new Poller(new[] { FrontendSocket, BackendSocket });
            TimeSpan timeout = TimeSpan.FromMilliseconds(PollingIntervalMsec);

            DoneEvent.Reset();
            ReadyEvent.Reset();
            IsRunning = true;

            try
            {
                while (IsRunning)
                {
                    poller.Poll(timeout);

                    ReadyEvent.Set();
                }
            }
            catch (ZmqException)
            {
                // Swallow any exceptions thrown while stopping
                if (IsRunning)
                {
                    throw;
                }
            }

            IsRunning = false;
            DoneEvent.Set();
        }

        /// <summary>
        /// Stops the device and releases the underlying sockets. Optionally disposes of managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (IsRunning)
            {
                Stop();
                Join(TimeSpan.FromMilliseconds(PollingIntervalMsec * 2));
            }

            if (disposing)
            {
                FrontendSocket.Dispose();
                BackendSocket.Dispose();
            }

            _disposed = true;
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
