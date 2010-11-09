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
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SysSockets = System.Net.Sockets;

namespace ZMQ {
    /// <summary>
    /// Message transport types
    /// </summary>
    public enum Transport {
        INPROC = 1,
        TCP = 2,
        IPC = 3,
        PGM = 4,
        EPGM = 5
    }

    /// <summary>
    /// Socket options
    /// </summary>
    public enum SocketOpt {
        HWM = 1,
        SWAP = 3,
        AFFINITY = 4,
        IDENTITY = 5,
        SUBSCRIBE = 6,
        UNSUBSCRIBE = 7,
        RATE = 8,
        RECOVERY_IVL = 9,
        MCAST_LOOP = 10,
        SNDBUF = 11,
        RCVBUF = 12,
        RCVMORE = 13,
        FD = 14,
        EVENTS = 15,
        TYPE = 16,
        LINGER = 17,
        RECONNECT_IVL = 18,
        BACKLOG = 19
    }

    /// <summary>
    /// Socket types
    /// </summary>
    public enum SocketType {
        PAIR = 0,
        PUB = 1,
        SUB = 2,
        REQ = 3,
        REP = 4,
        XREQ = 5,
        XREP = 6,
        PULL = 7,
        UPSTREAM = 7,       //***OBSOLETE: To be removed in 3.x***
        PUSH = 8,
        DOWNSTREAM = 8      //***OBSOLETE: To be removed in 3.x***
    }

    /// <summary>
    /// Device types
    /// </summary>
    public enum DeviceType {
        STREAMER = 1,
        FORWARDER = 2,
        QUEUE = 3
    }

    /// <summary>
    /// Send and receive options
    /// </summary>
    public enum SendRecvOpt {
        NONE = 0,
        NOBLOCK = 1,
        SNDMORE = 2
    }

    /// <summary>
    /// IO Multiplexing polling events bit flags
    /// </summary>
    public enum IOMultiPlex {
        POLLIN = 0x1,
        POLLOUT = 0x2,
        POLLERR = 0x4
    }

    /// <summary>
    /// Polling event handler
    /// </summary>
    /// <param name="socket">Target socket</param>
    /// <param name="revents">Poll events</param>
    public delegate void PollHandler(Socket socket, IOMultiPlex revents);

    /// <summary>
    /// ZMQ Poll item, sets target socket and events.
    /// </summary>
    public struct ZMQPollItem {
        private IntPtr socket;
#if x86 || POSIX
        private int fd;
#else
        private long fd;
#endif
        private short events;
        private short revents;

#if x86 || POSIX
        /// <summary>
        /// Do not call directly
        /// </summary>
        /// <param name="socket">Target ZMQ socket ptr</param>
        /// <param name="fd">Non ZMQ socket (Experimental)</param>
        /// <param name="events">Desired events</param>
        /// <param name="revents">Returned events</param>
        internal ZMQPollItem(IntPtr socket, object fd, short events) {
            this.socket = socket;
            this.events = events;
            this.revents = 0;
            this.fd = Convert.ToInt32(fd);
        }
#else
        /// <summary>
        /// Do not call directly
        /// </summary>
        /// <param name="socket">Target ZMQ socket ptr</param>
        /// <param name="fd">Non ZMQ socket (Not Supported)</param>
        /// <param name="events">Desired events</param>
        /// <param name="revents">Returned events</param>
        internal ZMQPollItem(IntPtr socket, object fd, short events) {
            this.socket = socket;
            this.events = events;
            this.revents = 0;
            this.fd = unchecked((long)Convert.ToInt64(fd));
        }
#endif

        /// <summary>
        /// Reset revents so that poll item can be safely reused
        /// </summary>
        public void ResetRevents() {
            revents = 0;
        }

        /// <summary>
        /// Get returned event flags
        /// </summary>
        public IOMultiPlex Revents {
            get {
                IOMultiPlex revents = (IOMultiPlex)this.revents;
                return revents;
            }
        }
    }

