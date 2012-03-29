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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SysSockets = System.Net.Sockets;

namespace ZMQ {
    /// <summary>
    /// ZMQ Socket
    /// </summary>
    public class Socket : IDisposable {
        private static Context _appContext;
        private static int _appSocketCount;
        private static readonly Object _lockObj = new object();

#if !PocketPC
        private readonly int _processorCount;
#endif

        private PollItem _pollItem;
        private bool _localSocket;
        private IntPtr _msg;
        private string _address;

        //  Figure out size of zmq_msg_t structure.
        //  It's size of pointer + 2 bytes + VSM buffer size.
        private const int ZMQ_MAX_VSM_SIZE = 30;
        private readonly int ZMQ_MSG_T_SIZE = IntPtr.Size + 2 + ZMQ_MAX_VSM_SIZE;

        /// <summary>
        /// This constructor should not be called directly, use the Context
        /// Socket method
        /// </summary>
        /// <param name="ptr">Pointer to a socket</param>
        internal Socket(IntPtr ptr) {
            Ptr = ptr;
            CommonInit(false);
#if !PocketPC
            _processorCount = Environment.ProcessorCount;
#endif
        }

        /// <summary>
        /// Create Socket using application wide Context. OBSOLETE: use <see cref="Context.Socket"/> and
        /// avoid using application-wide Context objects.
        /// </summary>
        /// <param name="type">Socket type</param>
        [Obsolete("Sockets should be constructed using Context.Socket. Will be removed in 3.x.")]
        public Socket(SocketType type) {
            lock (_lockObj) {
                if (_appContext == null) {
                    _appContext = new Context();
                }
                Ptr = _appContext.CreateSocketPtr(type);
            }
            Interlocked.Increment(ref _appSocketCount);
            CommonInit(true);
        }

        private void CommonInit(bool local) {
            _msg = Marshal.AllocHGlobal(ZMQ_MSG_T_SIZE);
            _localSocket = local;
            _pollItem = new PollItem(new ZMQPollItem(Ptr, 0, 0), this);
        }

        ~Socket() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal IntPtr Ptr { get; private set; }

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

