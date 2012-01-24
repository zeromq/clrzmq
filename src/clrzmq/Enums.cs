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
        RECOVERY_IVL_MSEC = 20,
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
        DEALER = 5,
        ROUTER = 6,
        PULL = 7,
        PUSH = 8,
        XPUB = 9,
        XSUB = 10,
        XREQ = DEALER,
        XREP = ROUTER,
        [Obsolete("To be removed in 3.x. Use PULL instead.")]
        UPSTREAM = PULL,
        [Obsolete("To be removed in 3.x. Use PUSH instead.")]
        DOWNSTREAM = PUSH
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
    /// Specifies possible results for socket send operations.
    /// </summary>
    public enum SendStatus {
        /// <summary>
        /// The message was queued to be sent by the socket.
        /// </summary>
        Sent,

        /// <summary>
        /// Non-blocking mode was requested and the message cannot be sent at the moment.
        /// </summary>
        TryAgain,

        /// <summary>
        /// The send operation was interrupted, likely by terminating the containing context.
        /// </summary>
        Interrupted
    }

    /// <summary>
    /// IO Multiplexing polling events bit flags
    /// </summary>
    [Flags]
    public enum IOMultiPlex {
        POLLIN = 0x1,
        POLLOUT = 0x2,
        POLLERR = 0x4
    }

    public enum ERRNOS {
        // Standard Posix error codes
        EPERM = 1,
        ENOENT = 2,
        ESRCH = 3,
        EINTR = 4,
        EIO = 5,
        ENXIO = 6,
        E2BIG = 7,
        ENOEXEC = 8,
        EBADF = 9,
        ECHILD = 10,
        EAGAIN = 11,
        ENOMEM = 12,
        EACCES = 13,
        EFAULT = 14,
        EBUSY = 16,
        EEXIST = 17,
        EXDEV = 18,
        ENODEV = 19,
        ENOTDIR = 20,
        EISDIR = 21,
        ENFILE = 23,
        EMFILE = 24,
        ENOTTY = 25,
        EFBIG = 27,
        ENOSPC = 28,
        ESPIPE = 29,
        EROFS = 30,
        EMLINK = 31,
        EPIPE = 32,
        EDOM = 33,
        EDEADLK = 36,
        ENAMETOOLONG = 38,
        ENOLCK = 39,
        ENOSYS = 40,
        ENOTEMPTY = 41,

        // TODO: These will likely be wrong on all platforms. Find a way to set these properly.
        ENOTSUP = ZHelpers.HAUSNUMERO + 1,
        EPROTONOSUPPORT,
        ENOBUFS,
        ENETDOWN,
        EADDRINUSE,
        EADDRNOTAVAIL,
        ECONNREFUSED,
        EINPROGRESS,
        ENOTSOCK,

        // Native ZMQ error codes
        EFSM = ZHelpers.HAUSNUMERO + 51,
        ENOCOMPATPROTO,
        ETERM,
        EMTHREAD
    }
}
