namespace ZeroMQ.Interop
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct EventDataIntervalEntry
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "This is an interop struct with a sequential layout.")]
        private IntPtr addr;

        public int Interval;

        public string Address
        {
            get { return Marshal.PtrToStringAnsi(addr); }
        }
        // ReSharper restore FieldCanBeMadeReadOnly.Local
    }
}