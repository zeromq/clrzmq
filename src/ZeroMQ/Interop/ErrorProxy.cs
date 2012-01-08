namespace ZeroMQ.Interop
{
    using System.Runtime.InteropServices;

    internal static class ErrorProxy
    {
        public static bool ShouldTryAgain
        {
            get { return GetErrorCode() == ErrorCode.EAGAIN; }
        }

        public static bool ContextWasTerminated
        {
            get { return GetErrorCode() == ErrorCode.ETERM; }
        }

        public static bool ThreadWasInterrupted
        {
            get { return GetErrorCode() == ErrorCode.EINTR; }
        }

        public static int GetErrorCode()
        {
            return LibZmq.zmq_errno();
        }

        public static ZmqException GetLastError()
        {
            int errorCode = GetErrorCode();

            return new ZmqException(errorCode, GetErrorMessage(errorCode));
        }

        public static ZmqSocketException GetLastSocketError()
        {
            ZmqException lastError = GetLastError();

            return new ZmqSocketException(lastError.ErrorCode, lastError.Message);
        }

        private static string GetErrorMessage(int errorCode)
        {
            return Marshal.PtrToStringAuto(LibZmq.zmq_strerror(errorCode));
        }
    }
}
