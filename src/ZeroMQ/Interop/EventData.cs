namespace ZeroMQ.Interop
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    internal struct EventData
    {
        [FieldOffset(0)]
        public EventDataFileDescriptorEntry Connected;
        [FieldOffset(0)]
        public EventDataErrorEntry ConnectDelayed;
        [FieldOffset(0)]
        public EventDataIntervalEntry ConnectRetried;
        [FieldOffset(0)]
        public EventDataFileDescriptorEntry Listening;
        [FieldOffset(0)]
        public EventDataErrorEntry BindFailed;
        [FieldOffset(0)]
        public EventDataFileDescriptorEntry Accepted;
        [FieldOffset(0)]
        public EventDataErrorEntry AcceptFailed;
        [FieldOffset(0)]
        public EventDataFileDescriptorEntry Closed;
        [FieldOffset(0)]
        public EventDataErrorEntry CloseFailed;
        [FieldOffset(0)]
        public EventDataFileDescriptorEntry Disconnected;
    }
}