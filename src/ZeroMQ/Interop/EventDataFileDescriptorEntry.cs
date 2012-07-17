namespace ZeroMQ.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct EventDataFileDescriptorEntry
    {
        private IntPtr addr;
#if UNIX
        public int FileDescriptor;
#else
        public IntPtr FileDescriptor;
#endif
        public string Address
        {
            get
            {
                return Marshal.PtrToStringAnsi(this.addr);
            }
        }
    }
}