    /// <summary>
    /// Polling item, provides the polling mechanism
    /// </summary>
    public class PollItem {
        private ZMQPollItem zmqPollItem;
        private Socket socket;

        private event PollHandler PollInHandlers;
        private event PollHandler PollOutHandlers;
        private event PollHandler PollErrHandlers;

        /// <summary>
        /// Should not be created directly, use Socket.CreatePollItem
        /// </summary>
        /// <param name="zmqPollItem"></param>
        /// <param name="socket"></param>
        internal PollItem(ZMQPollItem zmqPollItem, Socket socket) {
            this.socket = socket;
            this.zmqPollItem = zmqPollItem;
        }

        /// <summary>
        /// POLLIN event handler
        /// </summary>
        public event PollHandler PollInHandler {
            add {
                PollInHandlers += value;
            }
            remove {
                PollInHandlers -= value;
            }
        }

        /// <summary>
        /// POLLOUT event handler
        /// </summary>
        public event PollHandler PollOutHandler {
            add {
                PollOutHandlers += value;
            }
            remove {
                PollOutHandlers -= value;
            }
        }

        /// <summary>
        /// POLLERR event handler
        /// </summary>
        public event PollHandler PollErrHandler {
            add {
                PollErrHandlers += value;
            }
            remove {
                PollErrHandlers -= value;
            }
        }

        /// <summary>
        /// Fire handlers for any returned events
        /// </summary>
        public void FireEvents() {
            if ((zmqPollItem.Revents & IOMultiPlex.POLLIN) ==
                IOMultiPlex.POLLIN)  {
                PollInHandlers(socket, zmqPollItem.Revents);
            }
            if ((zmqPollItem.Revents & IOMultiPlex.POLLOUT) ==
                IOMultiPlex.POLLOUT) {
                PollOutHandlers(socket, zmqPollItem.Revents);
            }
            if ((zmqPollItem.Revents & IOMultiPlex.POLLERR) ==
                IOMultiPlex.POLLERR) {
                PollErrHandlers(socket, zmqPollItem.Revents);
            }
            zmqPollItem.ResetRevents();
        }

        /// <summary>
        /// Get and Set ZMQ poll item
        /// </summary>
        public ZMQPollItem ZMQPollItem {
            get {
                return zmqPollItem;
            }
            set {
                zmqPollItem = value;
            }
        }
    }

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
        private static Random rand;

        private static Random GetRandomGen() {
            if (rand == null)
                rand = new Random(DateTime.Now.Millisecond);
            return rand;
        }

