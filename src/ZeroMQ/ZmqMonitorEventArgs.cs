namespace ZeroMQ
{
    using System;

    /// <summary>
    /// A base class for the all ZmqMonitor events.
    /// </summary>
    public class ZmqMonitorEventArgs : EventArgs
    {
        public ZmqMonitorEventArgs(ZmqSocket socket, string address)
        {
            this.Socket = socket;
            this.Address = address;
        }

        /// <summary>
        /// Gets socket that triggered the event.
        /// </summary>
        public ZmqSocket Socket { get; private set; }

        /// <summary>
        /// Gets peer address.
        /// </summary>
        public string Address { get; private set; }
    }
}