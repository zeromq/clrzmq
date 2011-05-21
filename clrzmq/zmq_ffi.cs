/*
 
    Copyright (c) 2010 Jeffrey Dik <s450r1@gmail.com>
    Copyright (c) 2010 Martin Sustrik <sustrik@250bpm.com>
    Copyright (c) 2010 Michael Compton <michael.compton@littleedge.co.uk>
     
    This file is part of clrzmq.
     
    clrzmq is free software; you can redistribute it and/or modify it under
    the terms of the Lesser GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.
     
    clrzmq is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    Lesser GNU General Public License for more details.
     
    You should have received a copy of the Lesser GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Runtime.InteropServices;

namespace ZMQ {
    using System.Runtime.ConstrainedExecution;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    internal static class C {
        private static readonly UnmanagedLibrary ZmqLib;

        static C() {
            ZmqLib = new UnmanagedLibrary("libzmq64", "libzmq32", "libzmq");
            AssignDelegates();
        }

        private static void AssignDelegates() {
            zmq_init = ZmqLib.GetUnmanagedFunction<ZmqInitProc>("zmq_init");
            zmq_term = ZmqLib.GetUnmanagedFunction<ZmqTermProc>("zmq_term");
            zmq_close = ZmqLib.GetUnmanagedFunction<ZmqCloseProc>("zmq_close");
            zmq_setsockopt = ZmqLib.GetUnmanagedFunction<ZmqSetSockOptProc>("zmq_setsockopt");
            zmq_getsockopt = ZmqLib.GetUnmanagedFunction<ZmqGetSockOptProc>("zmq_getsockopt");
            zmq_bind = ZmqLib.GetUnmanagedFunction<ZmqBindProc>("zmq_bind");
            zmq_connect = ZmqLib.GetUnmanagedFunction<ZmqConnectProc>("zmq_connect");
            zmq_recv = ZmqLib.GetUnmanagedFunction<ZmqRecvProc>("zmq_recv");
            zmq_send = ZmqLib.GetUnmanagedFunction<ZmqSendProc>("zmq_send");
            zmq_socket = ZmqLib.GetUnmanagedFunction<ZmqSocketProc>("zmq_socket");
            zmq_msg_close = ZmqLib.GetUnmanagedFunction<ZmqMsgCloseProc>("zmq_msg_close");
            zmq_msg_data = ZmqLib.GetUnmanagedFunction<ZmqMsgDataProc>("zmq_msg_data");
            zmq_msg_init = ZmqLib.GetUnmanagedFunction<ZmqMsgInitProc>("zmq_msg_init");
            zmq_msg_init_size = ZmqLib.GetUnmanagedFunction<ZmqMsgInitSizeProc>("zmq_msg_init_size");
            zmq_msg_size = ZmqLib.GetUnmanagedFunction<ZmqMsgSizeProc>("zmq_msg_size");
            zmq_errno = ZmqLib.GetUnmanagedFunction<ZmqErrnoProc>("zmq_errno");
            zmq_strerror = ZmqLib.GetUnmanagedFunction<ZmqStrErrorProc>("zmq_strerror");
            zmq_device = ZmqLib.GetUnmanagedFunction<ZmqDeviceProc>("zmq_device");
            zmq_version = ZmqLib.GetUnmanagedFunction<ZmqVersionProc>("zmq_version");
            zmq_poll = ZmqLib.GetUnmanagedFunction<ZmqPollProc>("zmq_poll");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZmqInitProc(int io_threads);
        public static ZmqInitProc zmq_init;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqTermProc(IntPtr context);
        public static ZmqTermProc zmq_term;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqCloseProc(IntPtr socket);
        public static ZmqCloseProc zmq_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqSetSockOptProc(IntPtr socket, int option, IntPtr optval, int optvallen);
        public static ZmqSetSockOptProc zmq_setsockopt;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqGetSockOptProc(IntPtr socket, int option, IntPtr optval, IntPtr optvallen);
        public static ZmqGetSockOptProc zmq_getsockopt;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int ZmqBindProc(IntPtr socket, string addr);
        public static ZmqBindProc zmq_bind;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int ZmqConnectProc(IntPtr socket, string addr);
        public static ZmqConnectProc zmq_connect;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqRecvProc(IntPtr socket, IntPtr msg, int flags);
        public static ZmqRecvProc zmq_recv;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqSendProc(IntPtr socket, IntPtr msg, int flags);
        public static ZmqSendProc zmq_send;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ZmqSocketProc(IntPtr context, int type);
        public static ZmqSocketProc zmq_socket;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqMsgCloseProc(IntPtr msg);
        public static ZmqMsgCloseProc zmq_msg_close;

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
        public delegate int ZmqMsgSizeProc(IntPtr msg);
        public static ZmqMsgSizeProc zmq_msg_size;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqErrnoProc();
        public static ZmqErrnoProc zmq_errno;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr ZmqStrErrorProc(int errnum);
        public static ZmqStrErrorProc zmq_strerror;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqDeviceProc(int device, IntPtr inSocket, IntPtr outSocket);
        public static ZmqDeviceProc zmq_device;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ZmqVersionProc(IntPtr major, IntPtr minor, IntPtr patch);
        public static ZmqVersionProc zmq_version;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ZmqPollProc([In, Out] ZMQPollItem[] items, int nItems, long timeout);
        public static ZmqPollProc zmq_poll;
    }
    
    /// <summary>
    /// Utility class to wrap an unmanaged shared lib and be responsible for freeing it.
    /// </summary>
    /// <remarks>
    /// This is a managed wrapper over the native LoadLibrary, GetProcAddress, and FreeLibrary calls on Windows
    /// and dlopen, dlsym, and dlclose on Posix environments.
    /// </remarks>
    internal sealed class UnmanagedLibrary : IDisposable {
        private static class NativeMethods {
#if POSIX
            private const string KernelLib = "libdl.so";
                
            private const int RTLD_NOW = 2;
            private const int RTLD_GLOBAL = 0x100;
    
            [DllImport(KernelLib)]
            private static extern SafeLibraryHandle dlopen(string filename, int flags);
            
            [DllImport(KernelLib)]
            private static extern int dlclose(IntPtr handle);
            
            [DllImport(KernelLib)]
            private static extern string dlerror();
    
            [DllImport(KernelLib)]
            private static extern IntPtr dlsym(SafeLibraryHandle handle, string symbol);
            
            public static SafeLibraryHandle OpenHandle(string filename) {
                return NativeMethods.dlopen(filename + ".so", RTLD_NOW | RTLD_GLOBAL);
            }
            
            public static IntPtr LoadProcedure(SafeLibraryHandle handle, string functionName) {
                return NativeMethods.dlsym(handle, functionName);
            }
            
            public static bool ReleaseHandle(IntPtr handle) {
                return dlclose(handle) == 0;
            }
            
            public static void ThrowLastLibraryError() {
                throw new DllNotFoundException(dlerror());
            }
#else
            private const string KernelLib = "kernel32";

            [DllImport(KernelLib, CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
            private static extern SafeLibraryHandle LoadLibrary(string fileName);

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport(KernelLib, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool FreeLibrary(IntPtr moduleHandle);

            [DllImport(KernelLib)]
            private static extern IntPtr GetProcAddress(SafeLibraryHandle moduleHandle, String procname);
            
            public static SafeLibraryHandle OpenHandle(string filename) {
                return LoadLibrary(filename);
            }
            
            public static IntPtr LoadProcedure(SafeLibraryHandle handle, string functionName) {
                return GetProcAddress(handle, functionName);
            }
            
            public static bool ReleaseHandle(IntPtr handle) {
                return FreeLibrary(handle);
            }
            
            public static void ThrowLastLibraryError() {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
#endif
        }
        
        /// <summary>
        /// Safe handle for unmanaged libraries. See http://msdn.microsoft.com/msdnmag/issues/05/10/Reliability/ for more about safe handles.
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid {
            private SafeLibraryHandle()
                : base(true) {
            }

            protected override bool ReleaseHandle() {
                return NativeMethods.ReleaseHandle(handle);
            }
        }

        private SafeLibraryHandle _handle;
        
        /// <summary>
        /// Constructor to load a dll and be responible for freeing it.
        /// </summary>
        /// <param name="fileName">full path name of dll to load</param>
        /// <exception cref="System.IO.FileNotFoundException">if fileName can't be found</exception>
        /// <remarks>Throws exceptions on failure. Most common failure would be file-not-found, that the file is not a loadable image.</remarks>
        public UnmanagedLibrary(string fileName) {
            _handle = NativeMethods.OpenHandle(fileName);

            if (_handle.IsInvalid) {
                NativeMethods.ThrowLastLibraryError();
            }
        }
        
        /// <summary>
        /// Constructor load a dll from several possible file names and be responsible for freeing it.
        /// </summary>
        /// <param name="fileNames">File names to try in order of precedence</param>
        /// <exception cref="System.IO.FileNotFoundException">if fileName can't be found</exception>
        /// <remarks>Throws exceptions on failure. Most common failure would be file-not-found, that the file is not a loadable image.</remarks>
        public UnmanagedLibrary(params string[] fileNames) {
            foreach (var fileName in fileNames) {
                _handle = NativeMethods.OpenHandle(fileName);

                if (!_handle.IsInvalid) {
                    break;
                }
            }

            if (_handle.IsInvalid) {
                NativeMethods.ThrowLastLibraryError();
            }
        }
        
        /// <summary>
        /// Dynamically lookup a function in the dll via kernel32!GetProcAddress or libdl!dlsym.
        /// </summary>
        /// <typeparam name="TDelegate">Delegate type to load</typeparam>
        /// <param name="functionName">Raw name of the function in the export table.</param>
        /// <returns>A delegate to the unmanaged function.</returns>
        /// <exception cref="MissingMethodException">Thrown if the given function name is not found in the library.</exception>
        /// <remarks>
        /// GetProcAddress results are valid as long as the dll is not yet unloaded. This
        /// is very very dangerous to use since you need to ensure that the dll is not unloaded
        /// until after you're done with any objects implemented by the dll. For example, if you
        /// get a delegate that then gets an IUnknown implemented by this dll,
        /// you can not dispose this library until that IUnknown is collected. Else, you may free
        /// the library and then the CLR may call release on that IUnknown and it will crash.
        /// </remarks>
        public TDelegate GetUnmanagedFunction<TDelegate>(string functionName) where TDelegate : class {
            IntPtr p = NativeMethods.LoadProcedure(_handle, functionName);

            if (p == IntPtr.Zero) {
                throw new MissingMethodException("Unable to find function '" + functionName + "' in dynamically loaded library.");
            }

            // Ideally, we'd just make the constraint on TDelegate be
            // System.Delegate, but compiler error CS0702 (constrained can't be System.Delegate)
            // prevents that. So we make the constraint system.object and do the cast from object-->TDelegate.
            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(p, typeof(TDelegate));
        }
        
        public void Dispose() {
            if (!_handle.IsClosed) {
                _handle.Close();
            }
        }
    }
}
