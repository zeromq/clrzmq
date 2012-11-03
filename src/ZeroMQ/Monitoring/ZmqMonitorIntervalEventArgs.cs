namespace ZeroMQ.Monitoring
{
    using Interop;

    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.ConnectRetried"/> event.
    /// </summary>
    public class ZmqMonitorIntervalEventArgs : ZmqMonitorEventArgs
    {
        internal ZmqMonitorIntervalEventArgs(ZmqMonitor monitor, MonitorEventData data)
            : base(monitor, data.Address)
        {
            this.Interval = data.Value;
        }

        /// <summary>
        /// Gets the computed reconnect interval.
        /// </summary>
        public int Interval { get; private set; }
    }
}