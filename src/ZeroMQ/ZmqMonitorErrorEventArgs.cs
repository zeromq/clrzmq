namespace ZeroMQ
{
    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.ConnectDelayed"/>, <see cref="ZmqMonitor.AcceptFailed"/>,
    /// <see cref="ZmqMonitorErrorEventArgs"/> and <see cref="ZmqMonitor.BindFailed"/> events.
    /// </summary>
    public class ZmqMonitorErrorEventArgs : ZmqMonitorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZmqMonitorErrorEventArgs"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="ZmqSocket"/> that triggered the event.</param>
        /// <param name="address">The connected peer's address.</param>
        /// <param name="errorCode">The error code representing the reason for failure.</param>
        public ZmqMonitorErrorEventArgs(ZmqSocket socket, string address, int errorCode)
            : base(socket, address)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets error code number.
        /// </summary>
        public int ErrorCode { get; private set; }
    }
}