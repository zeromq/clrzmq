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
        BACKLOG = 19,
        RECONNECT_IVL_MAX = 21,
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
        [Obsolete("To be removed in 3.x. Use DEALER instead.")]
        XREQ = 5,
        DEALER = 5,
        [Obsolete("To be removed in 3.x. Use ROUTER instead.")]
        XREP = 6,
        ROUTER = 6,
        PULL = 7,
        [Obsolete("To be removed in 3.x. Use PULL instead.")]
        UPSTREAM = 7,
        PUSH = 8,
        [Obsolete("To be removed in 3.x. Use PUSH instead.")]
        DOWNSTREAM = 8
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

    public enum ERRNOS {
        ENOTSUP = ZHelpers.HAUSNUMERO,
        EPROTONOSUPPORT,
        ENOBUFS,
        ENETDOWN,
        EADDRINUSE,
        EADDRNOTAVAIL,
        ECONNREFUSED,
        EINPROGRESS,
        EFSM = ZHelpers.HAUSNUMERO + 51,
        ENOCOMPATPROTO,
        ETERM,
        EMTHREAD
    }
}
