namespace ZeroMQ
{
    using System;
    using System.Collections.Generic;
    using ZeroMQ.Interop;

    /// <summary>
    /// A socket monitoring object.
    /// </summary>
    /// <remarks>
    /// CAUTION: <see cref="ZmqMonitor"/> is intended for monitoring infrastructure /
    /// operations concerns only - NOT BUSINESS LOGIC. An event is a representation of
    /// something that happened - you cannot change the past, but only react to them.
    /// The implementation is also only concerned with a single session. No state of
    /// peers, other sessions etc. are tracked - this will only pollute internals and
    /// is the responsibility of application authors to either implement or correlate
    /// in another datastore. Monitor events are exceptional conditions and are thus
    /// not directly in the messaging critical path. However, still be careful with
    /// what you're doing in the event handlers as excess time spent in the handler
    /// will block the socket's application thread.
    /// </remarks>
    public class ZmqMonitor : IDisposable
    {
        private readonly ZmqContext _context;
        private readonly Dictionary<MonitorEvent, Action<ZmqSocket, EventData>> _eventHandler;

        private bool _disposed;

        internal ZmqMonitor(ZmqContext context)
        {
            _context = context;
            _eventHandler = new Dictionary<MonitorEvent, Action<ZmqSocket, EventData>>
            {
                { MonitorEvent.CONNECTED,       (socket, data) => InvokeEvent(Connected, () => CreateEventArgs(socket, data.Connected)) },
                { MonitorEvent.CONNECT_DELAYED, (socket, data) => InvokeEvent(ConnectDelayed, () => CreateEventArgs(socket, data.ConnectDelayed)) },
                { MonitorEvent.CONNECT_RETRIED, (socket, data) => InvokeEvent(ConnectRetried, () => CreateEventArgs(socket, data.ConnectRetried)) },
                { MonitorEvent.LISTENING,       (socket, data) => InvokeEvent(Listening, () => CreateEventArgs(socket, data.Listening)) },
                { MonitorEvent.BIND_FAILED,     (socket, data) => InvokeEvent(BindFailed, () => CreateEventArgs(socket, data.BindFailed)) },
                { MonitorEvent.ACCEPTED,        (socket, data) => InvokeEvent(Accepted, () => CreateEventArgs(socket, data.Accepted)) },
                { MonitorEvent.ACCEPT_FAILED,   (socket, data) => InvokeEvent(AcceptFailed, () => CreateEventArgs(socket, data.AcceptFailed)) },
                { MonitorEvent.CLOSED,          (socket, data) => InvokeEvent(Closed, () => CreateEventArgs(socket, data.Closed)) },
                { MonitorEvent.CLOSE_FAILED,    (socket, data) => InvokeEvent(CloseFailed, () => CreateEventArgs(socket, data.CloseFailed)) },
                { MonitorEvent.DISCONNECTED,    (socket, data) => InvokeEvent(Disconnected, () => CreateEventArgs(socket, data.Disconnected)) }
            };
        }

        /// <summary>
        /// Occurs when a new connection is established.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Connected;

        /// <summary>
        /// Occurs when a synchronous connection attempt failed, and its completion is being polled for.
        /// </summary>
        public event EventHandler<ZmqMonitorErrorEventArgs> ConnectDelayed;

        /// <summary>
        /// Occurs when an asynchronous connect / reconnection attempt is being handled by a reconnect timer.
        /// </summary>
        public event EventHandler<ZmqMonitorIntervalEventArgs> ConnectRetried;

        /// <summary>
        /// Occurs when a socket is bound to an address and is ready to accept connections.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Listening;

        /// <summary>
        /// Occurs when a socket could not bind to an address.
        /// </summary>
        public event EventHandler<ZmqMonitorErrorEventArgs> BindFailed;

        /// <summary>
        /// Occurs when a connection from a remote peer has been established with a socket's listen address.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Accepted;

        /// <summary>
        /// Occurs when a connection attempt to a socket's bound address fails.
        /// </summary>
        public event EventHandler<ZmqMonitorErrorEventArgs> AcceptFailed;

        /// <summary>
        /// Occurs when a connection was closed.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Closed;

        /// <summary>
        /// Occurs when a connection couldn't be closed.
        /// </summary>
        public event EventHandler<ZmqMonitorErrorEventArgs> CloseFailed;

        /// <summary>
        /// Occurs when the stream engine (tcp and ipc specific) detects a corrupted / broken session.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Disconnected;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ZmqMonitor"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void OnMonitor(ZmqSocket socket, int ev, ref EventData data)
        {
            _eventHandler[(MonitorEvent)ev](socket, data);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ZmqMonitor"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.MonitorDisposed(this);
                }
            }

            _disposed = true;
        }

        private static ZmqMonitorFileDescriptorEventArgs CreateEventArgs(ZmqSocket socket, EventDataFileDescriptorEntry data)
        {
            return new ZmqMonitorFileDescriptorEventArgs(socket, data.Address, data.FileDescriptor);
        }

        private static ZmqMonitorErrorEventArgs CreateEventArgs(ZmqSocket socket, EventDataErrorEntry data)
        {
            return new ZmqMonitorErrorEventArgs(socket, data.Address, data.ErrorCode);
        }

        private static ZmqMonitorIntervalEventArgs CreateEventArgs(ZmqSocket socket, EventDataIntervalEntry data)
        {
            return new ZmqMonitorIntervalEventArgs(socket, data.Address, data.Interval);
        }

        private void InvokeEvent<T>(EventHandler<T> handler, Func<T> create) where T : EventArgs
        {
            if (handler != null)
            {
                handler(_context, create());
            }
        }
    }
}