            if (Ptr != IntPtr.Zero) {
                int rc = C.zmq_close(Ptr);
                Ptr = IntPtr.Zero;
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

#if x86 || PocketPC
        /// <summary>
        /// Allows cross platform reading of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_t</param>
        /// <returns>Size_t value</returns>
        private static object ReadSizeT(IntPtr ptr) {
            return unchecked((uint)Marshal.ReadInt32(ptr));
        }

        /// <summary>
        /// Allows cross platform writing of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_</param>
        /// <param name="val">Value to write</param>
        private static void WriteSizeT(IntPtr ptr, object val) {
            Marshal.WriteInt32(ptr, unchecked(Convert.ToInt32(val)));
        }
#elif x64
        /// <summary>
        /// Allows cross platform reading of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_t</param>
        /// <returns>Size_t value</returns>
        private static object ReadSizeT(IntPtr ptr) {
            return unchecked((ulong)Marshal.ReadInt64(ptr));
        }

        /// <summary>
        /// Allows cross platform writing of size_t
        /// </summary>
        /// <param name="ptr">Pointer to a size_</param>
        /// <param name="val">Value to write</param>
        private static void WriteSizeT(IntPtr ptr, object val) {
            Marshal.WriteInt64(ptr, unchecked(Convert.ToInt64(val)));
        }
#endif
        /// <summary>
        /// Create poll item for ZMQ socket listening, for the supplied events
        /// </summary>
        /// <param name="events">Listening events</param>
        /// <returns>Socket Poll item</returns>
        public PollItem CreatePollItem(IOMultiPlex events) {
            return new PollItem(new ZMQPollItem(Ptr, 0, (short)events), this);
        }

        /// <summary>
        /// Create poll item for ZMQ and plain socket listening, for the supplied events
        /// </summary>
        /// <param name="events">Listening events</param>
        /// <param name="sysSocket">Raw Socket</param>
        /// <returns>Socket Poll item</returns>
        public PollItem CreatePollItem(IOMultiPlex events, SysSockets.Socket sysSocket) {
            if (sysSocket == null) {
                throw new ArgumentNullException("sysSocket");
            }

#if x86 || POSIX || PocketPC
            return new PollItem(new ZMQPollItem(Ptr, sysSocket.Handle.ToInt32(), (short)events), this);
#elif x64
            return new PollItem(new ZMQPollItem(Ptr, sysSocket.Handle.ToInt64(), (short)events), this);
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
            using (var valPtr = new DisposableIntPtr(sizeOfValue)) {
                Marshal.WriteInt64(valPtr.Ptr, unchecked(Convert.ToInt64(value)));
                if (C.zmq_setsockopt(Ptr, (int)option, valPtr.Ptr, sizeOfValue) != 0)
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
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            using (var valPtr = new DisposableIntPtr(value.Length)) {
                Marshal.Copy(value, 0, valPtr.Ptr, value.Length);
                if (C.zmq_setsockopt(Ptr, (int)option, valPtr.Ptr, value.Length) != 0)
                    throw new Exception();
            }
        }

        /// <summary>
        /// Set Socket Option
        /// </summary>
        /// <param name="option">Socket Option</param>
        /// <param name="value">Option value</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void SetSockOpt(SocketOpt option, int value) {
            int sizeOfValue = Marshal.SizeOf(typeof(int));
            using (var valPtr = new DisposableIntPtr(sizeOfValue)) {
                Marshal.WriteInt32(valPtr.Ptr, Convert.ToInt32(value));
                if (C.zmq_setsockopt(Ptr, (int)option, valPtr.Ptr, sizeOfValue) != 0)
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
            using (var valPtr = new DisposableIntPtr(sizeOfValue)) {
                Marshal.WriteInt64(valPtr.Ptr, Convert.ToInt64(value));
                if (C.zmq_setsockopt(Ptr, (int)option, valPtr.Ptr, sizeOfValue) != 0)
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
            using (var len = new DisposableIntPtr(IntPtr.Size)) {
                if (option == SocketOpt.IDENTITY) {
                    using (var val = new DisposableIntPtr(IDLenSize)) {
                        WriteSizeT(len.Ptr, IDLenSize);
                        if (C.zmq_getsockopt(Ptr, (int)option, val.Ptr, len.Ptr) != 0)
                            throw new Exception();
                        var buffer = new byte[Convert.ToInt32(ReadSizeT(len.Ptr))];
                        Marshal.Copy(val.Ptr, buffer, 0, buffer.Length);
                        output = buffer;
                    }
                }
                else {
                    using (var val = new DisposableIntPtr(lenSize)) {
                        WriteSizeT(len.Ptr, lenSize);
                        if (C.zmq_getsockopt(Ptr, (int)option, val.Ptr, len.Ptr) != 0)
                            throw new Exception();

                        switch (option) {
                            case SocketOpt.HWM:
                            case SocketOpt.AFFINITY:
                            case SocketOpt.SNDBUF:
                            case SocketOpt.RCVBUF:
                                //Unchecked casting of uint64 options
                                output = unchecked((ulong)Marshal.ReadInt64(val.Ptr));
                                break;
                            case SocketOpt.LINGER:
                            case SocketOpt.BACKLOG:
                            case SocketOpt.RECONNECT_IVL:
                            case SocketOpt.RECONNECT_IVL_MAX:
                                output = Marshal.ReadInt32(val.Ptr);
                                break;
                            case SocketOpt.EVENTS:
                                output = unchecked((uint)Marshal.ReadInt32(val.Ptr));
                                break;
                            case SocketOpt.FD:
#if POSIX
                                output = Marshal.ReadInt32(val.Ptr);
#else
                                output = Marshal.ReadIntPtr(val.Ptr);
#endif
                                break;
                            default:
                                output = Marshal.ReadInt64(val.Ptr);
                                break;
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
            if (addr == null) {
                throw new ArgumentNullException("addr");
            }

            _address = addr;
#if PocketPC
			if (C.zmq_bind(Ptr, Encoding.ASCII.GetBytes(addr)) != 0)
#else
            if (C.zmq_bind(Ptr, addr) != 0)
#endif
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
            if (addr == null) {
                throw new ArgumentNullException("addr");
            }

			Bind(GetTransportName(transport) + "://" + addr + ":" + port);
        }

        /// <summary>
        /// Bind the socket to address
        /// </summary>
        /// <param name="transport">Socket transport type</param>
        /// <param name="addr">Socket address</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Bind(Transport transport, string addr) {
            if (addr == null) {
                throw new ArgumentNullException("addr");
            }

			Bind(GetTransportName(transport) + "://" + addr);
        }

        /// <summary>
        /// Connect socket to destination address
        /// </summary>
        /// <param name="addr">Destination Address</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Connect(string addr) {
            if (addr == null) {
                throw new ArgumentNullException("addr");
            }

            _address = addr;
#if PocketPC
			if (C.zmq_connect(Ptr, Encoding.ASCII.GetBytes(addr)) != 0)
#else
            if (C.zmq_connect(Ptr, addr) != 0)
#endif
			throw new Exception();
        }

        /// <summary>
        /// Connect socket to destination address
        /// </summary>
        /// <param name="transport">Socket transport type</param>
        /// <param name="addr">Socket address</param>
        /// <param name="port">Socket port</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Connect(Transport transport, string addr, uint port) {
            if (addr == null) {
                throw new ArgumentNullException("addr");
            }

			Connect(GetTransportName(transport) + "://" + addr + ":" + port);
        }

        /// <summary>
        /// Connect socket to destination address
        /// </summary>
        /// <param name="transport">Socket transport type</param>
        /// <param name="addr">Socket address</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Connect(Transport transport, string addr) {
            if (addr == null) {
                throw new ArgumentNullException("addr");
            }

			Connect(GetTransportName(transport) + "://" + addr);
        }

		private static string GetTransportName(Transport tr)
		{
#if PocketPC
			switch (tr)
			{
				case Transport.TCP:
					return "tcp";
				case Transport.PGM:
					return "pgm";
				case Transport.IPC:
					return "ipc";
				case Transport.INPROC:
					return "inproc";
				case Transport.EPGM:
					return "epgm";
				default:
					throw new ArgumentException("Unexpected transport.", "tr");
			}
#else
			return Enum.GetName(typeof(Transport), tr).ToLower();
#endif
		}

        /// <summary>
        /// Forward all message parts directly to destination. No marshalling performed.
        /// </summary>
        /// <param name="destination">Destination Socket</param>
        public void Forward(Socket destination) {
            if (destination == null) {
                throw new ArgumentNullException("destination");
            }

            SendRecvOpt opt = SendRecvOpt.SNDMORE;
            while (opt == SendRecvOpt.SNDMORE) {
                if (C.zmq_msg_init(_msg) != 0)
                    throw new Exception();
                if (C.zmq_recv(Ptr, _msg, 0) == 0) {
                    opt = RcvMore ? SendRecvOpt.SNDMORE : SendRecvOpt.NONE;
                    if (C.zmq_send(destination.Ptr, _msg, (int)opt) != 0)
                        throw new Exception();
                    C.zmq_msg_close(_msg);
                }
                else {
                    if (C.zmq_errno() != (int)ERRNOS.EAGAIN)
                        throw new Exception();
                }
            }
        }

        /// <summary>
        /// Listen for message
        /// </summary>
        /// <param name="message">Message buffer</param>
        /// <param name="size">The size of the read message</param>
        /// <param name="flags">Receive Options</param>
        /// <returns>Message</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public byte[] Recv(byte[] message, out int size, params SendRecvOpt[] flags) {
            size = -1;
            int flagsVal = 0;
            foreach (SendRecvOpt opt in flags) {
                flagsVal += (int)opt;
            }
            if (C.zmq_msg_init(_msg) != 0)
                throw new Exception();
            while (true) {
                if (C.zmq_recv(Ptr, _msg, flagsVal) == 0) {
                    size = C.zmq_msg_size(_msg);

                    if (message == null || size > message.Length) {
                        message = new byte[size];
                    }

                    Marshal.Copy(C.zmq_msg_data(_msg), message, 0, size);
                    C.zmq_msg_close(_msg);
                    break;
                }
                if (C.zmq_errno() == (int)ERRNOS.EINTR) {
                    continue;
                }
                if (C.zmq_errno() != (int)ERRNOS.EAGAIN) {
                    throw new Exception();
                }
                break;
            }
            return message;
        }

        /// <summary>
        /// Listen for message
        /// </summary>
        /// <param name="flags">Receive Options</param>
        /// <returns>Message</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public byte[] Recv(params SendRecvOpt[] flags) {
            int size;
            return Recv(null, out size, flags);
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
            byte[] data = null;
            int iterations = 0;
            var timer = Stopwatch.StartNew();

            while (timer.ElapsedMilliseconds <= timeout) {
                data = Recv(SendRecvOpt.NOBLOCK);

                if (data == null) {
                    if (timeout > 1) {
#if !PocketPC
                        if (iterations < 20 && _processorCount > 1) {
                            // If we have a short wait (< 20 iterations) we
                            // SpinWait to allow other threads on HT CPUs
                            // to use the CPU, the more CPUs we have
                            // the longer it's "ok" to spin wait since
                            // we stall the overall system less
                            Thread.SpinWait(100 * _processorCount);
                        }
                        else {
#endif
                            // Yield my remaining time slice to another thread
#if NET_4
                            Thread.Yield();
#else
                            Thread.Sleep(0);
#endif
#if !PocketPC
                        }
#endif
                    }
                }
                else {
                    break;
                }

                ++iterations;
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
            byte[] data = Recv(timeout);
            if (data == null) {
                return null;
            }
            return encoding.GetString(data, 0, data.Length);
        }

        /// <summary>
        /// Listen for message, and return it in string format
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <param name="flags">Receive options</param>
        /// <returns>Message string</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public string Recv(Encoding encoding, params SendRecvOpt[] flags) {
            byte[] data = Recv(flags);
            if (data == null) {
                return null;
            }
            return encoding.GetString(data, 0, data.Length);
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts
        /// </summary>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<byte[]> RecvAll() {
            return RecvAll((Queue<byte[]>)null);
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts
        /// </summary>
        /// <param name="messages">The queue object to put the message into</param>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<byte[]> RecvAll(Queue<byte[]> messages) {
            if (messages == null) {
                messages = new Queue<byte[]>();
            }

            messages.Enqueue(Recv());
            while (RcvMore) {
                messages.Enqueue(Recv());
            }
            return messages;
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts.
        /// </summary>
        /// <param name="flags">Receive options</param>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<byte[]> RecvAll(params SendRecvOpt[] flags) {
            return RecvAll((Queue<byte[]>)null, flags);
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts.
        /// </summary>
        /// <param name="messages">The queue object to put the message into</param>
        /// <param name="flags">Receive options</param>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<byte[]> RecvAll(Queue<byte[]> messages, params SendRecvOpt[] flags) {
            if (messages == null) {
                messages = new Queue<byte[]>();
            }

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
            var messages = new Queue<string>();
            messages.Enqueue(Recv(encoding));
            while (RcvMore) {
                messages.Enqueue(Recv(encoding));
            }
            return messages;
        }

        /// <summary>
        /// Listen for message, retrieving all pending message parts.
        /// </summary>
        /// <param name="encoding">String Encoding</param>
        /// <param name="flags">Socket options to use when receiving</param>
        /// <returns>Queue of message parts</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public Queue<string> RecvAll(Encoding encoding, params SendRecvOpt[] flags) {
            var messages = new Queue<string>();
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
        /// <param name="length">Length of data to send from message</param>
        /// <param name="flags">Send Options</param>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus Send(byte[] message, int length, params SendRecvOpt[] flags) {
            return Send(message, 0, length, flags);
        }

        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="startIndex">Index to start reading data from</param>
        /// <param name="length">Length of data to send from message, starting at startIndex</param>
        /// <param name="flags">Send Options</param>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus Send(byte[] message, int startIndex, int length, params SendRecvOpt[] flags) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }

