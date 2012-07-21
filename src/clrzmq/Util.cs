/*

    Copyright (c) 2010 Jeffrey Dik <s450r1@gmail.com>
    Copyright (c) 2010 Martin Sustrik <sustrik@250bpm.com>
    Copyright (c) 2010 Michael Compton <michael.compton@littleedge.co.uk>

    This file is part of clrzmq2.

    clrzmq2 is free software; you can redistribute it and/or modify it under
    the terms of the Lesser GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    clrzmq2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    Lesser GNU General Public License for more details.

    You should have received a copy of the Lesser GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ZMQ {
    /// <summary>
    /// ZMQ Exception type
    /// </summary>
    public class Exception : System.Exception {
        private readonly int _errno;

        /// <summary>
        /// Get ZeroMQ Errno
        /// </summary>
        public int Errno {
            get { return _errno; }
        }

        public Exception()
#if PocketPC
        {
#else
            : base(Marshal.PtrToStringAnsi(C.zmq_strerror(C.zmq_errno()))) {
#endif
            _errno = C.zmq_errno();
        }
    }

    /// <summary>
    /// Utility methods
    /// </summary>
    public static class ZHelpers {
        public const int HAUSNUMERO = 156384712;
        private static Random _rand;

        private static Random GetRandomGen() {
            return _rand ?? (_rand = new Random(DateTime.Now.Millisecond));
        }

        /// <summary>
        /// Decode UUID to string
        /// </summary>
        /// <param name="data">UUID</param>
        /// <returns>String representation of UUID</returns>
        public static string DecodeUUID(byte[] data) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }

            const string hex = "0123456789ABCDEF";
            var uuid = new char[33];
            uuid[0] = '@';
            for (int byteNbr = 0; byteNbr < 16; byteNbr++) {
                uuid[(byteNbr * 2) + 1] = hex[data[byteNbr + 1] >> 4];
                uuid[(byteNbr * 2) + 2] = hex[data[byteNbr + 1] & 15];
            }
            return new String(uuid);
        }

        /// <summary>
        /// Encode string to UUID
        /// </summary>
        /// <param name="uuid">String representation of UUID</param>
        /// <returns>UUID</returns>
        public static byte[] EncodeUUID(string uuid) {
            if (uuid == null) {
                throw new ArgumentNullException("uuid");
            }

            var data = new byte[17];
            data[0] = 0;
            uuid = uuid.TrimStart('@');
            for (int byteNbr = 0; byteNbr < 16; byteNbr++) {
                data[byteNbr + 1] = Convert.ToByte(uuid.Substring(byteNbr * 2, 2), 16);
            }
            return data;
        }

        /// <summary>
        /// Prints all pending messages to the console.
        /// </summary>
        /// <param name="socket">ZMQ Socket</param>
        /// <param name="encoding">Encoding to use for message decoding</param>
        public static void Dump(Socket socket, Encoding encoding) {
            if (socket == null) {
                throw new ArgumentNullException("socket");
            }

            Console.WriteLine(new String('-', 38));
            foreach (byte[] msg in socket.RecvAll()) {
                Console.Write("[{0}] ", String.Format("{0:d3}", msg.Length));
                if (msg.Length == 17 && msg[0] == 0) {
                    Console.WriteLine(DecodeUUID(msg).Substring(1));
                }
                else {
                    Console.WriteLine(encoding.GetString(msg, 0, msg.Length));
                }
            }
        }

        /// <summary>
        /// Sets socket Identity to a random number.
        /// </summary>
        /// <param name="socket">ZMQ Socket</param>
        /// <param name="encoding">Encoding to use for the socket identity</param>
        /// <returns>The identity assigned to the socket.</returns>
        public static string SetID(Socket socket, Encoding encoding) {
            if (socket == null) {
                throw new ArgumentNullException("socket");
            }

            Random rand = GetRandomGen();
            string id = rand.Next() + "-" + rand.Next();
            socket.StringToIdentity(id, encoding);
            return id;
        }

        /// <summary>
        /// Get ZMQ version numbers
        /// </summary>
        /// <param name="major">Major</param>
        /// <param name="minor">Minor</param>
        /// <param name="patch">Patch</param>
        public static void Version(out int major, out int minor, out int patch) {
            int sizeofInt32 = Marshal.SizeOf(typeof(Int32));
            IntPtr maj = Marshal.AllocHGlobal(sizeofInt32);
            IntPtr min = Marshal.AllocHGlobal(sizeofInt32);
            IntPtr pat = Marshal.AllocHGlobal(sizeofInt32);
            C.zmq_version(maj, min, pat);
            major = Marshal.ReadInt32(maj);
            minor = Marshal.ReadInt32(min);
            patch = Marshal.ReadInt32(pat);
            Marshal.FreeHGlobal(maj);
            Marshal.FreeHGlobal(min);
            Marshal.FreeHGlobal(pat);
        }

        /// <summary>
        /// Assert the current version
        /// </summary>
        /// <param name="wantMajor">Desired Major</param>
        /// <param name="wantMinor">Desired Minor</param>
        public static void VersionAssert(int wantMajor, int wantMinor) {
            int major, minor, patch;
            Version(out major, out minor, out patch);
            if (major < wantMajor || (major == wantMajor && minor < wantMinor)) {
                Console.WriteLine("Current 0MQ version is {0}.{1}", major, minor);
                Console.WriteLine("Application needs at least {0}.{1} - cannot continue", wantMajor, wantMinor);
                throw new System.Exception(string.Format("Invalid 0MQ version. Current: {0}.{1}; expected: {2}.{3}", major, minor, wantMajor, wantMinor));
            }
        }

        /// <summary>
        /// Get ZMQ version
        /// </summary>
        /// <returns>ZMQ version string (major.minor.patch)</returns>
        public static string Version() {
            int major, minor, patch;
            Version(out major, out minor, out patch);
            return major + "." + minor + "." + patch;
        }
    }

    internal class DisposableIntPtr : IDisposable {
        public DisposableIntPtr(int size) {
            Ptr = Marshal.AllocHGlobal(size);
        }

        public IntPtr Ptr { get; private set; }

        ~DisposableIntPtr() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (Ptr != IntPtr.Zero) {
                Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
            }
        }
    }

    internal class ZmqMsgT : DisposableIntPtr {
        //  Figure out size of zmq_msg_t structure.
        //  It's size of pointer + 2 bytes + VSM buffer size.
        private const int ZMQ_MAX_VSM_SIZE = 30;
        private static readonly int ZMQ_MSG_T_SIZE = IntPtr.Size + 2 + ZMQ_MAX_VSM_SIZE;

        private bool initialized;

        public ZmqMsgT()
            : base(ZMQ_MSG_T_SIZE) {
        }

        public static implicit operator IntPtr(ZmqMsgT msg) {
            return msg.Ptr;
        }

        public void Init() {
            if (C.zmq_msg_init(Ptr) != 0) {
                throw new Exception();
            }
            initialized = true;
        }

        public void Init(int size) {
            if (C.zmq_msg_init_size(Ptr, size) != 0) {
                throw new Exception();
            }
            initialized = true;
        }

        public void Close() {
            if (C.zmq_msg_close(Ptr) != 0) {
                throw new Exception();
            }
            initialized = false;
        }

        public int Size() {
            return C.zmq_msg_size(Ptr);
        }

        public IntPtr Data() {
            return C.zmq_msg_data(Ptr);
        }

        protected override void Dispose(bool disposing) {
            if (disposing && initialized) {
                // Call the native method directly to avoid
                // throwing an exception during disposal
                C.zmq_msg_close(Ptr);
                initialized = false;
            }

            base.Dispose(disposing);
        }
    }
}
