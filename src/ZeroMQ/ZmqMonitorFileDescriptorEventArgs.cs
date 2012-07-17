namespace ZeroMQ
{
    using System;

    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.Connected"/>, <see cref="ZmqMonitor.Listening"/>, <see cref="ZmqMonitor.Accepted"/>, <see cref="ZmqMonitor.Closed"/> and <see cref="ZmqMonitor.Disconnected"/> events.
    /// </summary>
    public class ZmqMonitorFileDescriptorEventArgs : ZmqMonitorEventArgs
    {
#if UNIX
        public ZmqMonitorFileDescriptorEventArgs(ZmqSocket socket, string address, int fileDescriptor)
#else
        public ZmqMonitorFileDescriptorEventArgs(ZmqSocket socket, string address, IntPtr fileDescriptor)
#endif
            : base(socket, address)
        {
            this.FileDescriptor = fileDescriptor;
        }

        /// <summary>
        /// Gets socket descriptor.
        /// </summary>
#if UNIX
        public int FileDescriptor { get; private set; }
#else
        public IntPtr FileDescriptor { get; private set; }
#endif
    }
}