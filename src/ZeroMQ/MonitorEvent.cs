namespace ZeroMQ
{
    using System;

    [Flags]
    internal enum MonitorEvent
    {
        /// <summary>
        /// connection established
        /// </summary>
        CONNECTED = 1,

        /// <summary>
        /// synchronous connect failed, it's being polled
        /// </summary>
        CONNECT_DELAYED = 2,

        /// <summary>
        /// asynchronous connect / reconnection attempt
        /// </summary>
        CONNECT_RETRIED = 4,

        /// <summary>
        /// socket bound to an address, ready to accept connections
        /// </summary>
        LISTENING = 8,

        /// <summary>
        /// socket could not bind to an address
        /// </summary>
        BIND_FAILED = 16,

        /// <summary>
        /// connection accepted to bound interface
        /// </summary>
        ACCEPTED = 32,

        /// <summary>
        /// could not accept client connection
        /// </summary>
        ACCEPT_FAILED = 64,

        /// <summary>
        /// connection closed
        /// </summary>
        CLOSED = 128,

        /// <summary>
        /// connection couldn't be closed
        /// </summary>
        CLOSE_FAILED = 256,

        /// <summary>
        /// broken session
        /// </summary>
        DISCONNECTED = 512,
    }
}