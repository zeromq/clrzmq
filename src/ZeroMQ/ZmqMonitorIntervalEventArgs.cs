namespace ZeroMQ
{
    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.ConnectRetried"/> event.
    /// </summary>
    public class ZmqMonitorIntervalEventArgs : ZmqMonitorEventArgs
    {
        public ZmqMonitorIntervalEventArgs(ZmqSocket socket, string address, int interval)
            : base(socket, address)
        {
            this.Interval = interval;
        }

        /// <summary>
        /// Gets computed reconnect interval.
        /// </summary>
        public int Interval { get; private set; }
    }
}