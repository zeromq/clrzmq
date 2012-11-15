namespace ZeroMQ.Interop
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorEventData
    {
        public int Event;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Address;

        public int Value;
    }
}