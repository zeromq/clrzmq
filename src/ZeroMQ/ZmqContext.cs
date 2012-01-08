namespace ZeroMQ
{
    using System;
    using System.Text;
    using ZeroMQ.Interop;

    /// <summary>
    /// Creates <see cref="ZmqSocket"/> instances within a process boundary.
    /// </summary>
    /// <remarks>
    /// The <see cref="ZmqContext"/> object is a container for all sockets in a single process,
    /// and acts as the transport for inproc sockets. <see cref="ZmqContext"/> is thread safe.
    /// A <see cref="ZmqContext"/> must not be terminated until all spawned sockets have been
    /// successfully closed.
    /// </remarks>
    public class ZmqContext : IDisposable
    {
        private const int DefaultThreadPoolSize = 1;

        private readonly ContextProxy _contextProxy;

        private bool _disposed;

        static ZmqContext()
        {
            DefaultEncoding = Encoding.UTF8;
        }

        internal ZmqContext(ContextProxy contextProxy)
        {
            if (contextProxy == null)
            {
                throw new ArgumentNullException("contextProxy");
            }

            _contextProxy = contextProxy;
        }

        ~ZmqContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets the default encoding for all sockets in the current process.
        /// </summary>
        public static Encoding DefaultEncoding { get; set; }

        /// <summary>
        /// Gets the size of the thread pool for the current context.
        /// </summary>
        public int ThreadPoolSize
        {
            get { return _contextProxy.ThreadPoolSize; }
        }

        /// <summary>
        /// Create a <see cref="ZmqContext"/> instance.
        /// </summary>
        /// <returns>A <see cref="ZmqContext"/> instance with the default thread pool size (1).</returns>
        public static ZmqContext Create()
        {
            return Create(DefaultThreadPoolSize);
        }

        /// <summary>
        /// Create a <see cref="ZmqContext"/> instance.
        /// </summary>
        /// <param name="threadPoolSize">Number of threads to use in the ZMQ thread pool.</param>
        /// <returns>A <see cref="ZmqContext"/> instance with the specified thread pool size.</returns>
        public static ZmqContext Create(int threadPoolSize)
        {
            if (threadPoolSize < 0)
            {
                throw new ArgumentOutOfRangeException("threadPoolSize", threadPoolSize, "Thread pool size must be non-negative.");
            }

            var contextProxy = new ContextProxy(threadPoolSize);
            contextProxy.Initialize();

            return new ZmqContext(contextProxy);
        }

        /// <summary>
        /// Create a socket with the current context and the specified socket type.
        /// </summary>
        /// <param name="socketType">A <see cref="SocketType"/> value for the socket.</param>
        /// <returns>A <see cref="ZmqSocket"/> instance with the current context and the specified socket type.</returns>
        public ZmqSocket CreateSocket(SocketType socketType)
        {
            return CreateSocket(sp => new ZmqSocket(sp, socketType), socketType);
        }

        /// <summary>
        /// Terminate the ZeroMQ context.
        /// </summary>
        /// <remarks>
        /// Context termination is performed in the following steps:
        /// <ul>
        ///   <li>
        ///     Any blocking operations currently in progress on sockets open within context shall return immediately
        ///     with an error code of ETERM. With the exception of <see cref="ZmqSocket.Close"/>, any further operations
        ///     on sockets open within the context shall fail with a <see cref="ZmqSocketException"/>.
        ///   </li>
        ///   <li>
        ///     After interrupting all blocking calls, <see cref="Terminate"/> shall block until the following conditions
        ///     are met:
        ///     <ul>
        ///       <li>
        ///         All sockets open within the context have been closed with <see cref="ZmqSocket.Close"/>.
        ///       </li>
        ///       <li>
        ///         For each socket within the context, all messages sent by the application  have either been
        ///         physically transferred to a network peer, or the socket's linger period set with the
        ///         <see cref="ZmqSocket.Linger"/> socket option has expired.
        ///       </li>
        ///     </ul>
        ///   </li>
        /// </ul>
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ZmqContext"/> has already been disposed.</exception>
        /// <exception cref="ZmqException">An error occurred creating the socket.</exception>
        public void Terminate()
        {
            EnsureNotDisposed();

            _contextProxy.Terminate();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ZmqContext"/> class.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ZmqContext"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _contextProxy.Dispose();
                }
            }

            _disposed = true;
        }

        private TSocket CreateSocket<TSocket>(Func<SocketProxy, TSocket> constructor, SocketType socketType)
        {
            EnsureNotDisposed();

            return constructor(new SocketProxy(_contextProxy.CreateSocket((int)socketType)));
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
