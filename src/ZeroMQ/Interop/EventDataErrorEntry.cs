namespace ZeroMQ.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct EventDataErrorEntry
    {
        private IntPtr addr;
        public int ErrorCode;
        public string Address
        {
            get
            {
                return Marshal.PtrToStringAnsi(this.addr);
            }
        }
    }
}