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
#pragma warning disable 414 // ReSharper disable UnaccessedField.Local
        private IntPtr _socket;
#if x86 || POSIX
        private int _fd;
#else
        private long _fd;
#endif
        private short _events;
#pragma warning restore // ReSharper restore UnaccessedField.Local
        private short _revents;

#if x86 || POSIX
        /// <summary>
        /// Do not call directly
        /// </summary>
        /// <param name="socket">Target ZMQ socket ptr</param>
        /// <param name="fd">Non ZMQ socket (Experimental)</param>
        /// <param name="events">Desired events</param>
        internal ZMQPollItem(IntPtr socket, object fd, short events) {
            _socket = socket;
            _events = events;
            _revents = 0;
            _fd = Convert.ToInt32(fd);
        }
#else
        /// <summary>
        /// Do not call directly
        /// </summary>
        /// <param name="socket">Target ZMQ socket ptr</param>
        /// <param name="fd">Non ZMQ socket (Experimental)</param>
        /// <param name="events">Desired events</param>
        internal ZMQPollItem(IntPtr socket, object fd, short events) {
            _socket = socket;
            _events = events;
            _revents = 0;
            _fd = unchecked(Convert.ToInt64(fd));
        }
#endif

        /// <summary>
        /// Reset revents so that poll item can be safely reused
        /// </summary>
        public void ResetRevents() {
            _revents = 0;
        }

        /// <summary>
        /// Get returned event flags
        /// </summary>
        public IOMultiPlex Revents {
            get { return (IOMultiPlex)_revents; }
        }

        internal void ActivateEvent(params IOMultiPlex[] events) {
            foreach (IOMultiPlex evt in events) {
                _events = (short)(_events | (short)evt);
            }
        }

        internal void DeactivateEvent(params IOMultiPlex[] events) {
            foreach (IOMultiPlex evt in events) {
                _events &= (short)evt;
            } 
        }
    }

    /// <summary>
    /// Polling item, provides the polling mechanism
    /// </summary>
    public class PollItem {
        private readonly Socket _socket;

        private ZMQPollItem _zmqPollItem;

        private event PollHandler _PollInHandlers;
        private event PollHandler _PollOutHandlers;
        private event PollHandler _PollErrHandlers;

        /// <summary>
        /// Should not be created directly, use Socket.CreatePollItem
        /// </summary>
        /// <param name="zmqPollItem">Native poll item</param>
        /// <param name="socket">Socket to poll</param>
        internal PollItem(ZMQPollItem zmqPollItem, Socket socket) {
            _socket = socket;
            _zmqPollItem = zmqPollItem;
        }

        /// <summary>
        /// POLLIN event handler
        /// </summary>
        public event PollHandler PollInHandler {
            add {
                _PollInHandlers += value;
                _zmqPollItem.ActivateEvent(IOMultiPlex.POLLIN);
            }
            remove {
                if (_PollInHandlers.GetInvocationList().Length <= 0) {
                    _zmqPollItem.DeactivateEvent(IOMultiPlex.POLLIN);
                }
                _PollInHandlers -= value;
            }
        }

        /// <summary>
        /// POLLOUT event handler
        /// </summary>
        public event PollHandler PollOutHandler {
            add {
                _zmqPollItem.ActivateEvent(IOMultiPlex.POLLOUT);
                _PollOutHandlers += value;
            }
            remove {
                if (_PollOutHandlers.GetInvocationList().Length <= 0) {
                    _zmqPollItem.DeactivateEvent(IOMultiPlex.POLLOUT);
                }
                _PollOutHandlers -= value;
            }
        }

        /// <summary>
        /// POLLERR event handler
        /// </summary>
        public event PollHandler PollErrHandler {
            add {
                ZMQPollItem.ActivateEvent(IOMultiPlex.POLLERR);
                _PollErrHandlers += value;
            }
            remove {
                if (_PollErrHandlers.GetInvocationList().Length <= 0) {
                    ZMQPollItem.DeactivateEvent(IOMultiPlex.POLLERR);
                }
                _PollErrHandlers -= value;
            }
        }

        /// <summary>
        /// Fire handlers for any returned events
        /// </summary>
        public void FireEvents() {
            if (_PollInHandlers != null &&
                (_zmqPollItem.Revents & IOMultiPlex.POLLIN) ==
                IOMultiPlex.POLLIN) {
                _PollInHandlers(_socket, _zmqPollItem.Revents);
            }
            if (_PollOutHandlers != null &&
                (_zmqPollItem.Revents & IOMultiPlex.POLLOUT) ==
                IOMultiPlex.POLLOUT) {
                _PollOutHandlers(_socket, _zmqPollItem.Revents);
            }
            if (_PollErrHandlers != null &&
                (_zmqPollItem.Revents & IOMultiPlex.POLLERR) ==
                IOMultiPlex.POLLERR) {
                _PollErrHandlers(_socket, _zmqPollItem.Revents);
            }
        }

        public bool CheckEvent(IOMultiPlex revent) {
            return (_zmqPollItem.Revents & revent) > 0;
        }

        /// <summary>
        /// Get and Set ZMQ poll item
        /// </summary>
        public ZMQPollItem ZMQPollItem {
            get { return _zmqPollItem; }
            set { _zmqPollItem = value; }
        }
    }
}
