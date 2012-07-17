namespace ZeroMQ
{
    using System;
    using System.Collections.Generic;
    using ZeroMQ.Interop;

    /// <summary>
    /// A socket monitoring object.
    /// </summary>
    /// <remarks>
    /// CAUTION: ZmqMonitor is intended for monitoring infrastructure /
    /// operations concerns only - NOT BUSINESS LOGIC. An event is a representation of
    /// something that happened - you cannot change the past, but only react to them.
    /// The implementation is also only concerned with a single session. No state of
    /// peers, other sessions etc. are tracked - this will only pollute internals and
    /// is the responsibility of application authors to either implement or correlate
    /// in another datastore. Monitor events are exceptional conditions and are thus
    /// not directly in the messaging critical path. However, still be careful with
    /// what you're doing in the callback function as excess time spent in the handler
    /// will block the socket's application thread.
    /// </remarks>
    public class ZmqMonitor : IDisposable
    {
        private readonly ZmqContext context;

        private readonly Dictionary<MonitorEvent, Action<ZmqSocket, EventData>> eventHandler = new Dictionary<MonitorEvent, Action<ZmqSocket, EventData>>();

        private bool _disposed;

        internal ZmqMonitor(ZmqContext context)
        {
            this.context = context;

            this.eventHandler.Add(MonitorEvent.CONNECTED, (socket, data) => this.InvokeEvent(this.Connected, () => this.CreateEventArgs(socket, data.Conencted)));
            this.eventHandler.Add(MonitorEvent.CONNECT_DELAYED, (socket, data) => this.InvokeEvent(this.ConnectDelayed, () => this.CreateEventArgs(socket, data.ConenctDelayed)));
            this.eventHandler.Add(MonitorEvent.CONNECT_RETRIED, (socket, data) => this.InvokeEvent(this.ConnectRetried, () => this.CreateEventArgs(socket, data.ConenctRetried)));

            this.eventHandler.Add(MonitorEvent.LISTENING, (socket, data) => this.InvokeEvent(this.Listening, () => this.CreateEventArgs(socket, data.Listening)));
            this.eventHandler.Add(MonitorEvent.BIND_FAILED, (socket, data) => this.InvokeEvent(this.BindFailed, () => this.CreateEventArgs(socket, data.BindFailed)));

            this.eventHandler.Add(MonitorEvent.ACCEPTED, (socket, data) => this.InvokeEvent(this.Accepted, () => this.CreateEventArgs(socket, data.Accepted)));
            this.eventHandler.Add(MonitorEvent.ACCEPT_FAILED, (socket, data) => this.InvokeEvent(this.AcceptFailed, () => this.CreateEventArgs(socket, data.AcceptFailed)));

            this.eventHandler.Add(MonitorEvent.CLOSED, (socket, data) => this.InvokeEvent(this.Closed, () => this.CreateEventArgs(socket, data.Closed)));
            this.eventHandler.Add(MonitorEvent.CLOSE_FAILED, (socket, data) => this.InvokeEvent(this.CloseFailed, () => this.CreateEventArgs(socket, data.CloseFailed)));
            this.eventHandler.Add(MonitorEvent.DISCONNECTED, (socket, data) => this.InvokeEvent(this.Disconnected, () => this.CreateEventArgs(socket, data.Disconnected)));
        }

        /// <summary>
        /// Occurs when a new connection established.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Connected;

        /// <summary>
        /// Occurs when a synchronous connect failed, and it's being polled.
        /// </summary>
        public event EventHandler<ZmqMonitorErrorEventArgs> ConnectDelayed;

        /// <summary>
        /// Occurs when a asynchronous connect / reconnection attempt.
        /// </summary>
        public event EventHandler<ZmqMonitorIntervalEventArgs> ConnectRetried;

        /// <summary>
        /// Occurs when a socket bound to an address, ready to accept connections.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Listening;

        /// <summary>
        /// Occurs when a socket could not bind to an address.
        /// </summary>
        public event EventHandler<ZmqMonitorErrorEventArgs> BindFailed;

        /// <summary>
        /// Occurs when connection accepted to bound interface.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Accepted;

        /// <summary>
        /// Occurs when a socket could not accept client connection.
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
        /// Occurs when a session is broken.
        /// </summary>
        public event EventHandler<ZmqMonitorFileDescriptorEventArgs> Disconnected;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void OnMonitor(ZmqSocket socket, int ev, ref EventData data)
        {
            this.eventHandler[(MonitorEvent)ev](socket, data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.context.MonitorDisposed(this);
                }
            }

            _disposed = true;
        }

        private ZmqMonitorFileDescriptorEventArgs CreateEventArgs(ZmqSocket socket, EventDataFileDescriptorEntry data)
        {
            return new ZmqMonitorFileDescriptorEventArgs(socket, data.Address, data.FileDescriptor);
        }

        private ZmqMonitorErrorEventArgs CreateEventArgs(ZmqSocket socket, EventDataErrorEntry data)
        {
            return new ZmqMonitorErrorEventArgs(socket, data.Address, data.ErrorCode);
        }

        private ZmqMonitorIntervalEventArgs CreateEventArgs(ZmqSocket socket, EventDataIntervalEntry data)
        {
            return new ZmqMonitorIntervalEventArgs(socket, data.Address, data.Interval);
        }

        private void InvokeEvent<T>(EventHandler<T> handler, Func<T> create) where T : EventArgs
        {
            if (handler != null)
            {
                handler(this.context, create());
            }
        }
    }
}