        /// <summary>
        /// Prints all pending messages to the console.
        /// </summary>
        /// <param name="socket">ZMQ Socket</param>
        public static void Dump(Socket socket, Encoding encoding) {
            foreach (byte[] msg in socket.RecvAll()) {
                Console.WriteLine(encoding.GetString(msg));
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

    /// <summary>
    /// ZMQ Context
    /// </summary>
    public class Context : IDisposable {
        private IntPtr ptr;

        /// <summary>
        /// Create ZMQ Context
        /// </summary>
        /// <param name="io_threads">Thread pool size</param>
        public Context(int io_threads) {
            ptr = C.zmq_init(io_threads);
            if (ptr == IntPtr.Zero)
                throw new Exception();
        }

        ~Context() {
            Dispose(false);
        }

        /// <summary>
        /// Creates a new ZMQ socket
        /// </summary>
        /// <param name="type">Type of socket to be created</param>
        /// <returns>A new ZMQ socket</returns>
        public Socket Socket(SocketType type) {
            IntPtr socket_ptr = C.zmq_socket(ptr, (int)type);
            if (ptr == IntPtr.Zero)
                throw new Exception();

            return new Socket(socket_ptr);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (ptr != IntPtr.Zero) {
                int rc = C.zmq_term(ptr);
                ptr = IntPtr.Zero;
                if (rc != 0)
                    throw new Exception();
            }
        }

        /// <summary>
        /// Polls the supplied items for events
        /// </summary>
        /// <param name="items">Items to Poll</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Number of Poll items with events</returns>
        public int Poll(PollItem[] items, long timeout) {
            int sizeOfZPL = Marshal.SizeOf(typeof(ZMQPollItem));
            IntPtr itemList = Marshal.AllocHGlobal(sizeOfZPL * items.Length);
            IntPtr offset = itemList;
            foreach (PollItem item in items) {
                Marshal.StructureToPtr(item.ZMQPollItem, offset, false);
#if x64
                offset = new IntPtr(offset.ToInt64() + sizeOfZPL);
#elif x86
                offset = new IntPtr(offset.ToInt32() + sizeOfZPL);
#endif
            }
            int rc = C.zmq_poll(itemList, items.Length, timeout);
            if (rc < 0)
                throw new Exception();
            if (rc > 0) {
                for (int index = 0; index < items.Length; index++) {
                    items[index].ZMQPollItem = (ZMQPollItem)
                        Marshal.PtrToStructure(itemList, typeof(ZMQPollItem));
#if x64
                    itemList = new IntPtr(itemList.ToInt64() + sizeOfZPL);
#elif x86
                    itemList = new IntPtr(itemList.ToInt32() + sizeOfZPL);
#endif
                }
                foreach (PollItem item in items) {
                    item.FireEvents();
                }
            }
            return rc;
        }

        /// <summary>
        /// Polls the supplied items for events, with immediate timeout
        /// </summary>
        /// <param name="items">Items to Poll</param>
        /// <returns>Number of Poll items with events</returns>
        public int Poll(PollItem[] items) {
            return Poll(items, -1);
        }
    }

    /// <summary>
    /// ZMQ Socket
    /// </summary>
    public class Socket : IDisposable {
        private IntPtr ptr;
        private IntPtr msg;
        private string address;

        //  TODO:  This won't hold on different platforms.
        //  Isn't there a way to access POSIX error codes in CLR?
        private const int EAGAIN = 11;

        //  Figure out size of zmq_msg_t structure.
        //  It's size of pointer + 2 bytes + VSM buffer size.
        private const int ZMQ_MAX_VSM_SIZE = 30;
        private int ZMQ_MSG_T_SIZE = IntPtr.Size + 2 + ZMQ_MAX_VSM_SIZE;

        /// <summary>
        /// This constructor should not be called directly, use the Context
        /// Socket method
        /// </summary>
        /// <param name="ptr">Pointer to a socket</param>
        internal Socket(IntPtr ptr) {
            this.ptr = ptr;
            msg = Marshal.AllocHGlobal(ZMQ_MSG_T_SIZE);
        }

        ~Socket() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (msg != IntPtr.Zero) {
                Marshal.FreeHGlobal(msg);
                msg = IntPtr.Zero;
            }

            if (ptr != IntPtr.Zero) {
                int rc = C.zmq_close(ptr);
                ptr = IntPtr.Zero;
                if (rc != 0)
                    throw new Exception();
            }
        }

#if x86
        /// <summary>
        /// Allows cross platform reading of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_t</param>
        /// <returns>Size_t value</returns>
        private object ReadSizeT(IntPtr ptr) {
            return unchecked((uint)Marshal.ReadInt32(ptr));
        }

        /// <summary>
        /// Allows cross platform writing of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_</param>
        /// <param name="val">Value to write</param>
        private void WriteSizeT(IntPtr ptr, object val) {
            Marshal.WriteInt32(ptr, unchecked(Convert.ToInt32(val)));
        }
#elif x64
        /// <summary>
        /// Allows cross platform reading of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_t</param>
        /// <returns>Size_t value</returns>
        private object ReadSizeT(IntPtr ptr) {
            return unchecked((ulong)Marshal.ReadInt64(ptr));
        }

        /// <summary>
        /// Allows cross platform writing of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_</param>
        /// <param name="val">Value to write</param>
        private void WriteSizeT(IntPtr ptr, object val) {
            Marshal.WriteInt64(ptr, unchecked(Convert.ToInt64(val)));
        }
#endif
        /// <summary>
        /// Create poll item for ZMQ socket listening, for the supplied events
        /// </summary>
        /// <param name="events">Listening events</param>
        /// <returns>Socket Poll item</returns>
        public PollItem CreatePollItem(IOMultiPlex events) {
            return new PollItem(new ZMQPollItem(ptr, 0, (short)events), this);
        }

        /// <summary>
        /// Create poll item for ZMQ and plain socket listening, for the supplied events
        /// </summary>
        /// <param name="events">Listening events</param>
        /// <param name="sysSocket">Raw Socket</param>
        /// <returns>Socket Poll item</returns>
        public PollItem CreatePollItem(IOMultiPlex events,
                                       SysSockets.Socket sysSocket) {
#if x86 || POSIX
            return new PollItem(new ZMQPollItem(ptr, sysSocket.Handle.ToInt32(),
                                                (short)events), this);
#elif x64
            return new PollItem(new ZMQPollItem(ptr, sysSocket.Handle.ToInt64(),
                                                (short)events), this);
#endif
        }


        /// <summary>
        /// Set Socket Option
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <param name="value">Option value</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SetSockOpt(SocketOpt option, ulong value) {
            int sizeOfValue = Marshal.SizeOf(typeof(ulong));
            IntPtr valPtr = Marshal.AllocHGlobal(sizeOfValue);
            WriteSizeT(valPtr, value);
            if (C.zmq_setsockopt(ptr, (int)option, valPtr, sizeOfValue) != 0)
                throw new Exception();
            Marshal.FreeHGlobal(valPtr);
        }

        /// <summary>
        /// Set Socket Option
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <param name="value">Option value</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SetSockOpt(SocketOpt option, byte[] value) {
            IntPtr valPtr = Marshal.AllocHGlobal(value.Length);
            Marshal.Copy(value, 0, valPtr, value.Length);
            if (C.zmq_setsockopt(ptr, (int)option, valPtr, value.Length) != 0)
                throw new Exception();
            Marshal.FreeHGlobal(valPtr);
        }

        /// <summary>
        /// Set Socket Option
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <param name="value">Option value</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SetSockOpt(SocketOpt option, long value) {
            int sizeOfValue = Marshal.SizeOf(typeof(long));
            IntPtr valPtr = Marshal.AllocHGlobal(sizeOfValue);
            WriteSizeT(valPtr, value);
            if (C.zmq_setsockopt(ptr, (int)option, valPtr, sizeOfValue) != 0)
                throw new Exception();
            Marshal.FreeHGlobal(valPtr);
        }

        /// <summary>
        /// Get the socket option value
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <returns>Option value</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public object GetSockOpt(SocketOpt option) {
            object output;
            IntPtr val;
            IntPtr len = Marshal.AllocHGlobal(IntPtr.Size);
            if (option == SocketOpt.IDENTITY) {
                val = Marshal.AllocHGlobal(255);
                WriteSizeT(len, 255);
                if (C.zmq_getsockopt(ptr, (int)option, val, len) != 0)
                    throw new Exception();
                byte[] buffer = new byte[Convert.ToInt32(ReadSizeT(len))];
                Marshal.Copy(val, buffer, 0, buffer.Length);
                output = buffer;
            } else {
                val = Marshal.AllocHGlobal(IntPtr.Size);
                WriteSizeT(len, Marshal.SizeOf(typeof(long)));
                if (C.zmq_getsockopt(ptr, (int)option, val, len) != 0)
                    throw new Exception();
                //Unchecked casting of uint64 options
                if (option == SocketOpt.HWM || option == SocketOpt.AFFINITY ||
                    option == SocketOpt.SNDBUF || option == SocketOpt.RCVBUF) {
                    output = unchecked((ulong)Marshal.ReadInt64(val));
                } else {
                    output = Marshal.ReadInt64(val);
                }
            }
            Marshal.FreeHGlobal(val);
            Marshal.FreeHGlobal(len);
            return output;
        }

        /// <summary>
        /// Bind the socket to address
        /// </summary>
        /// <param name="addr">Socket Address</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Bind(string addr) {
            address = addr;
            if (C.zmq_bind(ptr, addr) != 0)
                throw new Exception();
        }

        /// <summary>
        /// Bind the socket to address
        /// </summary>
        /// <param name="transport">Socket transport type</param>
        /// <param name="addr">Socket address</param>
        /// <param name="port">Socket port</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Bind(Transport transport, string addr, uint port) {
            Bind(Enum.GetName(typeof(Transport), transport).ToLower() + "://" +
                 addr + ":" + port);
        }

