namespace ZeroMQ.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PollItem : IEquatable<PollItem>
    {
        public IntPtr Socket;
        public IntPtr FileDescriptor;
        public short Events;
        public short ReadyEvents;

        public PollItem(IntPtr socket, IntPtr fileDescriptor, PollEvents pollEvents)
        {
            if (socket == IntPtr.Zero && fileDescriptor == IntPtr.Zero)
            {
                throw new ArgumentException("One of 'socket' or 'fileDescriptor' must be a valid handle.");
            }

            Socket = socket;
            FileDescriptor = fileDescriptor;
            Events = (short)pollEvents;
            ReadyEvents = 0;
        }

        public bool Equals(PollItem other)
        {
            return Socket.ToInt64() == other.Socket.ToInt64() &&
                   FileDescriptor.ToInt64() == other.FileDescriptor.ToInt64();
        }

        public override int GetHashCode()
        {
            return Socket != IntPtr.Zero ? Socket.GetHashCode() : FileDescriptor.GetHashCode();
        }
    }
}
