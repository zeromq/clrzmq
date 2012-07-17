namespace ZeroMQ
{
    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.ConnectDelayed"/>, <see cref="ZmqMonitor.AcceptFailed"/>, <see cref="ZmqMonitorErrorEventArgs"/> and <see cref="ZmqMonitor.BindFailed"/> events.
    /// </summary>
    public class ZmqMonitorErrorEventArgs : ZmqMonitorEventArgs
    {
        public ZmqMonitorErrorEventArgs(ZmqSocket socket, string address, int errorCode)
            : base(socket, address)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets error code number.
        /// </summary>
        public int ErrorCode { get; private set; }
    }
}