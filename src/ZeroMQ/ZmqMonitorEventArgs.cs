namespace ZeroMQ
{
    using System;

    /// <summary>
    /// A base class for the all ZmqMonitor events.
    /// </summary>
    public class ZmqMonitorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZmqMonitorEventArgs"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="ZmqSocket"/> that triggered the event.</param>
        /// <param name="address">The peer address.</param>
        public ZmqMonitorEventArgs(ZmqSocket socket, string address)
        {
            Socket = socket;
            Address = address;
        }

        /// <summary>
        /// Gets the socket that triggered the event.
        /// </summary>
        public ZmqSocket Socket { get; private set; }

        /// <summary>
        /// Gets the peer address.
        /// </summary>
        public string Address { get; private set; }
    }
}