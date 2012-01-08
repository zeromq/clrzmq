namespace ZeroMQ
{
    internal enum SocketOption
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
        MAX_MSG_SIZE = 22,
        SNDHWM = 23,
        RCVHWM = 24,
        MULTICAST_HOPS = 25,
        RCVTIMEO = 27,
        SNDTIMEO = 28,
        IPV4_ONLY = 31,
    }
}