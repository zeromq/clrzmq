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
    internal static class C {
#if PocketPC
        private const CallingConvention PlatformConvention = CallingConvention.Winapi;
#else
        private const CallingConvention PlatformConvention = CallingConvention.Cdecl;
#endif

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern IntPtr zmq_init(int io_threads);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_term(IntPtr context);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_close(IntPtr socket);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_setsockopt(IntPtr socket, int option, IntPtr optval, int optvallen);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_getsockopt(IntPtr socket, int option, IntPtr optval, IntPtr optvallen);

#if PocketPC // Because on WM we have to pass ANSI as byte arrays
        [DllImport("libzmq", CallingConvention = CallingConvention.Winapi)]
        public static extern int zmq_bind(IntPtr socket, byte[] addr);

        [DllImport("libzmq", CallingConvention = CallingConvention.Winapi)]
        public static extern int zmq_connect(IntPtr socket, byte[] addr);
#else
        [DllImport("libzmq", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_bind(IntPtr socket, string addr);

        [DllImport("libzmq", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_connect(IntPtr socket, string addr);
#endif

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_recv(IntPtr socket, IntPtr msg, int flags);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_send(IntPtr socket, IntPtr msg, int flags);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern IntPtr zmq_socket(IntPtr context, int type);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_msg_close(IntPtr msg);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern IntPtr zmq_msg_data(IntPtr msg);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_msg_init(IntPtr msg);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_msg_init_size(IntPtr msg, int size);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_msg_size(IntPtr msg);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_errno();

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern IntPtr zmq_strerror(int errnum);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_device(int device, IntPtr inSocket, IntPtr outSocket);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern void zmq_version(IntPtr major, IntPtr minor, IntPtr patch);

        [DllImport("libzmq", CallingConvention = PlatformConvention)]
        public static extern int zmq_poll([In, Out] ZMQPollItem[] items, int numItems, long timeout);
    }
}
