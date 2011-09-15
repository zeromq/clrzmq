/*

    Copyright (c) 2010 Jeffrey Dik <s450r1@gmail.com>
    Copyright (c) 2010 Martin Sustrik <sustrik@250bpm.com>
    Copyright (c) 2010 Michael Compton <michael.compton@littleedge.co.uk>
    Copyright (c) 2011 Calvin de Vries <devries.calvin@gmail.com>

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

namespace ZMQ
{
    /// <summary>
    /// Message transport types
    /// </summary>
    public enum Transport
    {
        INPROC = 1,
        TCP = 2,
        IPC = 3,
        PGM = 4,
        EPGM = 5
    }

    /// <summary>
    /// Socket options
    /// </summary>
    public enum SocketOpt
    {
        AFFINITY = 4,
        IDENTITY = 5,
        SUBSCRIBE = 6,
        UNSUBSCRIBE = 7,
        RATE = 8,
        RECOVERY_IVL = 9,
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
        MAXMSGSIZE = 22,
        SNDHWM = 23,
        RCVHWM = 24,
        MULTICAST_HOPS = 25,
        RCVTIME0 = 27,
        SNDTIME0 = 28,
        RCVLABEL = 29
    }

    /// <summary>
    /// Socket types
    /// </summary>
    public enum SocketType
    {
        PAIR = 0,
        PUB = 1,
        SUB = 2,
        REQ = 3,
        REP = 4,
        [Obsolete("To be removed in 3.x. Use DEALER instead.")]
        XREQ = 5,
        [Obsolete("To be removed in 3.x. Use ROUTER instead.")]
        XREP = 6,
        PULL = 7,
        PUSH = 8,
        XPUB = 9,
        XSUB = 10,
        ZMQ_ROUTER = 11,
        ZMQ_DEALER = 12
    }

    /// <summary>
    /// Device types
    /// </summary>
    public enum DeviceType
    {
        STREAMER = 1,
        FORWARDER = 2,
        QUEUE = 3
    }

    /// <summary>
    /// Send and receive options
    /// </summary>
    public enum SendRecvOpt
    {
        NONE = 0,
        DONTWAIT = 1,
        SNDMORE = 2,
        SNDLABEL = 4
    }

    /// <summary>
    /// IO Multiplexing polling events bit flags
    /// </summary>
    public enum IOMultiPlex
    {
        POLLIN = 0x1,
        POLLOUT = 0x2,
        POLLERR = 0x4
    }

    public enum ERRNOS
    {
        ENOTSUP = ZHelpers.HAUSNUMERO + 1,
        EPROTONOSUPPORT = ZHelpers.HAUSNUMERO + 2,
        ENOBUFS = ZHelpers.HAUSNUMERO + 3,
        ENETDOWN = ZHelpers.HAUSNUMERO + 4,
        EADDRINUSE = ZHelpers.HAUSNUMERO + 5,
        EADDRNOTAVAIL = ZHelpers.HAUSNUMERO + 6,
        ECONNREFUSED = ZHelpers.HAUSNUMERO + 7,
        EINPROGRESS = ZHelpers.HAUSNUMERO + 8,
        ENOTSOCK = ZHelpers.HAUSNUMERO + 9,
        EFSM = ZHelpers.HAUSNUMERO + 51,
        ENOCOMPATPROTO = ZHelpers.HAUSNUMERO + 52,
        ETERM = ZHelpers.HAUSNUMERO + 53,
        EMTHREAD = ZHelpers.HAUSNUMERO + 54
    }
}
