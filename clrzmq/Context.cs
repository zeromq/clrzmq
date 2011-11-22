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
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
#if POSIX
using Mono.Posix;
using Mono.Unix;
#endif

namespace ZMQ {
#if POSIX
    public static class SignalHandler {
        private static object _sync = new object();
        private static UnixSignal[] _signals;
        private static Thread _signalThread;

        private static void Initialize() {
            _signals = new UnixSignal[] {
                new UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
                new UnixSignal(Mono.Unix.Native.Signum.SIGINT)
            };
            _signalThread = new Thread(delegate () {
                UnixSignal.WaitAny(_signals, -1);
                System.Environment.Exit(-1);
            });
            _signalThread.Start();
        }

        public static void StartHandler() {
            lock (_sync){
                if(_signals == null){
                    Initialize();
                }
            }
        }
    }
#endif

    /// <summary>
    /// ZMQ Context
    /// </summary>
    public class Context : IDisposable {
        private static int _defaultIOThreads = 1;
        private IntPtr _ptr;


        /// <summary>
        /// Create ZMQ Context
        /// </summary>
        /// <param name="io_threads">Thread pool size</param>
        public Context(int io_threads) {
#if POSIX
            SignalHandler.StartHandler();
#endif
            _ptr = C.zmq_init(io_threads);
            if (_ptr == IntPtr.Zero)
                throw new Exception();
        }

        public Context() {
#if POSIX
            SignalHandler.StartHandler();
#endif
            _ptr = C.zmq_init(_defaultIOThreads);
            if (_ptr == IntPtr.Zero)
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
            return new Socket(CreateSocketPtr(type));
        }

        internal IntPtr CreateSocketPtr(SocketType type) {
            IntPtr socket_ptr = C.zmq_socket(_ptr, (int)type);
            if (socket_ptr == IntPtr.Zero)
                throw new Exception();
            return socket_ptr;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_ptr != IntPtr.Zero) {
                int rc = C.zmq_term(_ptr);
                _ptr = IntPtr.Zero;
                if (rc != 0)
                    throw new Exception();
            }
        }

        public static int DefaultIOThreads {
            get { return _defaultIOThreads; }
            set { _defaultIOThreads = value; }
        }

        /// <summary>
        /// Polls the supplied items for events.
        /// </summary>
        /// <param name="items">Items to Poll</param>
        /// <param name="timeout">Timeout(micro seconds)</param>
        /// <returns>Number of Poll items with events</returns>
        public int Poll(PollItem[] items, long timeout) {
            return Poller(items, timeout);
        }

        /// <summary>
        /// Polls the supplied items for events, with infinite timeout.
        /// </summary>
        /// <param name="items">Items to Poll</param>
        /// <returns>Number of Poll items with events</returns>
        public int Poll(PollItem[] items) {
            return Poll(items, -1);
        }


        /// <summary>
        /// Polls the supplied items for events. Static method.
        /// </summary>
        /// <param name="items">Items to Poll</param>
        /// <param name="timeout">Timeout(micro seconds)</param>
        /// <returns>Number of Poll items with events</returns>
        public static int Poller(PollItem[] items, long timeout) {
            Stopwatch spentTimeout = new Stopwatch();
            int rc = -1;
            if (timeout >= 0) {
                spentTimeout.Start();
            }
            while (rc != 0) {
                ZMQPollItem[] zitems = new ZMQPollItem[items.Length];
                int index = 0;
                foreach (PollItem item in items) {
                    item.ZMQPollItem.ResetRevents();
                    zitems[index] = item.ZMQPollItem;
                    index++;
                }
                rc = C.zmq_poll(zitems, items.Length, timeout);
                if (rc > 0) {
                    for (index = 0; index < items.Length; index++) {
                        items[index].ZMQPollItem = zitems[index];
                        items[index].FireEvents();
                    }
                    break;
                } else if (rc < 0) {
                    if (ZMQ.C.zmq_errno() == 4) {
                        if (spentTimeout.IsRunning) {
                            long elapsed = spentTimeout.ElapsedMilliseconds * 1000;
                            if (timeout < elapsed) {
                                break;
                            } else {
                                timeout -= elapsed;
                                continue;
                            }
                        } else {
                            continue;
                        }
                    }
                    throw new Exception();
                }
            }
            return rc;
        }

        /// <summary>
        /// Polls the supplied items for events, with infinite timeout.
        /// Static method
        /// </summary>
        /// <param name="items">Items to Poll</param>
        /// <returns>Number of Poll items with events</returns>
        public static int Poller(PollItem[] items) {
            return Poller(items, -1);
        }

        public static int Poller(params Socket[] sockets) {
            return Poller(new List<Socket>(sockets));
        }

        public static int Poller(long timeout, params Socket[] sockets) {
            return Poller(new List<Socket>(sockets), timeout);
        }

        /// <summary>
        /// Polls the supplied sockets (sockets need to have events set).
        /// Static method
        /// </summary>
        /// <param name="skts">Socket List</param>
        /// <param name="timeout">Timeout(micro seconds)</param>
        /// <returns>Number of Poll items with events</returns>
        public static int Poller(IList<Socket> skts, long timeout) {
            List<PollItem> items = new List<PollItem>(skts.Count);
            foreach (Socket skt in skts) {
                items.Add(skt.PollItem);
            }
            return Poller(items.ToArray(), timeout);
        }

        /// <summary>
        /// Polls the supplied sockets (sockets need to have events set),
        /// infinite timeout. Static method.
        /// </summary>
        /// <param name="skts">Socket List</param>
        /// <returns>Number of Poll items with events</returns>
        public static int Poller(IList<Socket> skts) {
            return Poller(skts, -1);
        }
    }
}
