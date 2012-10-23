namespace ZeroMQ.Monitoring
{
    using Interop;

    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.ConnectDelayed"/>, <see cref="ZmqMonitor.AcceptFailed"/>,
    /// <see cref="ZmqMonitorErrorEventArgs"/> and <see cref="ZmqMonitor.BindFailed"/> events.
    /// </summary>
    public class ZmqMonitorErrorEventArgs : ZmqMonitorEventArgs
    {
        internal ZmqMonitorErrorEventArgs(ZmqMonitor monitor, MonitorEventData data)
            : base(monitor, data.Address)
        {
            this.ErrorCode = data.Value;
        }

        /// <summary>
        /// Gets error code number.
        /// </summary>
        public int ErrorCode { get; private set; }
    }
}