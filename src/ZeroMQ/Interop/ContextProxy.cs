namespace ZeroMQ.Interop
{
    using System;

    internal class ContextProxy : IDisposable
    {
        private bool _disposed;

        public ContextProxy(int threadPoolSize)
        {
            ThreadPoolSize = threadPoolSize;
        }

        ~ContextProxy()
        {
            Dispose(false);
        }

        public IntPtr ContextHandle { get; private set; }

        public int ThreadPoolSize { get; private set; }

        public void Initialize()
        {
            ContextHandle = LibZmq.zmq_init(ThreadPoolSize);

            if (ContextHandle == IntPtr.Zero)
            {
                throw ErrorProxy.GetLastError();
            }
        }

        public IntPtr CreateSocket(int socketType)
        {
            IntPtr socket = LibZmq.zmq_socket(ContextHandle, socketType);

            if (socket == IntPtr.Zero)
            {
                throw ErrorProxy.GetLastError();
            }

            return socket;
        }

        public void Terminate()
        {
            if (ContextHandle == IntPtr.Zero)
            {
                return;
            }

            while (LibZmq.zmq_term(ContextHandle) != 0)
            {
                int errorCode = ErrorProxy.GetErrorCode();

                // If zmq_term fails, valid return codes are EFAULT or EINTR. If EINTR is set, termination
                // was interrupted by a signal and may be safely retried.
                if (errorCode == ErrorCode.EFAULT)
                {
                    // This indicates an invalid context was passed in. There's nothing we can do about it here.
                    // It's arguably not a fatal error, so throwing an exception would be bad seeing as this may
                    // run inside a finalizer.
                    break;
                }
            }

            ContextHandle = IntPtr.Zero;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                Terminate();
            }

            _disposed = true;
        }
    }
}
