namespace ZeroMQ.Interop
{
    using System;
    using System.Runtime.InteropServices;

    internal class DisposableIntPtr : IDisposable
    {
        public DisposableIntPtr(int size)
        {
            Ptr = Marshal.AllocHGlobal(size);
        }

        ~DisposableIntPtr()
        {
            Dispose(false);
        }

        public IntPtr Ptr { get; private set; }

        public static implicit operator IntPtr(DisposableIntPtr disposablePtr)
        {
            return disposablePtr.Ptr;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
            }
        }
    }
}
