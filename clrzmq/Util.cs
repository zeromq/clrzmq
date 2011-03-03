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
using System.Text;
using System.Runtime.InteropServices;

namespace ZMQ {
    /// <summary>
    /// CLRZeroMQ Exception type
    /// </summary>
    public class Exception : System.Exception {
        private int errno;

        /// <summary>
        /// Get ZeroMQ Errno
        /// </summary>
        public int Errno {
            get { return errno; }
        }

        public Exception()
            : base(Marshal.PtrToStringAnsi(C.zmq_strerror(C.zmq_errno()))) {
            this.errno = C.zmq_errno();
        }
    }

    /// <summary>
    /// CLRZMQ utility methods
    /// </summary>
    public static class ZHelpers {
        public const int HAUSNUMERO = 156384712;
        private static Random rand;

        private static Random GetRandomGen() {
            if (rand == null)
                rand = new Random(DateTime.Now.Millisecond);
            return rand;
        }

        /// <summary>
        /// Decode UUID to string
        /// </summary>
        /// <param name="data">UUID</param>
        /// <returns>String representation of UUID</returns>
        public static string DecodeUUID(byte[] data) {
            const string hex = "0123456789ABCDEF";
            char[] uuid = new char[33];
            uuid[0] = '@';
            for (int byteNbr = 0; byteNbr < 16; byteNbr++) {
                uuid[byteNbr * 2 + 1] = hex[data[byteNbr + 1] >> 4];
                uuid[byteNbr * 2 + 2] = hex[data[byteNbr + 1] & 15];
            }
            return new String(uuid);
        }

        /// <summary>
        /// Encode string to UUID
        /// </summary>
        /// <param name="uuid">String representation of UUID</param>
        /// <returns>UUID</returns>
        public static byte[] EncodeUUID(string uuid) {
            byte[] data = new byte[17];
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
        public static void Dump(Socket socket, Encoding encoding) {
            Console.WriteLine(new String('-', 38));
            foreach (byte[] msg in socket.RecvAll()) {
                Console.Write("[{0}] ", String.Format("{0:d3}", msg.Length));
                if (msg.Length == 17 && msg[0] == 0)  {
                    Console.WriteLine(ZHelpers.DecodeUUID(msg).Substring(1));
                } else {
                    Console.WriteLine(encoding.GetString(msg));
                }
            }
        }

        /// <summary>
        /// Sets socket Identity to a random number.
        /// </summary>
        /// <param name="socket">ZMQ Socket</param>
        public static void SetID(Socket socket, Encoding encoding) {
            Random rand = GetRandomGen();
            socket.StringToIdentity(rand.Next().ToString() + "-" +
                rand.Next().ToString(), encoding);
        }

        /// <summary>
        /// Get ZMQ version numbers
        /// </summary>
        /// <param name="major">Major</param>
        /// <param name="minor">Minor</param>
        /// <param name="patch">Patch</param>
        public static void Version(out int major, out int minor,
                                   out int patch) {
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
        /// Get ZMQ version
        /// </summary>
        /// <returns>ZMQ version string (major.minor.patch)</returns>
        public static string Version() {
            int major, minor, patch;
            Version(out major, out minor, out patch);
            return major.ToString() + "." + minor.ToString() + "." +
                patch.ToString();
        }
    }

    internal class DisposableIntPtr : IDisposable {
        IntPtr ptr;

        public DisposableIntPtr(int size) {
            ptr = Marshal.AllocHGlobal(size);
        }

        public IntPtr Ptr {
            get { return ptr; }
        }

        ~DisposableIntPtr() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (ptr != IntPtr.Zero) {
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
            }
        }
    }
}