            if ((startIndex + length) > message.Length) {
                throw new InvalidOperationException("The value of startIndex + length must not exceed message.Length.");
            }

            int flagsVal = 0;

            foreach (SendRecvOpt opt in flags) {
                flagsVal |= (int)opt;
            }

            if (C.zmq_msg_init_size(_msg, length) != 0) {
                throw new Exception();
            }

            Marshal.Copy(message, startIndex, C.zmq_msg_data(_msg), length);

            int rc = C.zmq_send(Ptr, _msg, flagsVal);

            if (C.zmq_msg_close(_msg) != 0) {
                throw new Exception();
            }

            if (rc >= 0) {
                return SendStatus.Sent;
            }

            int errno = C.zmq_errno();

            if (errno == (int)ERRNOS.EAGAIN) {
                return SendStatus.TryAgain;
            }

            if (errno == (int)ERRNOS.EINTR) {
                return SendStatus.Interrupted;
            }

            throw new Exception();
        }

        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="flags">Send Options</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus Send(byte[] message, params SendRecvOpt[] flags) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }

            return Send(message, 0, message.Length, flags);
        }

        /// <summary>
        /// Send Message.
        /// </summary>
        /// <param name="message">Message</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus Send(byte[] message) {
            return Send(message, SendRecvOpt.NONE);
        }

        /// <summary>
        /// Send Message.
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="encoding">String encoding</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus Send(string message, Encoding encoding) {
            return Send(message, encoding, SendRecvOpt.NONE);
        }

        /// <summary>
        /// Send empty message part
        /// </summary>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus Send() {
            return Send(new byte[0]);
        }

        /// <summary>
        /// Send empty message part
        /// </summary>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus SendMore() {
            return Send(new byte[0], SendRecvOpt.SNDMORE);
        }

        /// <summary>
        /// Send multi-part message, holding for further message parts.
        /// </summary>
        /// <param name="message">Message</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus SendMore(byte[] message) {
            return Send(message, SendRecvOpt.SNDMORE);
        }

        /// <summary>
        /// Send multi-part message, holding for further message parts.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encoding">String encoding</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus SendMore(string message, Encoding encoding) {
            return Send(message, encoding, SendRecvOpt.SNDMORE);
        }

        /// <summary>
        /// Send multi-part message, holding for further message parts.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encoding">String encoding</param>
        /// <param name="flags">Send options</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus SendMore(string message, Encoding encoding, params SendRecvOpt[] flags) {
            return Send(message, encoding, flags);
        }

        /// <summary>
        /// Send Message.
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="encoding">Encoding to use when sending</param>
        /// <param name="flags">Send Options</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        /// <returns>A <see cref="SendStatus"/> value indicating the outcome of the Send operation.</returns>
        public SendStatus Send(string message, Encoding encoding, params SendRecvOpt[] flags) {
            return Send(encoding.GetBytes(message), flags);
        }

        /// <summary>
        /// Retrieve identity as a string
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <returns>Socket Identity</returns>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public string IdentityToString(Encoding encoding) {
            return encoding.GetString(Identity, 0, Identity.Length);
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
        /// Gets or Sets socket recovery interval in milliseconds
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public long RecoveryIvlMsec {
            get {
                return (long)GetSockOpt(SocketOpt.RECOVERY_IVL_MSEC);
            }
            set {
                SetSockOpt(SocketOpt.RECOVERY_IVL_MSEC, value);
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
        /// Get or Set linger period for socket shutdown
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public int Linger {
            get {
                return (int)GetSockOpt(SocketOpt.LINGER);
            }
            set {
                SetSockOpt(SocketOpt.LINGER, value);
            }
        }

        /// <summary>
        /// Get or Set reconnection interval
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public int ReconnectIvl {
            get {
                return (int)GetSockOpt(SocketOpt.RECONNECT_IVL);
            }
            set {
                SetSockOpt(SocketOpt.RECONNECT_IVL, value);
            }
        }

        /// <summary>
        /// Get or Set maximum reconnection interval
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public int ReconnectIvlMax {
            get {
                return (int)GetSockOpt(SocketOpt.RECONNECT_IVL_MAX);
            }
            set {
                SetSockOpt(SocketOpt.RECONNECT_IVL_MAX, value);
            }
        }

        /// <summary>
        /// Get or Set maximum length of the queue of outstanding connections
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public int Backlog {
            get {
                return (int)GetSockOpt(SocketOpt.BACKLOG);
            }
            set {
                SetSockOpt(SocketOpt.BACKLOG, value);
            }
        }

#if POSIX
        /// <summary>
        /// Get Socket File descriptor
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public int FD {
            get {
                return (int)GetSockOpt(SocketOpt.FD);
            }
        }
#else
        /// <summary>
        /// Get Socket winsock HANDLE
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public IntPtr FD {
            get {
                return (IntPtr)GetSockOpt(SocketOpt.FD);
            }
        }
#endif

        /// <summary>
        /// Get socket event state
        /// </summary>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public IOMultiPlex[] Events {
            get {
                var evts = (uint)GetSockOpt(SocketOpt.EVENTS);
                var evtList = new List<IOMultiPlex>();
                if (((int)IOMultiPlex.POLLIN & evts) > 0) {
                    evtList.Add(IOMultiPlex.POLLIN);
                }
                if (((int)IOMultiPlex.POLLOUT & evts) > 0) {
                    evtList.Add(IOMultiPlex.POLLOUT);
                }
                return evtList.ToArray();
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
        /// <param name="encoding">Encoding to use when setting the filter</param>
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
        /// <param name="encoding">Encoding to use when removing the filter</param>
        /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
        public void Unsubscribe(string filter, Encoding encoding) {
            SetSockOpt(SocketOpt.UNSUBSCRIBE, encoding.GetBytes(filter));
        }

        public bool CheckEvent(IOMultiPlex revent) {
            return _pollItem.CheckEvent(revent);
        }

        /// <summary>
        /// ZMQ Device creation
        /// </summary>
        [Obsolete("zmq_device support will be removed in 3.x.")]
        public static class Device {
            /// <summary>
            /// Create ZMQ Device
            /// </summary>
            /// <param name="device">Device type</param>
            /// <param name="inSocket">Input socket</param>
            /// <param name="outSocket">Output socket</param>
            /// <exception cref="ZMQ.Exception">ZMQ Exception</exception>
            public static void Create(DeviceType device, Socket inSocket, Socket outSocket) {
                int rc = C.zmq_device((int)device, inSocket.Ptr, outSocket.Ptr);

                if (rc == -1 && C.zmq_errno() != (int)ERRNOS.ETERM)
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
