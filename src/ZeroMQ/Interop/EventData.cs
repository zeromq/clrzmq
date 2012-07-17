using System;
using System.Runtime.InteropServices;

namespace ZeroMQ.Interop
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    internal struct EventData
    {
        [FieldOffset(0)]
        public EventDataFileDescriptorEntry Conencted;
        [FieldOffset(0)]
        public EventDataErrorEntry ConenctDelayed;
        [FieldOffset(0)]
        public EventDataIntervalEntry ConenctRetried;
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