        /// <summary>
        /// Bind the socket to address
        /// </summary>
        /// <param name="transport">Socket transport type</param>
        /// <param name="addr">Socket address</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Bind(Transport transport, string addr) {
            Bind(Enum.GetName(typeof(Transport), transport).ToLower() + "://" +
                 addr);
        }

        /// <summary>
        /// Connect socket to destination address
        /// </summary>
        /// <param name="addr">Destination Address</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Connect(string addr) {
            address = addr;
            if (C.zmq_connect(ptr, addr) != 0)
                throw new Exception();
        }

        /// <summary>
        /// Listen for message
        /// </summary>
        /// <param name="flags">Receive Options</param>
        /// <returns>Message</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public byte[] Recv(params SendRecvOpt[] flags) {
            byte[] message = null;
            int flagsVal = 0;
            foreach (SendRecvOpt opt in flags) {
                flagsVal += (int)opt;
            }
            if (C.zmq_msg_init(msg) != 0)
                throw new Exception();
            if (C.zmq_recv(ptr, msg, flagsVal) == 0) {
                message = new byte[C.zmq_msg_size(msg)];
                Marshal.Copy(C.zmq_msg_data(msg), message, 0, message.Length);
                C.zmq_msg_close(msg);
            } else {
                if (C.zmq_errno() != EAGAIN)
                    throw new Exception();
            }
            return message;
        }

        /// <summary>
        /// Listen for message
        /// </summary>
        /// <returns>Message</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public byte[] Recv() {
            return Recv(SendRecvOpt.NONE);
        }

        /// <summary>
        /// Listen for message, and return it in string format
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <returns>Message string</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public string Recv(Encoding encoding) {
            return Recv(encoding, SendRecvOpt.NONE);
        }

        /// <summary>
        /// Listen for message, and return it in string format
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <param name="flags">Receive options</param>
        /// <returns>Message string</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public string Recv(Encoding encoding, params SendRecvOpt[] flags) {
            return encoding.GetString(Recv(flags));
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts
        /// </summary>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<byte[]> RecvAll() {
            return RecvAll(SendRecvOpt.NONE);
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts
        /// </summary>
        /// <param name="flags">Receive options</param>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<byte[]> RecvAll(params SendRecvOpt[] flags) {
            Queue<byte[]> messages = new Queue<byte[]>();
            messages.Enqueue(Recv(flags));
            while (RcvMore) {
                messages.Enqueue(Recv(flags));
            }
            return messages;
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts
        /// </summary>
        /// <param name="encoding">String Encoding</param>
        /// <returns>Queue of message parts as strings</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<string> RecvAll(Encoding encoding) {
            Queue<string> messages = new Queue<string>();
            messages.Enqueue(Recv(encoding));
            while (RcvMore) {
                messages.Enqueue(Recv(encoding));
            }
            return messages;
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts
        /// </summary>
        /// <param name="encoding">String Encoding</param>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<string> RecvAll(Encoding encoding,
                                     params SendRecvOpt[] flags) {
            Queue<string> messages = new Queue<string>();
            messages.Enqueue(Recv(encoding, flags));
            while (RcvMore) {
                messages.Enqueue(Recv(encoding, flags));
            }
            return messages;
        }

        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="flags">Send Options</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Send(byte[] message, params SendRecvOpt[] flags) {
            int flagsVal = 0;
            foreach (SendRecvOpt opt in flags) {
                flagsVal += (int)opt;
            }
            if (C.zmq_msg_init_size(msg, message.Length) != 0)
                throw new Exception();
            Marshal.Copy(message, 0, C.zmq_msg_data(msg), message.Length);
            if (C.zmq_send(ptr, msg, flagsVal) != 0)
                throw new Exception();
        }

        /// <summary>
        /// Send Message.
        /// </summary>
        /// <param name="message">Message</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Send(byte[] message) {
            Send(message, SendRecvOpt.NONE);
        }

        /// <summary>
        /// Send Message.
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="encoding">String encoding</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Send(string message, Encoding encoding) {
            Send(message, encoding, SendRecvOpt.NONE);
        }

        /// <summary>
        /// Send multi-part message, holding for further message parts.
        /// </summary>
        /// <param name="message">Message</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SendMore(byte[] message) {
            Send(message, SendRecvOpt.SNDMORE);
        }

        /// <summary>
        /// Send multi-part message, holding for further message parts.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encoding">String encoding</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SendMore(string message, Encoding encoding) {
            Send(message, encoding, SendRecvOpt.SNDMORE);
        }

        /// <summary>
        /// Send multi-part message, holding for further message parts.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encoding">String encoding</param>
        /// <param name="flags">Send options</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SendMore(string message, Encoding encoding,
                             params SendRecvOpt[] flags) {
            Send(message, encoding, flags);
        }

        /// <summary>
        /// Send Message.
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="flags">Send Options</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Send(string message, Encoding encoding,
                         params SendRecvOpt[] flags) {
            Send(encoding.GetBytes(message), flags);
        }

        /// <summary>
        /// Retrieve identity as a string
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <returns>Socket Identity</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public string IdentityToString(Encoding encoding) {
            return encoding.GetString(Identity);
        }

        /// <summary>
        /// Set identity from string
        /// </summary>
        /// <param name="identity">Identity</param>
        /// <param name="encoding">String encoding</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void StringToIdentity(string identity, Encoding encoding) {
            Identity = encoding.GetBytes(identity);
        }

        /// <summary>
        /// Gets or Sets socket identity
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public byte[] Identity {
            get {
                return (byte[])GetSockOpt(SocketOpt.IDENTITY);
            }
            set {
                SetSockOpt(SocketOpt.IDENTITY, value);
            }
        }

        /// <summary>
        /// Gets or Sets socket High Water Mark
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public ulong HWM {
            get {
                return (ulong)GetSockOpt(SocketOpt.HWM);
            }
            set {
                SetSockOpt(SocketOpt.HWM, value);
            }
        }

        /// <summary>
        /// Gets socket more messages indicator (multipart message)
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public bool RcvMore {
            get {
                return (long)GetSockOpt(SocketOpt.RCVMORE) == 1;
            }
        }

        /// <summary>
        /// Gets or Sets socket swap
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public long Swap {
            get {
                return (long)GetSockOpt(SocketOpt.SWAP);
            }
            set {
                SetSockOpt(SocketOpt.SWAP, value);
            }
        }

        /// <summary>
        /// Gets or Sets socket thread affinity
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public ulong Affinity {
            get {
                return (ulong)GetSockOpt(SocketOpt.AFFINITY);
            }
            set {
                SetSockOpt(SocketOpt.AFFINITY, value);
            }
        }

        /// <summary>
        /// Gets or Sets socket transfer rate
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public long Rate {
            get {
                return (long)GetSockOpt(SocketOpt.RATE);
            }
            set {
                SetSockOpt(SocketOpt.RATE, value);
            }
        }

        /// <summary>
        /// Gets or Sets socket recovery interval
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public long RecoveryIvl {
            get {
                return (long)GetSockOpt(SocketOpt.RECOVERY_IVL);
            }
            set {
                SetSockOpt(SocketOpt.RECOVERY_IVL, value);
            }
        }

        /// <summary>
        /// Gets or Sets socket MultiCast Loop
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public long MCastLoop {
            get {
                return (long)GetSockOpt(SocketOpt.MCAST_LOOP);
            }
            set {
                SetSockOpt(SocketOpt.MCAST_LOOP, value);
            }
        }

        /// <summary>
        /// Gets or Sets socket Send buffer size(bytes)
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public ulong SndBuf {
            get {
                return (ulong)GetSockOpt(SocketOpt.SNDBUF);
            }
            set {
                SetSockOpt(SocketOpt.SNDBUF, value);
            }
        }

        /// <summary>
        /// Gets or Sets socket Receive buffer size(bytes)
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public ulong RcvBuf {
            get {
                return (ulong)GetSockOpt(SocketOpt.RCVBUF);
            }
            set {
                SetSockOpt(SocketOpt.RCVBUF, value);
            }
        }

        /// <summary>
        /// Get connection address
        /// </summary>
        public string Address {
            get {
                return address;
            }
        }

        /// <summary>
        /// Set socket subscription filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Subscribe(byte[] filter) {
            SetSockOpt(SocketOpt.SUBSCRIBE, filter);
        }

        /// <summary>
        /// Set socket subscription filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Subscribe(string filter, Encoding encoding) {
            SetSockOpt(SocketOpt.SUBSCRIBE, encoding.GetBytes(filter));
        }

        /// <summary>
        /// Remove socket subscription filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Unsubscribe(byte[] filter) {
            SetSockOpt(SocketOpt.UNSUBSCRIBE, filter);
        }

        /// <summary>
        /// Remove socket subscription filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Unsubscribe(string filter, Encoding encoding) {
            SetSockOpt(SocketOpt.UNSUBSCRIBE, encoding.GetBytes(filter));
        }

        /// <summary>
        /// ZMQ Device creation
        /// </summary>
        public static class Device {
            /// <summary>
            /// Create ZMQ Device
            /// </summary>
            /// <param name="device">Device type</param>
            /// <param name="inSocket">Input socket</param>
            /// <param name="outSocket">Output socket</param>
            /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
            public static void Create(DeviceType device, Socket inSocket,
                                      Socket outSocket) {
                if (C.zmq_device((int)device, inSocket.ptr, outSocket.ptr) != 0)
                    throw new Exception();
            }

            /// <summary>
            /// Create ZMQ Queue Device
            /// </summary>
            /// <param name="inSocket">Input socket</param>
            /// <param name="outSocket">Output socket</param>
            /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
            public static void Queue(Socket inSocket, Socket outSocket) {
                Create(DeviceType.QUEUE, inSocket, outSocket);
            }

            /// <summary>
            /// Create ZMQ Forwarder Device
            /// </summary>
            /// <param name="inSocket">Input socket</param>
            /// <param name="outSocket">Output socket</param>
            /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
            public static void Forwarder(Socket inSocket, Socket outSocket) {
                Create(DeviceType.FORWARDER, inSocket, outSocket);
            }

            /// <summary>
            /// Create ZMQ Streamer Device
            /// </summary>
            /// <param name="inSocket">Input socket</param>
            /// <param name="outSocket">Output socket</param>
            /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
            public static void Streamer(Socket inSocket, Socket outSocket) {
                Create(DeviceType.STREAMER, inSocket, outSocket);
            }
        }
    }
}