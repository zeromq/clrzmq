namespace ZeroMQ.Monitoring
{
    internal enum MonitorEvent
    {
        CONNECTED = 1,
        CONNECT_DELAYED = 2,
        CONNECT_RETRIED = 4,
        LISTENING = 8,
        BIND_FAILED = 16,
        ACCEPTED = 32,
        ACCEPT_FAILED = 64,
        CLOSED = 128,
        CLOSE_FAILED = 256,
        DISCONNECTED = 512,
    }
}