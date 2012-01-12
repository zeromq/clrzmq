namespace ZeroMQ.Interop
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Mirrors native API.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Mirrors native API.")]
    internal static class LibZmq
    {
        public const string LibraryName = "libzmq";

        // From zmq.h:
        // typedef struct {unsigned char _ [32];} zmq_msg_t;
        public const int ZmqMsgTSize = 32;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeMessageDataCallback(IntPtr data, IntPtr hint);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_init(int io_threads);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_term(IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_setsockopt(IntPtr socket, int option, IntPtr optval, int optvallen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_getsockopt(IntPtr socket, int option, IntPtr optval, IntPtr optvallen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_getmsgopt(IntPtr message, int option, IntPtr optval, IntPtr optvallen);

        [DllImport(LibraryName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_bind(IntPtr socket, string addr);

        [DllImport(LibraryName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_connect(IntPtr socket, string addr);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_close(IntPtr socket);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_recvmsg(IntPtr socket, IntPtr msg, int flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_sendmsg(IntPtr socket, IntPtr msg, int flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_socket(IntPtr context, int type);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_close(IntPtr msg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_copy(IntPtr destmsg, IntPtr srcmsg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_msg_data(IntPtr msg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_init(IntPtr msg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_init_size(IntPtr msg, int size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_init_data(IntPtr msg, IntPtr data, int size, FreeMessageDataCallback ffn, IntPtr hint);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_move(IntPtr destmsg, IntPtr srcmsg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_size(IntPtr msg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_errno();

        [DllImport(LibraryName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_strerror(int errnum);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void zmq_version(IntPtr major, IntPtr minor, IntPtr patch);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_poll([In] [Out] PollItem[] items, int numItems, long timeoutMsec);
    }
// ReSharper restore InconsistentNaming
}