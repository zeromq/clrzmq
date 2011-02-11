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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SysSockets = System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace ZMQ {
    /// <summary>
    /// ZMQ Socket
    /// </summary>
    public class Socket : IDisposable {
        private static Context _appContext;
        private static int _appSocketCount;
        private static Object _lockObj = new object();

        private PollItem _pollItem;
        private bool _localSocket;
        private IntPtr _ptr;
        private IntPtr _msg;
        private string _address;

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
            this._ptr = ptr;
            CommonInit(false);
        }

        /// <summary>
        /// Create Socket using application wide Context
        /// </summary>
        /// <param name="type">Socket type</param>
        public Socket(SocketType type) {
            lock (_lockObj) {
                if (_appContext == null) {
                    _appContext = new Context();
                }
                _ptr = _appContext.CreateSocketPtr(type);
            }
            Interlocked.Increment(ref _appSocketCount);
            CommonInit(true);
        }

        private void CommonInit(bool local) {
            _msg = Marshal.AllocHGlobal(ZMQ_MSG_T_SIZE);
            _localSocket = local;
            _pollItem = new PollItem(new ZMQPollItem(_ptr, 0, 0), this);
        }

        ~Socket() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal IntPtr Ptr {
            get { return _ptr;}
        }

        /// <summary>
        /// POLLIN event handler
        /// </summary>
        public event PollHandler PollInHandler {
            add {
                _pollItem.PollInHandler += value;                
            }
            remove {
                _pollItem.PollInHandler -= value;
            }
        }

        /// <summary>
        /// POLLOUT event handler
        /// </summary>
        public event PollHandler PollOutHandler {
            add {
                _pollItem.PollOutHandler += value;                
            }
            remove {
                _pollItem.PollOutHandler -= value;
            }
        }

        /// <summary>
        /// POLLERR event handler
        /// </summary>
        public event PollHandler PollErrHandler {
            add {
                _pollItem.PollErrHandler += value;
            }
            remove {
                _pollItem.PollErrHandler -= value;
            }
        }

        internal PollItem PollItem {
            get { return _pollItem; }
        }

        protected virtual void Dispose(bool disposing) {
            if (_msg != IntPtr.Zero) {
                Marshal.FreeHGlobal(_msg);
                _msg = IntPtr.Zero;
            }

            if (_ptr != IntPtr.Zero) {
                int rc = C.zmq_close(_ptr);
                _ptr = IntPtr.Zero;
                if (rc != 0)
                    throw new Exception();
            }
            if (_localSocket) {
                Interlocked.Decrement(ref _appSocketCount);
                lock (_lockObj) {
                    if (_appSocketCount == 0) {
                        _appContext.Dispose();
                        _appContext = null;
                    }
                }
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
            return new PollItem(new ZMQPollItem(_ptr, 0, (short)events), this);
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
            return new PollItem(new ZMQPollItem(_ptr, sysSocket.Handle.ToInt32(),
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
            using (DisposableIntPtr valPtr = new DisposableIntPtr(sizeOfValue)) {
                WriteSizeT(valPtr.Ptr, value);
                if (C.zmq_setsockopt(_ptr, (int)option, valPtr.Ptr, sizeOfValue) != 0)
                    throw new Exception();
            }
        }

        /// <summary>
        /// Set Socket Option
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <param name="value">Option value</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SetSockOpt(SocketOpt option, byte[] value) {
            using (DisposableIntPtr valPtr = new DisposableIntPtr(value.Length)) {
                Marshal.Copy(value, 0, valPtr.Ptr, value.Length);
                if (C.zmq_setsockopt(_ptr, (int)option, valPtr.Ptr, value.Length) != 0)
                    throw new Exception();
            }
        }

        /// <summary>
        /// Set Socket Option
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <param name="value">Option value</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SetSockOpt(SocketOpt option, long value) {
            int sizeOfValue = Marshal.SizeOf(typeof(long));
            using (DisposableIntPtr valPtr = new DisposableIntPtr(sizeOfValue)) {
                WriteSizeT(valPtr.Ptr, value);
                if (C.zmq_setsockopt(_ptr, (int)option, valPtr.Ptr, sizeOfValue) != 0)
                    throw new Exception();
            }
        }

        /// <summary>
        /// Get the socket option value
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <returns>Option value</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public object GetSockOpt(SocketOpt option) {
            const int IDLenSize = 255;  //Identity value length 255 bytes
            const int lenSize = 8;      //Non-Identity value size 8 bytes
            object output;
            using (DisposableIntPtr len = new DisposableIntPtr(IntPtr.Size)) {
                if (option == SocketOpt.IDENTITY) {
                    using (DisposableIntPtr val = new DisposableIntPtr(IDLenSize)) {
                        WriteSizeT(len.Ptr, IDLenSize);
                        if (C.zmq_getsockopt(_ptr, (int)option, val.Ptr, len.Ptr) != 0)
                            throw new Exception();
                        byte[] buffer = new byte[Convert.ToInt32(ReadSizeT(len.Ptr))];
                        Marshal.Copy(val.Ptr, buffer, 0, buffer.Length);
                        output = buffer;
                    }
                } else {
                    using (DisposableIntPtr val = new DisposableIntPtr(lenSize)) {
                        WriteSizeT(len.Ptr, lenSize);
                        if (C.zmq_getsockopt(_ptr, (int)option, val.Ptr, len.Ptr) != 0)
                            throw new Exception();
                        //Unchecked casting of uint64 options
                        if (option == SocketOpt.HWM || option == SocketOpt.AFFINITY ||
                            option == SocketOpt.SNDBUF || option == SocketOpt.RCVBUF) {
                            output = unchecked((ulong)Marshal.ReadInt64(val.Ptr));
                        } else {
                            output = Marshal.ReadInt64(val.Ptr);
                        }
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Bind the socket to address
        /// </summary>
        /// <param name="addr">Socket Address</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Bind(string addr) {
            _address = addr;
            if (C.zmq_bind(_ptr, addr) != 0)
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
            _address = addr;
            if (C.zmq_connect(_ptr, addr) != 0)
                throw new Exception();
        }

        /// <summary>
        /// Forward all message parts directly to destination. No marshalling performed.
        /// </summary>
        /// <param name="destination">Destination Socket</param>
        public void Forward(Socket destination) {
            SendRecvOpt opt = SendRecvOpt.SNDMORE;
            while (opt == SendRecvOpt.SNDMORE) {
                if (C.zmq_msg_init(_msg) != 0)
                    throw new Exception();
                if (C.zmq_recv(_ptr, _msg, 0) == 0) {
                    opt = RcvMore ? SendRecvOpt.SNDMORE : SendRecvOpt.NONE;
                    if (C.zmq_send(destination.Ptr, _msg, (int)opt) != 0)
                        throw new Exception();
                    C.zmq_msg_close(_msg);
                } else {
                    if (C.zmq_errno() != EAGAIN)
                        throw new Exception();
                }
            }
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
            if (C.zmq_msg_init(_msg) != 0)
                throw new Exception();
            if (C.zmq_recv(_ptr, _msg, flagsVal) == 0) {
                message = new byte[C.zmq_msg_size(_msg)];
                Marshal.Copy(C.zmq_msg_data(_msg), message, 0, message.Length);
                C.zmq_msg_close(_msg);
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
        /// Listen for message with timeout
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Message</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public byte[] Recv(int timeout) {
            Stopwatch timer = new Stopwatch();
            byte[] data = null;
            timer.Start();
            while (data == null && timer.ElapsedMilliseconds < timeout) {
                data = Recv(SendRecvOpt.NOBLOCK);
            }
            return data;
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
        /// Listen for message, and return it in string format, with timeout
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Message string</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public string Recv(Encoding encoding, int timeout) {
            return encoding.GetString(Recv(timeout));
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
            if (C.zmq_msg_init_size(_msg, message.Length) != 0)
                throw new Exception();
            Marshal.Copy(message, 0, C.zmq_msg_data(_msg), message.Length);
            if (C.zmq_send(_ptr, _msg, flagsVal) != 0)
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
        /// Send empty message part
        /// </summary>
        public void Send() {
            Send(new byte[0]);
        }

        /// <summary>
        /// Send empty message part
        /// </summary>
        public void SendMore() {
            Send(new byte[0], SendRecvOpt.SNDMORE);
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
                return _address;
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
                if (C.zmq_device((int)device, inSocket._ptr, outSocket._ptr) != 0)
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