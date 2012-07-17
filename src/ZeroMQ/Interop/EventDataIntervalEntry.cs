namespace ZeroMQ.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct EventDataIntervalEntry
    {
        private IntPtr addr;
        public int Interval;
        public string Address
        {
            get
            {
                return Marshal.PtrToStringAnsi(this.addr);
            }
        }
    }
}