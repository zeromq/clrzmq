namespace ZeroMQ.Monitoring
{
    using System;
    using Interop;

    /// <summary>
    /// Provides data for <see cref="ZmqMonitor.Connected"/>, <see cref="ZmqMonitor.Listening"/>, <see cref="ZmqMonitor.Accepted"/>, <see cref="ZmqMonitor.Closed"/> and <see cref="ZmqMonitor.Disconnected"/> events.
    /// </summary>
    public class ZmqMonitorFileDescriptorEventArgs : ZmqMonitorEventArgs
    {
        internal ZmqMonitorFileDescriptorEventArgs(ZmqMonitor monitor, MonitorEventData data)
            : base(monitor, data.Address)
        {
#if UNIX
            this.FileDescriptor = data.Value;
#else
            this.FileDescriptor = new IntPtr(data.Value);
#endif
        }

        /// <summary>
        /// Gets the monitor descriptor.
        /// </summary>
#if UNIX
        public int FileDescriptor { get; private set; }
#else
        public IntPtr FileDescriptor { get; private set; }
#endif
    }
}