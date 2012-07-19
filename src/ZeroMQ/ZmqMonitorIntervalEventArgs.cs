namespace ZeroMQ
{
    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.ConnectRetried"/> event.
    /// </summary>
    public class ZmqMonitorIntervalEventArgs : ZmqMonitorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZmqMonitorIntervalEventArgs"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="ZmqSocket"/> that triggered the event.</param>
        /// <param name="address">The peer address.</param>
        /// <param name="interval">The computed reconnect interval</param>
        public ZmqMonitorIntervalEventArgs(ZmqSocket socket, string address, int interval)
            : base(socket, address)
        {
            Interval = interval;
        }

        /// <summary>
        /// Gets the computed reconnect interval.
        /// </summary>
        public int Interval { get; private set; }
    }
}