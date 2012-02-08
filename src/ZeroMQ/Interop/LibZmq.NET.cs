#if !MONO

namespace ZeroMQ.Interop
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Compatibility with native headers.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Compatibility with native headers.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Compatibility with native headers.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Compatibility with native headers.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
    internal static class LibZmq
    {
        public const string LibraryName = "libzmq";

        // From zmq.h:
        // typedef struct {unsigned char _ [32];} zmq_msg_t;
        public const int ZmqMsgTSize = 32;

        public static readonly int MajorVersion;
        public static readonly int MinorVersion;
        public static readonly int PatchVersion;

        private static readonly UnmanagedLibrary NativeLib;

        public static readonly long PollTimeoutRatio;

        static LibZmq()
        {
            NativeLib = new UnmanagedLibrary(LibraryName);

            AssignCommonDelegates();
            AssignCurrentVersion(out MajorVersion, out MinorVersion, out PatchVersion);

            if (MajorVersion >= 3)
            {
                zmq_getmsgopt = NativeLib.GetUnmanagedFunction<ZmqGetMsgOptProc>("zmq_getmsgopt");
                zmq_recvmsg = NativeLib.GetUnmanagedFunction<ZmqRecvMsgProc>("zmq_recvmsg");
                zmq_sendmsg = NativeLib.GetUnmanagedFunction<ZmqSendMsgProc>("zmq_sendmsg");
                zmq_msg_init_data = NativeLib.GetUnmanagedFunction<ZmqMsgInitDataProc>("zmq_msg_init_data");
                zmq_msg_move = NativeLib.GetUnmanagedFunction<ZmqMsgMoveProc>("zmq_msg_move");

                PollTimeoutRatio = 1;
            }
            else if (MajorVersion == 2)
            {
                zmq_recvmsg = NativeLib.GetUnmanagedFunction<ZmqRecvMsgProc>("zmq_recv");
                zmq_sendmsg = NativeLib.GetUnmanagedFunction<ZmqSendMsgProc>("zmq_send");

                PollTimeoutRatio = 1000;
            }
        }

        private static void AssignCommonDelegates()
        {
            zmq_init = NativeLib.GetUnmanagedFunction<ZmqInitProc>("zmq_init");
            zmq_term = NativeLib.GetUnmanagedFunction<ZmqTermProc>("zmq_term");
            zmq_close = NativeLib.GetUnmanagedFunction<ZmqCloseProc>("zmq_close");
            zmq_setsockopt = NativeLib.GetUnmanagedFunction<ZmqSetSockOptProc>("zmq_setsockopt");
            zmq_getsockopt = NativeLib.GetUnmanagedFunction<ZmqGetSockOptProc>("zmq_getsockopt");
            zmq_bind = NativeLib.GetUnmanagedFunction<ZmqBindProc>("zmq_bind");
            zmq_connect = NativeLib.GetUnmanagedFunction<ZmqConnectProc>("zmq_connect");
            zmq_socket = NativeLib.GetUnmanagedFunction<ZmqSocketProc>("zmq_socket");
            zmq_msg_close = NativeLib.GetUnmanagedFunction<ZmqMsgCloseProc>("zmq_msg_close");
            zmq_msg_copy = NativeLib.GetUnmanagedFunction<ZmqMsgCopyProc>("zmq_msg_copy");
            zmq_msg_data = NativeLib.GetUnmanagedFunction<ZmqMsgDataProc>("zmq_msg_data");
            zmq_msg_init = NativeLib.GetUnmanagedFunction<ZmqMsgInitProc>("zmq_msg_init");
            zmq_msg_init_size = NativeLib.GetUnmanagedFunction<ZmqMsgInitSizeProc>("zmq_msg_init_size");
            zmq_msg_size = NativeLib.GetUnmanagedFunction<ZmqMsgSizeProc>("zmq_msg_size");
            zmq_errno = NativeLib.GetUnmanagedFunction<ZmqErrnoProc>("zmq_errno");
            zmq_strerror = NativeLib.GetUnmanagedFunction<ZmqStrErrorProc>("zmq_strerror");
            zmq_version = NativeLib.GetUnmanagedFunction<ZmqVersionProc>("zmq_version");
            zmq_poll = NativeLib.GetUnmanagedFunction<ZmqPollProc>("zmq_poll");
        }

        private static void AssignCurrentVersion(out int majorVersion, out int minorVersion, out int patchVersion)
        {
            int sizeofInt32 = Marshal.SizeOf(typeof(int));

            IntPtr majorPointer = Marshal.AllocHGlobal(sizeofInt32);
            IntPtr minorPointer = Marshal.AllocHGlobal(sizeofInt32);
            IntPtr patchPointer = Marshal.AllocHGlobal(sizeofInt32);

            zmq_version(majorPointer, minorPointer, patchPointer);

            majorVersion = Marshal.ReadInt32(majorPointer);
            minorVersion = Marshal.ReadInt32(minorPointer);
            patchVersion = Marshal.ReadInt32(patchPointer);

            Marshal.FreeHGlobal(majorPointer);
            Marshal.FreeHGlobal(minorPointer);
            Marshal.FreeHGlobal(patchPointer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeMessageDataCallback(IntPtr data, IntPtr hint);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZmqInitProc(int io_threads);
        public static ZmqInitProc zmq_init;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqTermProc(IntPtr context);
        public static ZmqTermProc zmq_term;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqSetSockOptProc(IntPtr socket, int option, IntPtr optval, int optvallen);
        public static ZmqSetSockOptProc zmq_setsockopt;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqGetSockOptProc(IntPtr socket, int option, IntPtr optval, IntPtr optvallen);
        public static ZmqGetSockOptProc zmq_getsockopt;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqGetMsgOptProc(IntPtr message, int option, IntPtr optval, IntPtr optvallen);
        public static ZmqGetMsgOptProc zmq_getmsgopt;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int ZmqBindProc(IntPtr socket, string addr);
        public static ZmqBindProc zmq_bind;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int ZmqConnectProc(IntPtr socket, string addr);
        public static ZmqConnectProc zmq_connect;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqCloseProc(IntPtr socket);
        public static ZmqCloseProc zmq_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqRecvMsgProc(IntPtr socket, IntPtr msg, int flags);
        public static ZmqRecvMsgProc zmq_recvmsg;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqSendMsgProc(IntPtr socket, IntPtr msg, int flags);
        public static ZmqSendMsgProc zmq_sendmsg;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZmqSocketProc(IntPtr context, int type);
        public static ZmqSocketProc zmq_socket;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgCloseProc(IntPtr msg);
        public static ZmqMsgCloseProc zmq_msg_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgCopyProc(IntPtr destmsg, IntPtr srcmsg);
        public static ZmqMsgCopyProc zmq_msg_copy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZmqMsgDataProc(IntPtr msg);
        public static ZmqMsgDataProc zmq_msg_data;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgInitProc(IntPtr msg);
        public static ZmqMsgInitProc zmq_msg_init;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgInitSizeProc(IntPtr msg, int size);
        public static ZmqMsgInitSizeProc zmq_msg_init_size;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgInitDataProc(IntPtr msg, IntPtr data, int size, FreeMessageDataCallback ffn, IntPtr hint);
        public static ZmqMsgInitDataProc zmq_msg_init_data;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgMoveProc(IntPtr destmsg, IntPtr srcmsg);
        public static ZmqMsgMoveProc zmq_msg_move;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgSizeProc(IntPtr msg);
        public static ZmqMsgSizeProc zmq_msg_size;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqErrnoProc();
        public static ZmqErrnoProc zmq_errno;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr ZmqStrErrorProc(int errnum);
        public static ZmqStrErrorProc zmq_strerror;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ZmqVersionProc(IntPtr major, IntPtr minor, IntPtr patch);
        public static ZmqVersionProc zmq_version;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqPollProc([In] [Out] PollItem[] items, int numItems, long timeoutMsec);
        public static ZmqPollProc zmq_poll;
    }
    // ReSharper restore InconsistentNaming
}

#endif