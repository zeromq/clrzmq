namespace ZeroMQ
{
    using System;

    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.Connected"/>, <see cref="ZmqMonitor.Listening"/>, <see cref="ZmqMonitor.Accepted"/>, <see cref="ZmqMonitor.Closed"/> and <see cref="ZmqMonitor.Disconnected"/> events.
    /// </summary>
    public class ZmqMonitorFileDescriptorEventArgs : ZmqMonitorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZmqMonitorFileDescriptorEventArgs"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="ZmqSocket"/> that triggered the event.</param>
        /// <param name="address">The peer address.</param>
        /// <param name="fileDescriptor">The socket descriptor associated with the event.</param>
#if UNIX
        public ZmqMonitorFileDescriptorEventArgs(ZmqSocket socket, string address, int fileDescriptor)
#else
        public ZmqMonitorFileDescriptorEventArgs(ZmqSocket socket, string address, IntPtr fileDescriptor)
#endif
            : base(socket, address)
        {
            FileDescriptor = fileDescriptor;
        }

        /// <summary>
        /// Gets the socket descriptor.
        /// </summary>
#if UNIX
        public int FileDescriptor { get; private set; }
#else
        public IntPtr FileDescriptor { get; private set; }
#endif
    }
}