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
using SysSockets = System.Net.Sockets;

namespace ZMQ {
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
#pragma warning disable 414 //Silence variable not used warnings
        private IntPtr socket;
#if x86 || POSIX
        private int fd;
#else
        private long fd;
#endif
        private short events;
#pragma warning restore //Restore full warnings
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
                IOMultiPlex.POLLIN) {
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
}
