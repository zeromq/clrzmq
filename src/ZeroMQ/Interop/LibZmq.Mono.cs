#if MONO

namespace ZeroMQ.Interop
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    internal static class LibZmq
    {
        public const string LibraryName = "libzmq";

        // From zmq.h:
        // typedef struct {unsigned char _ [32];} zmq_msg_t;
        public const int ZmqMsgTSize = 32;

        public static readonly int MajorVersion;
        public static readonly int MinorVersion;
        public static readonly int PatchVersion;

        public static long PollTimeoutRatio;

        static LibZmq()
        {
            AssignCurrentVersion(out MajorVersion, out MinorVersion, out PatchVersion);

            if (MajorVersion >= 3)
            {
                zmq_recvmsg = zmq_recvmsg_v3;
                zmq_sendmsg = zmq_sendmsg_v3;

                zmq_getmsgopt = zmq_getmsgopt_v3;
                zmq_msg_init_data = zmq_msg_init_data_v3;
                zmq_msg_move = zmq_msg_move_v3;

                PollTimeoutRatio = 1;
            }
            else if (MajorVersion == 2)
            {
                zmq_recvmsg = zmq_recvmsg_v2;
                zmq_sendmsg = zmq_sendmsg_v2;

                zmq_getmsgopt = (message, option, optval, optvallen) => { throw new ZmqVersionException(MajorVersion, MinorVersion, 3, 1); };
                zmq_msg_init_data = (msg, data, size, ffn, hint) => { throw new ZmqVersionException(MajorVersion, MinorVersion, 3, 1); };
                zmq_msg_move = (destmsg, srcmsg) => { throw new ZmqVersionException(MajorVersion, MinorVersion, 3, 1); };

                PollTimeoutRatio = 1000;
            }
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
        public delegate int ZmqGetMsgOptProc(IntPtr message, int option, IntPtr optval, IntPtr optvallen);
        public static readonly ZmqGetMsgOptProc zmq_getmsgopt;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqRecvMsgProc(IntPtr socket, IntPtr msg, int flags);
        public static readonly ZmqRecvMsgProc zmq_recvmsg;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqSendMsgProc(IntPtr socket, IntPtr msg, int flags);
        public static readonly ZmqSendMsgProc zmq_sendmsg;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgInitDataProc(IntPtr msg, IntPtr data, int size, FreeMessageDataCallback ffn, IntPtr hint);
        public static readonly ZmqMsgInitDataProc zmq_msg_init_data;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgMoveProc(IntPtr destmsg, IntPtr srcmsg);
        public static readonly ZmqMsgMoveProc zmq_msg_move;

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_init(int io_threads);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_term(IntPtr context);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_close(IntPtr socket);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_setsockopt(IntPtr socket, int option, IntPtr optval, int optvallen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_getsockopt(IntPtr socket, int option, IntPtr optval, IntPtr optvallen);

        [DllImport(LibraryName, EntryPoint = "zmq_getmsgopt", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_getmsgopt_v3(IntPtr message, int option, IntPtr optval, IntPtr optvallen);

        [DllImport(LibraryName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_bind(IntPtr socket, string addr);

        [DllImport(LibraryName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_connect(IntPtr socket, string addr);

        [DllImport(LibraryName, EntryPoint = "zmq_recv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_recvmsg_v2(IntPtr socket, IntPtr msg, int flags);

        [DllImport(LibraryName, EntryPoint = "zmq_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_sendmsg_v2(IntPtr socket, IntPtr msg, int flags);

        [DllImport(LibraryName, EntryPoint = "zmq_recvmsg", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_recvmsg_v3(IntPtr socket, IntPtr msg, int flags);

        [DllImport(LibraryName, EntryPoint = "zmq_sendmsg", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_sendmsg_v3(IntPtr socket, IntPtr msg, int flags);

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

        [DllImport(LibraryName, EntryPoint = "zmq_msg_init_data", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_init_data_v3(IntPtr msg, IntPtr data, int size, FreeMessageDataCallback ffn, IntPtr hint);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_init_size(IntPtr msg, int size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_size(IntPtr msg);

        [DllImport(LibraryName, EntryPoint = "zmq_msg_move", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_move_v3(IntPtr destmsg, IntPtr srcmsg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_errno();

        [DllImport(LibraryName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_strerror(int errnum);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_device(int device, IntPtr inSocket, IntPtr outSocket);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void zmq_version(IntPtr major, IntPtr minor, IntPtr patch);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_poll([In, Out] PollItem[] items, int numItems, long timeout);
    }
}

#endif